using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wada.AOP.Logging;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.WorkingMonthlyReportAggregation;

// https://stackoverflow.com/questions/49648179/how-to-use-methoddecorator-fody-decorator-in-another-project
[module: Logging] // <- これ重要
namespace DetermineDifferenceApplication
{
    public interface IDetermineDifferenceUseCase
    {
        Task<(int csvCount, int xlsxCount, Dictionary<uint, List<string>> differntialMaps)> ExecuteAsync(string csvPath, IEnumerable<string> attendanceTableDirectory, int year, int month);
    }

    public class DetermineDifferenceUseCase : IDetermineDifferenceUseCase
    {
        private readonly IStreamReaderOpener streamReaderOpener;
        private readonly IStreamOpener streamOpener;
        private readonly IMatchedEmployeeNumberRepository matchedEmployeeNumberRepository;
        private readonly IEmployeeAttendanceRepository employeeAttendanceRepository;
        private readonly IAttendanceTableRepository attendanceTableRepository;

        public DetermineDifferenceUseCase(IStreamReaderOpener streamReaderOpener, IStreamOpener streamOpener, IMatchedEmployeeNumberRepository matchedEmployeeNumberRepository, IEmployeeAttendanceRepository employeeAttendanceRepository, IAttendanceTableRepository attendanceTableRepository)
        {
            this.streamReaderOpener = streamReaderOpener;
            this.streamOpener = streamOpener;
            this.matchedEmployeeNumberRepository = matchedEmployeeNumberRepository;
            this.employeeAttendanceRepository = employeeAttendanceRepository;
            this.attendanceTableRepository = attendanceTableRepository;
        }

        [Logging]
        public async Task<(int csvCount, int xlsxCount, Dictionary<uint, List<string>> differntialMaps)> ExecuteAsync(string csvPath, IEnumerable<string> attendanceTableDirectory, int year, int month)
        {
            // CSVを取得する
            StreamReader reader = streamReaderOpener.Open(csvPath);
            Task<IEnumerable<WorkedMonthlyReport>> taskCSV = Task.Run(() => employeeAttendanceRepository.ReadAll(reader));

            // 社員番号対応表を取得する
            var employees = matchedEmployeeNumberRepository.FindAll();
            if (employees == null)
                throw new Exception();
            // メモリ上に展開しておいてから照合する関数
            uint mutchEmployee(uint id)
            {
                try
                {
                    return employees!
                        .Single(x => x.EmployeeNumber == id)
                        !.AttendancePersonalCode;
                }
                catch (InvalidOperationException ex)
                {
                    string msg = $"社員番号対応表に該当がありません 社員番号: {id}";
                    throw new EmployeeNumberNotFoundException(msg, ex);
                }
            }

            // 勤怠表を取得する
            Regex spreadSheetName = new($"{year}" + @"年度_(勤務表|工数記録)_.+\.xls[xm]");
            var geko = attendanceTableDirectory
                .Where(x => Directory.Exists(x))
                .Select(x => Directory.EnumerateFiles(x))
                .SelectMany(x => x);
            IEnumerable<Task<WorkedMonthlyReport>> taskXLSs = attendanceTableDirectory
                .Where(x => Directory.Exists(x))
                .Select(x => Directory.EnumerateFiles(x))
                .SelectMany(x => x)
                .Where(y => spreadSheetName.IsMatch(y))
                .Select(y =>
                {
                    return Task.Run(() =>
                    {
                        Stream stream = streamOpener.Open(y);
                        var tbl = attendanceTableRepository.ReadByMonth(stream, month);
                        return WorkedMonthlyReport.CreateForAttendanceTable(tbl, mutchEmployee);
                    });
                });

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
                    AnomalyHour = outer.csv.AnomalyHour == xlsx?.AnomalyHour,
                    LunchBoxOrderedTime = outer.csv.LunchBoxOrderedTime == xlsx?.LunchBoxOrderedTime,
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
                    AnomalyHour = outer.xlsx.AnomalyHour == csv?.AnomalyHour,
                    LunchBoxOrderedTime = outer.xlsx.LunchBoxOrderedTime == csv?.LunchBoxOrderedTime,
                });
            // 結果を集合
            var unionDifferentialReports = differentialCSVReports.Union(differentialXLSXReports);

            Dictionary<uint, List<string>> differntialMaps = new();
            foreach (var item in unionDifferentialReports)
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
                if (!item.EducationDay)
                    differentialMsgs.Add("教育日数");
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
                if (!item.LunchBoxOrderedTime)
                    differentialMsgs.Add("弁当注文数");

                if (differentialMsgs.Count > 0)
                    differntialMaps.Add(item.AttendancePersonalCode, differentialMsgs);
            }

            return (csvReports.Count(), xlsReports.Count(), differntialMaps);
        }
    }
}
