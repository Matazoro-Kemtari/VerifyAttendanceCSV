using CsvHelper;
using DetermineDifferenceApplication;
using NLog;
using System.Globalization;
using Wada.AttendanceCSV.Models;
using Wada.AttendanceTableService;

namespace Wada.AttendanceCSV
{
    public class EmployeeAttendanceRepository : IEmployeeAttendanceRepository
    {
        private readonly ILogger logger;

        public EmployeeAttendanceRepository(ILogger logger)
        {
            this.logger = logger;
        }

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
                logger.Error(msg);
                throw new DetermineDifferenceApplicationException(msg);
            }

            return employeeAttendanceCSVs.Select(x => x.ToDomainEntity())
                .Select(x => WorkedMonthlyReport.CreateForAttendanceCSV(x));
        }
    }
}
