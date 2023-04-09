using NLog;
using System.Text.RegularExpressions;
using Wada.AOP.Logging;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.WorkingMonthlyReportAggregation;

// https://stackoverflow.com/questions/49648179/how-to-use-methoddecorator-fody-decorator-in-another-project
[module: Logging] // <- これ重要
namespace Wada.DetermineDifferenceApplication
{
    public interface IDetermineDifferenceUseCase
    {
        Task<DetermineDifferenceUseCaseDTO> ExecuteAsync(string csvPath, IEnumerable<string> attendanceTableDirectory, int year, int month);
    }

    public record class DetermineDifferenceUseCaseDTO(
        int CSVCount,
        int XLSXCount,
        IEnumerable<DetermineDifferenceEmployeesDTO> DetermineDifferenceEmployeesDTOs);

    public record class DetermineDifferenceEmployeesDTO(
        uint EmployeeNumber,
        uint AttendancePersonalCode,
        string EmployeeName,
        IEnumerable<string> Differences);

    public class DetermineDifferenceUseCase : IDetermineDifferenceUseCase
    {
        private readonly ILogger _logger;
        private readonly IStreamReaderOpener _streamReaderOpener;
        private readonly IStreamOpener _streamOpener;
        private readonly IMatchedEmployeeNumberRepository _matchedEmployeeNumberRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IEmployeeAttendanceRepository _employeeAttendanceRepository;
        private readonly IAttendanceTableRepository _attendanceTableRepository;

        public DetermineDifferenceUseCase(ILogger logger,
                                          IStreamReaderOpener streamReaderOpener,
                                          IStreamOpener streamOpener,
                                          IMatchedEmployeeNumberRepository matchedEmployeeNumberRepository,
                                          IEmployeeRepository employeeRepository,
                                          IEmployeeAttendanceRepository employeeAttendanceRepository,
                                          IAttendanceTableRepository attendanceTableRepository)
        {
            _logger = logger;
            _streamReaderOpener = streamReaderOpener;
            _streamOpener = streamOpener;
            _matchedEmployeeNumberRepository = matchedEmployeeNumberRepository;
            _employeeRepository = employeeRepository;
            _employeeAttendanceRepository = employeeAttendanceRepository;
            _attendanceTableRepository = attendanceTableRepository;
        }

        // 早期完成版だからと言って長すぎだ!
        [Logging]
        public async Task<DetermineDifferenceUseCaseDTO> ExecuteAsync(string csvPath, IEnumerable<string> attendanceTableDirectory, int year, int month)
        {
            // CSVを取得する
            StreamReader reader = _streamReaderOpener.Open(csvPath);
            Task<IEnumerable<WorkedMonthlyReport>> taskCSV = Task.Run(() => _employeeAttendanceRepository.ReadAll(reader));

            // 社員番号対応表を取得する
            var employeeComparisons = await Task.Run(() => _matchedEmployeeNumberRepository.FindAll());
            if (employeeComparisons == null)
                // TODO: ちゃんとThrowする
                throw new Exception();

            // メモリ上に展開しておいてから照合する関数
            uint mutchEmployee(uint id)
            {
                try
                {
                    return employeeComparisons!
                        .Single(x => x.EmployeeNumber == id)
                        !.AttendancePersonalCode;
                }
                catch (InvalidOperationException ex)
                {
                    string msg = $"社員番号対応表に該当がありません 社員番号: {id}";
                    throw new EmployeeNumberNotFoundException(msg, ex);
                }
            }

            // S社員を取得する
            var taskEmployee = Task.Run(() => _employeeRepository.FetchAll());

            // 年度にする
            var fiscalYear = month <= 3 ? year - 1 : year;
            // 勤怠表を取得する
            Regex spreadSheetName = new(@"(?<=\\)" + $"{fiscalYear}" + @"年度_(勤務表|工数記録)_.+\.xls[xm]");
            IEnumerable<Task<WorkedMonthlyReport>> taskXLSs = attendanceTableDirectory
                .Where(x => Directory.Exists(x))
                .Select(x => Directory.EnumerateFiles(x))
                .SelectMany(x => x)
                .Where(y => spreadSheetName.IsMatch(y))
                .Select(y =>
                {
                    return Task.Run(() =>
                    {
                        Stream stream = _streamOpener.Open(y);
                        var tbl = _attendanceTableRepository.ReadByMonth(stream, month);
                        _logger.Trace($"ファイル読み込み完了 {y}, {tbl}");
                        return WorkedMonthlyReport.CreateForAttendanceTable(tbl, mutchEmployee);
                    });
                });

            var employees = await taskEmployee;
            IEnumerable<WorkedMonthlyReport> csvReports = await taskCSV;
            IEnumerable<WorkedMonthlyReport> xlsReports = await Task.WhenAll(taskXLSs);

            // 差分確認
            var differentialCSVReports = csvReports
                .GroupJoin(xlsReports,
                c => c.AttendancePersonalCode,
                x => x.AttendancePersonalCode,
                (csv, xlsx) => new { csv, xlsx })
                .SelectMany(x => x.xlsx.DefaultIfEmpty(),
                (outer, xlsx) => new
                {
                    outer.csv.AttendancePersonalCode,
                    AttendanceDay = outer.csv.AttendanceDay == xlsx?.AttendanceDay,
                    HolidayWorkedDay = outer.csv.HolidayWorkedDay == xlsx?.HolidayWorkedDay,
                    PaidLeaveDay = outer.csv.PaidLeaveDay == xlsx?.PaidLeaveDay,
                    AbsenceDay = outer.csv.AbsenceDay == xlsx?.AbsenceDay,
                    TransferedAttendanceDay = outer.csv.TransferedAttendanceDay == xlsx?.TransferedAttendanceDay,
                    PaidSpecialLeaveDay = outer.csv.PaidSpecialLeaveDay == xlsx?.PaidSpecialLeaveDay,
                    LatenessTime = outer.csv.LatenessTime == xlsx?.LatenessTime,
                    EarlyLeaveTime = outer.csv.EarlyLeaveTime == xlsx?.EarlyLeaveTime,
                    BusinessSuspensionDay = outer.csv.BusinessSuspensionDay == xlsx?.BusinessSuspensionDay,
                    EducationDay = outer.csv.EducationDay == xlsx?.EducationDay,
                    RegularWorkedHour = outer.csv.RegularWorkedHour == xlsx?.RegularWorkedHour,
                    OvertimeHour = outer.csv.OvertimeHour == xlsx?.OvertimeHour,
                    LateNightWorkingHour = outer.csv.LateNightWorkingHour == xlsx?.LateNightWorkingHour,
                    LegalHolidayWorkedHour = outer.csv.LegalHolidayWorkedHour == xlsx?.LegalHolidayWorkedHour,
                    RegularHolidayWorkedHour = outer.csv.RegularHolidayWorkedHour == xlsx?.RegularHolidayWorkedHour,
                    AnomalyHour = (outer.csv.AnomalyHour == null ? 0 : outer.csv.AnomalyHour) == (xlsx?.AnomalyHour == null ? 0 : xlsx?.AnomalyHour),
                });
            // 左右入れ替え
            var differentialXLSXReports = xlsReports
                .GroupJoin(csvReports,
                x => x.AttendancePersonalCode,
                c => c.AttendancePersonalCode,
                (xlsx, csv) => new { xlsx, csv })
                .SelectMany(x => x.csv.DefaultIfEmpty(),
                (outer, csv) => new
                {
                    outer.xlsx.AttendancePersonalCode,
                    AttendanceDay = outer.xlsx.AttendanceDay == csv?.AttendanceDay,
                    HolidayWorkedDay = outer.xlsx.HolidayWorkedDay == csv?.HolidayWorkedDay,
                    PaidLeaveDay = outer.xlsx.PaidLeaveDay == csv?.PaidLeaveDay,
                    AbsenceDay = outer.xlsx.AbsenceDay == csv?.AbsenceDay,
                    TransferedAttendanceDay = outer.xlsx.TransferedAttendanceDay == csv?.TransferedAttendanceDay,
                    PaidSpecialLeaveDay = outer.xlsx.PaidSpecialLeaveDay == csv?.PaidSpecialLeaveDay,
                    LatenessTime = outer.xlsx.LatenessTime == csv?.LatenessTime,
                    EarlyLeaveTime = outer.xlsx.EarlyLeaveTime == csv?.EarlyLeaveTime,
                    BusinessSuspensionDay = outer.xlsx.BusinessSuspensionDay == csv?.BusinessSuspensionDay,
                    EducationDay = outer.xlsx.EducationDay == csv?.EducationDay,
                    RegularWorkedHour = outer.xlsx.RegularWorkedHour == csv?.RegularWorkedHour,
                    OvertimeHour = outer.xlsx.OvertimeHour == csv?.OvertimeHour,
                    LateNightWorkingHour = outer.xlsx.LateNightWorkingHour == csv?.LateNightWorkingHour,
                    LegalHolidayWorkedHour = outer.xlsx.LegalHolidayWorkedHour == csv?.LegalHolidayWorkedHour,
                    RegularHolidayWorkedHour = outer.xlsx.RegularHolidayWorkedHour == csv?.RegularHolidayWorkedHour,
                    AnomalyHour = (outer.xlsx.AnomalyHour == null ? 0 : outer.xlsx.AnomalyHour) == (csv?.AnomalyHour == null ? 0 : csv?.AnomalyHour),
                });
            // 結果を集合
            var unionDifferentialReports = differentialCSVReports.Union(differentialXLSXReports);

            // 氏名を付加
            var differentialReportsWithName =
                unionDifferentialReports
                .Join(
                    employeeComparisons,
                    u => u.AttendancePersonalCode,
                    c => c.AttendancePersonalCode,
                    (u, c) => new
                    {
                        c.EmployeeNumber,
                        u.AttendancePersonalCode,
                        u.AttendanceDay,
                        u.HolidayWorkedDay,
                        u.PaidLeaveDay,
                        u.AbsenceDay,
                        u.TransferedAttendanceDay,
                        u.PaidSpecialLeaveDay,
                        u.LatenessTime,
                        u.EarlyLeaveTime,
                        u.BusinessSuspensionDay,
                        u.EducationDay,
                        u.RegularWorkedHour,
                        u.OvertimeHour,
                        u.LateNightWorkingHour,
                        u.LegalHolidayWorkedHour,
                        u.RegularHolidayWorkedHour,
                        u.AnomalyHour,
                    })
                .GroupJoin(
                    employees,
                    u => u.EmployeeNumber,
                    e => e.EmployeeNumber,
                    (u, e) => new
                    {
                        u.EmployeeNumber,
                        u.AttendancePersonalCode,
                        Name = e.DefaultIfEmpty(),
                        u.AttendanceDay,
                        u.HolidayWorkedDay,
                        u.PaidLeaveDay,
                        u.AbsenceDay,
                        u.TransferedAttendanceDay,
                        u.PaidSpecialLeaveDay,
                        u.LatenessTime,
                        u.EarlyLeaveTime,
                        u.BusinessSuspensionDay,
                        u.EducationDay,
                        u.RegularWorkedHour,
                        u.OvertimeHour,
                        u.LateNightWorkingHour,
                        u.LegalHolidayWorkedHour,
                        u.RegularHolidayWorkedHour,
                        u.AnomalyHour,
                    })
                .SelectMany(x=> x.Name, (x,e)=>new
                {
                    x.EmployeeNumber,
                    x.AttendancePersonalCode,
                    Name = e?.Name ?? string.Empty,
                    x.AttendanceDay,
                    x.HolidayWorkedDay,
                    x.PaidLeaveDay,
                    x.AbsenceDay,
                    x.TransferedAttendanceDay,
                    x.PaidSpecialLeaveDay,
                    x.LatenessTime,
                    x.EarlyLeaveTime,
                    x.BusinessSuspensionDay,
                    x.EducationDay,
                    x.RegularWorkedHour,
                    x.OvertimeHour,
                    x.LateNightWorkingHour,
                    x.LegalHolidayWorkedHour,
                    x.RegularHolidayWorkedHour,
                    x.AnomalyHour,
                });

            List<DetermineDifferenceEmployeesDTO> differences = new();
            foreach (var item in differentialReportsWithName)
            {
                List<string> differentialMsgs = new();
                if (!item.AttendanceDay)
                    differentialMsgs.Add("出勤日数");
                if (!item.HolidayWorkedDay)
                    differentialMsgs.Add("休日出勤数");
                if (!item.PaidLeaveDay)
                    differentialMsgs.Add("有休日数");
                if (!item.AbsenceDay)
                    differentialMsgs.Add("欠勤日数");
                if (!item.TransferedAttendanceDay)
                    differentialMsgs.Add("振休出勤日数");
                if (!item.PaidSpecialLeaveDay)
                    differentialMsgs.Add("有休特別休暇(特A)日数");
                if (!item.LatenessTime)
                    differentialMsgs.Add("遅刻回数");
                if (!item.EarlyLeaveTime)
                    differentialMsgs.Add("早退回数");
                if (!item.BusinessSuspensionDay)
                    differentialMsgs.Add("休業日数");
                // 教育日数の差分は無視する
                if (!item.RegularWorkedHour)
                    differentialMsgs.Add("所定時間");
                if (!item.OvertimeHour)
                    differentialMsgs.Add("(早出)残業時間");
                if (!item.LateNightWorkingHour)
                    differentialMsgs.Add("深夜勤務時間");
                if (!item.LegalHolidayWorkedHour)
                    differentialMsgs.Add("法定休出勤時間");
                if (!item.RegularHolidayWorkedHour)
                    differentialMsgs.Add("法定外休出勤時間");
                if (!item.AnomalyHour)
                    differentialMsgs.Add("変則時間");

                if (differentialMsgs.Count > 0)
                {
                    differences.Add(
                        new(item.EmployeeNumber,
                            item.AttendancePersonalCode,
                            item.Name,
                            differentialMsgs));
                }
            }

            return new(csvReports.Count(), xlsReports.Count(), differences);
        }
    }
}
