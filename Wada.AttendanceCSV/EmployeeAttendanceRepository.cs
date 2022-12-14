using CsvHelper;
using System.Globalization;
using Wada.AOP.Logging;
using Wada.AttendanceCSV.Models;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.WorkingMonthlyReportAggregation;

[module: Logging] // https://stackoverflow.com/questions/49648179/how-to-use-methoddecorator-fody-decorator-in-another-project
namespace Wada.AttendanceCSV
{
    public class EmployeeAttendanceRepository : IEmployeeAttendanceRepository
    {
        [Logging]
        public IEnumerable<WorkedMonthlyReport> ReadAll(StreamReader streamReader)
        {
            var config = new CsvHelper.Configuration.CsvConfiguration(new CultureInfo("ja-JP", false))
            {
                //ヘッダ無（デフォルトtrue）
                HasHeaderRecord = false,
            };
            
            using CsvReader csv = new(streamReader, config);
            List<EmployeeAttendanceCSV> employeeAttendanceCSVs =
                csv.GetRecords<EmployeeAttendanceCSV>().ToList();
            if (employeeAttendanceCSVs.Count == 0)
            {
                string msg = "CSVファイルにデータがありません";
                throw new AttendanceTableServiceException(msg);
            }

            return employeeAttendanceCSVs.Select(x => x.ToDomainEntity())
                .Select(x => WorkedMonthlyReport.CreateForAttendanceCSV(x));
        }
    }
}
