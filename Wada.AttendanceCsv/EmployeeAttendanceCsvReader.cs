using CsvHelper;
using System.Globalization;
using Wada.AOP.Logging;
using Wada.AttendanceCsv.Models;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.WorkingMonthlyReportAggregation;

namespace Wada.AttendanceCsv;

public class EmployeeAttendanceCsvReader : IEmployeeAttendanceCsvReader
{
    [Logging]
    public async Task<IEnumerable<WorkedMonthlyReport>> ReadAllAsync(StreamReader streamReader)
    {
        var config = new CsvHelper.Configuration.CsvConfiguration(new CultureInfo("ja-JP", false))
        {
            // ヘッダ無（デフォルトtrue）
            HasHeaderRecord = false,
            // 列が不足していても無視
            MissingFieldFound = _ => { },
        };

        using CsvReader csv = new(streamReader, config);
        List<EmployeeAttendanceCsv> employeeAttendanceCsvs = new();
        await foreach (var employeeAttendance in csv.GetRecordsAsync<EmployeeAttendanceCsv>())
        {
            employeeAttendanceCsvs.Add(employeeAttendance);
        }
        
        if (employeeAttendanceCsvs.Count == 0)
        {
            string msg = "CSVファイルにデータがありません";
            throw new DomainException(msg);
        }

        return employeeAttendanceCsvs
            .Where(x => x.AttendancePersonalCode != 1)
            .Where(x => x.AttendancePersonalCode != 2)
            .Where(x => x.AttendancePersonalCode != 52)
            .Select(x => x.ToDomainEntity())
            .Select(x => WorkedMonthlyReport.CreateForAttendanceCSV(x));
    }
}
