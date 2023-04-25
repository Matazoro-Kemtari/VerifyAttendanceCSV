using Wada.AttendanceTableService.AttendanceTableAggregation;

namespace Wada.AttendanceTableService;

public interface IAttendanceTableRepository
{
    /// <summary>
    /// 指定した月の勤怠表を取得する
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="calendarGroupId"></param>
    /// <param name="month"></param>
    /// <returns></returns>
    Task<AttendanceTable> ReadByMonthAsync(Stream stream, int month);
}
