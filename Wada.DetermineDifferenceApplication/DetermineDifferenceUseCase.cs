using NLog;
using System.Text.RegularExpressions;
using Wada.AOP.Logging;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.WorkingMonthlyReportAggregation;
using Wada.Data.DesignDepartmentDataBase.Models;
using Wada.Data.DesignDepartmentDataBase.Models.MatchedEmployeeNumberAggregation;
using Wada.Data.OrderManagement.Models;
using Wada.Data.OrderManagement.Models.EmployeeAggregation;
using Wada.Extensions;

namespace Wada.DetermineDifferenceApplication;

public interface IDetermineDifferenceUseCase
{
    /// <summary>
    /// 勤怠CSVと勤務表エクセルの差異を判断する
    /// </summary>
    /// <param name="csvPath"></param>
    /// <param name="attendanceTableDirectories"></param>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <returns></returns>
    Task<DetermineDifferenceUseCaseDTO> ExecuteAsync(string csvPath, IEnumerable<string> attendanceTableDirectories, DateTime targetDate);
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
    private readonly IFileStreamOpener _streamOpener;
    private readonly IMatchedEmployeeNumberRepository _matchedEmployeeNumberRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeeAttendanceCsvReader _employeeAttendanceCsvReader;
    private readonly IAttendanceTableRepository _attendanceTableRepository;

    public DetermineDifferenceUseCase(ILogger logger,
                                      IStreamReaderOpener streamReaderOpener,
                                      IFileStreamOpener streamOpener,
                                      IMatchedEmployeeNumberRepository matchedEmployeeNumberRepository,
                                      IEmployeeRepository employeeRepository,
                                      IEmployeeAttendanceCsvReader employeeAttendanceCsvReader,
                                      IAttendanceTableRepository attendanceTableRepository)
    {
        _logger = logger;
        _streamReaderOpener = streamReaderOpener;
        _streamOpener = streamOpener;
        _matchedEmployeeNumberRepository = matchedEmployeeNumberRepository;
        _employeeRepository = employeeRepository;
        _employeeAttendanceCsvReader = employeeAttendanceCsvReader;
        _attendanceTableRepository = attendanceTableRepository;
    }

    [Logging]
    public async Task<DetermineDifferenceUseCaseDTO> ExecuteAsync(string csvPath, IEnumerable<string> attendanceTableDirectories, DateTime targetDate)
    {
        // CSVを取得する
        var csvReports = await ReadAllAttendanceCsvAsync(csvPath);

        // 社員番号対応表を取得する
        var employeeComparisons = await _matchedEmployeeNumberRepository.FindAllAsync()
            ?? throw new UseCaseException("社員番号対応表が取得できませんでした システム担当まで連絡してください");

        // S社員を取得する
        var employees = await _employeeRepository.FindAllAsync();

        // 勤務表を開く
        var xlsReports = await Task.WhenAll(
            ReadSpreadSheetsAsync(attendanceTableDirectories, targetDate, employeeComparisons));

        // 差分確認
        var unionDifferentialReports = GetDifference(csvReports, xlsReports);

        // 氏名を付加
        var differentialReportsWithName = AttachEmployeeName(unionDifferentialReports, employeeComparisons, employees);

        List<DetermineDifferenceEmployeesDTO> differences = differentialReportsWithName
            .Select(item => new
            {
                item,
                differentialMsgs = AttachDifferentialMessage(item)
            })
            .Where(x => x.differentialMsgs.Any())
            .Select(x => new DetermineDifferenceEmployeesDTO(
                x.item.EmployeeNumber,
                x.item.AttendancePersonalCode,
                x.item.Name,
                x.differentialMsgs))
            .ToList();

        return new(csvReports.Count(), xlsReports.Length, differences);
    }

    private static IEnumerable<string> AttachDifferentialMessage(DifferentialReportWithEmployeeNameAttempt item)
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
        return differentialMsgs;
    }

    /// <summary>
    /// 勤務表を読み込む
    /// </summary>
    /// <param name="attendanceTableDirectories"></param>
    /// <param name="date"></param>
    /// <param name="employeeComparisons"></param>
    /// <returns></returns>
    private IEnumerable<Task<WorkedMonthlyReport>> ReadSpreadSheetsAsync(IEnumerable<string> attendanceTableDirectories, DateTime date, IEnumerable<MatchedEmployeeNumber>? employeeComparisons)
    {
        // 年度にする
        var fiscalYear = date.FiscalYear();
        // 勤怠表を取得する
        Regex spreadSheetName = new(@"(?<=\\)" + $"{fiscalYear}" + @"年度_(勤務表|工数記録)_.+\.xls[xm]");
        var taskXLSs = attendanceTableDirectories
            .Where(x => Directory.Exists(x))
            .Select(x => Directory.EnumerateFiles(x))
            .SelectMany(x => x)
            .Where(y => spreadSheetName.IsMatch(y))
            .Select(async y =>
            {
                Stream stream = await _streamOpener.OpenAsync(y);
                var tbl = _attendanceTableRepository.ReadByMonth(stream, date.Month);
                _logger.Trace($"ファイル読み込み完了 {y}, {tbl}");
                return WorkedMonthlyReport.CreateForAttendanceTable(tbl, id => MutchEmployeeCode(id, employeeComparisons));
            });
        return taskXLSs;
    }

    /// <summary>
    /// 社員番号と勤怠個人コードを照合する
    /// </summary>
    /// <param name="id"></param>
    /// <param name="employeeComparisons"></param>
    /// <returns></returns>
    /// <exception cref="EmployeeNumberNotFoundException"></exception>
    private static uint MutchEmployeeCode(uint id, IEnumerable<MatchedEmployeeNumber>? employeeComparisons)
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

    private static IEnumerable<DifferentialReportWithEmployeeNameAttempt> AttachEmployeeName(IEnumerable<DifferentialReportAttempt> unionDifferentialReports, IEnumerable<MatchedEmployeeNumber> employeeComparisons, IEnumerable<Employee> employees)
    {
        return unionDifferentialReports
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
            .SelectMany(x => x.Name, (x, e) => new DifferentialReportWithEmployeeNameAttempt
            (
                x.EmployeeNumber,
                x.AttendancePersonalCode,
                e?.Name ?? string.Empty,
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
                x.AnomalyHour
            ));
    }

    /// <summary>
    /// 二つの配列の差分を取得する
    /// Left Join/Right Join 双方実施する
    /// </summary>
    /// <param name="csvReports"></param>
    /// <param name="xlsReports"></param>
    /// <returns></returns>
    private static IEnumerable<DifferentialReportAttempt> GetDifference(IEnumerable<WorkedMonthlyReport> csvReports, IEnumerable<WorkedMonthlyReport> xlsReports)
    {
        var differentialCSVReports = csvReports
            .GroupJoin(xlsReports,
            c => c.AttendancePersonalCode,
            x => x.AttendancePersonalCode,
            (csv, xlsx) => new { csv, xlsx })
            .SelectMany(x => x.xlsx.DefaultIfEmpty(),
            (outer, xlsx) => new DifferentialReportAttempt
            (
                outer.csv.AttendancePersonalCode,
                outer.csv.AttendanceDay == xlsx?.AttendanceDay,
                outer.csv.HolidayWorkedDay == xlsx?.HolidayWorkedDay,
                outer.csv.PaidLeaveDay == xlsx?.PaidLeaveDay,
                outer.csv.AbsenceDay == xlsx?.AbsenceDay,
                outer.csv.TransferedAttendanceDay == xlsx?.TransferedAttendanceDay,
                outer.csv.PaidSpecialLeaveDay == xlsx?.PaidSpecialLeaveDay,
                outer.csv.LatenessTime == xlsx?.LatenessTime,
                outer.csv.EarlyLeaveTime == xlsx?.EarlyLeaveTime,
                outer.csv.BusinessSuspensionDay == xlsx?.BusinessSuspensionDay,
                outer.csv.EducationDay == xlsx?.EducationDay,
                outer.csv.RegularWorkedHour == xlsx?.RegularWorkedHour,
                outer.csv.OvertimeHour == xlsx?.OvertimeHour,
                outer.csv.LateNightWorkingHour == xlsx?.LateNightWorkingHour,
                outer.csv.LegalHolidayWorkedHour == xlsx?.LegalHolidayWorkedHour,
                outer.csv.RegularHolidayWorkedHour == xlsx?.RegularHolidayWorkedHour,
                (outer.csv.AnomalyHour == null ? 0 : outer.csv.AnomalyHour) == (xlsx?.AnomalyHour == null ? 0 : xlsx?.AnomalyHour)
            ));
        // 左右入れ替え
        var differentialXLSXReports = xlsReports
            .GroupJoin(csvReports,
            x => x.AttendancePersonalCode,
            c => c.AttendancePersonalCode,
            (xlsx, csv) => new { xlsx, csv })
            .SelectMany(x => x.csv.DefaultIfEmpty(),
            (outer, csv) => new DifferentialReportAttempt
            (
                outer.xlsx.AttendancePersonalCode,
                outer.xlsx.AttendanceDay == csv?.AttendanceDay,
                outer.xlsx.HolidayWorkedDay == csv?.HolidayWorkedDay,
                outer.xlsx.PaidLeaveDay == csv?.PaidLeaveDay,
                outer.xlsx.AbsenceDay == csv?.AbsenceDay,
                outer.xlsx.TransferedAttendanceDay == csv?.TransferedAttendanceDay,
                outer.xlsx.PaidSpecialLeaveDay == csv?.PaidSpecialLeaveDay,
                outer.xlsx.LatenessTime == csv?.LatenessTime,
                outer.xlsx.EarlyLeaveTime == csv?.EarlyLeaveTime,
                outer.xlsx.BusinessSuspensionDay == csv?.BusinessSuspensionDay,
                outer.xlsx.EducationDay == csv?.EducationDay,
                outer.xlsx.RegularWorkedHour == csv?.RegularWorkedHour,
                outer.xlsx.OvertimeHour == csv?.OvertimeHour,
                outer.xlsx.LateNightWorkingHour == csv?.LateNightWorkingHour,
                outer.xlsx.LegalHolidayWorkedHour == csv?.LegalHolidayWorkedHour,
                outer.xlsx.RegularHolidayWorkedHour == csv?.RegularHolidayWorkedHour,
                (outer.xlsx.AnomalyHour == null ? 0 : outer.xlsx.AnomalyHour) == (csv?.AnomalyHour == null ? 0 : csv?.AnomalyHour)
            ));
        // 結果を集合
        var unionDifferentialReports = differentialCSVReports.Union(differentialXLSXReports);
        return unionDifferentialReports;
    }

    private Task<IEnumerable<WorkedMonthlyReport>> ReadAllAttendanceCsvAsync(string csvPath)
    {
        StreamReader reader = _streamReaderOpener.Open(csvPath);
        return _employeeAttendanceCsvReader.ReadAllAsync(reader);
    }
}

internal record class DifferentialReportAttempt(
    uint AttendancePersonalCode,
    bool AttendanceDay,
    bool HolidayWorkedDay,
    bool PaidLeaveDay,
    bool AbsenceDay,
    bool TransferedAttendanceDay,
    bool PaidSpecialLeaveDay,
    bool LatenessTime,
    bool EarlyLeaveTime,
    bool BusinessSuspensionDay,
    bool EducationDay,
    bool RegularWorkedHour,
    bool OvertimeHour,
    bool LateNightWorkingHour,
    bool LegalHolidayWorkedHour,
    bool RegularHolidayWorkedHour,
    bool AnomalyHour);

internal record class DifferentialReportWithEmployeeNameAttempt(
    uint EmployeeNumber,
    uint AttendancePersonalCode,
    string Name,
    bool AttendanceDay,
    bool HolidayWorkedDay,
    bool PaidLeaveDay,
    bool AbsenceDay,
    bool TransferedAttendanceDay,
    bool PaidSpecialLeaveDay,
    bool LatenessTime,
    bool EarlyLeaveTime,
    bool BusinessSuspensionDay,
    bool EducationDay,
    bool RegularWorkedHour,
    bool OvertimeHour,
    bool LateNightWorkingHour,
    bool LegalHolidayWorkedHour,
    bool RegularHolidayWorkedHour,
    bool AnomalyHour);
