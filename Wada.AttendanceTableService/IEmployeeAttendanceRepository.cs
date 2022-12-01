using Wada.AttendanceTableService.WorkingMonthlyReportAggregation;

namespace Wada.AttendanceTableService
{
    public interface IEmployeeAttendanceRepository
    {
        IEnumerable<WorkedMonthlyReport> ReadAll(StreamReader streamReader);
    }
}
