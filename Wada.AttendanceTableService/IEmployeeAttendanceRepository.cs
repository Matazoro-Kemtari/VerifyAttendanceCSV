using Wada.AttendanceTableService.WorkingMonthlyReportAggregation;

namespace Wada.AttendanceTableService
{
    public interface IEmployeeAttendanceRepository
    {
        /// <summary>
        /// 全ての就業月報を取得する
        /// </summary>
        /// <param name="streamReader"></param>
        /// <returns></returns>
        IEnumerable<WorkedMonthlyReport> ReadAll(StreamReader streamReader);
    }
}
