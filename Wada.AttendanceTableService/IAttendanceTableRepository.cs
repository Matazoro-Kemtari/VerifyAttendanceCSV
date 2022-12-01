using Wada.AttendanceTableService.AttendanceTableAggregation;

namespace Wada.AttendanceTableService
{
    public interface IAttendanceTableRepository
    {
        AttendanceTable ReadByMonth(Stream stream, int month);
    }
}
