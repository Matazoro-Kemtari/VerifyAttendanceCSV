namespace Wada.AttendanceTableService
{
    public interface IAttendanceTableRepository
    {
        AttendanceTable LoadMonth(Stream stream, int month);
    }
}
