using Wada.Data.DesignDepartmentDataBase.Models.OwnCompanyCalendarAggregation;

namespace Wada.AttendanceTableService;

public interface IOwnCompanyHolidayListReader
{
    /// <summary>
    /// 自社カレンダーを読み込む
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="calendarGroupName"></param>
    /// <returns></returns>
    Task<IEnumerable<OwnCompanyHoliday>> ReadAllAsync(Stream stream, string calendarGroupName);
}
