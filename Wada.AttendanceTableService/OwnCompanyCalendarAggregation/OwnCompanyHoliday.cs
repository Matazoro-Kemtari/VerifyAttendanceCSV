using Wada.AttendanceTableService.ValueObjects;

namespace Wada.AttendanceTableService.OwnCompanyCalendarAggregation;

public record class OwnCompanyHoliday
{
    private OwnCompanyHoliday(string calendarGroupId, DateTime holidayDate, HolidayClassification holidayClassification)
    {
        CalendarGroupId = calendarGroupId;
        HolidayDate = holidayDate;
        HolidayClassification = holidayClassification;
    }

    /// <summary>
    /// ファクトリメソッド
    /// </summary>
    /// <param name="calendarGroupId"></param>
    /// <param name="holidayDate"></param>
    /// <param name="holidayClassification"></param>
    /// <returns></returns>
    public static OwnCompanyHoliday Create(string calendarGroupId,
                                                DateTime holidayDate,
                                                HolidayClassification holidayClassification)
        => new(calendarGroupId, holidayDate, holidayClassification);

    /// <summary>
    /// インフラ層専用再構築メソッド
    /// </summary>
    /// <param name="calendarGroupId"></param>
    /// <param name="holidayDate"></param>
    /// <param name="holidayClassification"></param>
    /// <returns></returns>
    public static OwnCompanyHoliday Reconstruct(string calendarGroupId,
                                                DateTime holidayDate,
                                                HolidayClassification holidayClassification)
        => Create(calendarGroupId, holidayDate, holidayClassification);

    public static string GetCalendarGroupId(CalendarGroupClassification calendarGroupClass)
        => calendarGroupClass switch
        {
            CalendarGroupClassification.HeadOffice => "01GW8E3ENDPWX0FXW0788VR63J",
            CalendarGroupClassification.MatsuzakaOffice => "01GW8Y1FA5XFHY027KZY4WX6KW",
            _ => throw new OwnCompanyCalendarAggregationException(
                $"引数calendarGroupNameに{calendarGroupClass}が渡されました")
        };

    /// <summary>
    /// カレンダーグループID
    /// </summary>
    public string CalendarGroupId { get; }

    /// <summary>
    /// 日付
    /// </summary>
    public DateTime HolidayDate { get; }

    /// <summary>
    /// 休日区分
    /// </summary>
    public HolidayClassification HolidayClassification { get; }
}

public class TestOwnCompanyHolidayFactory
{
    public static OwnCompanyHoliday Create(string calendarGroupId = "__Dummy__",
                                           DateTime? holidayDate = default,
                                           HolidayClassification holidayClassification = default)
    {
        holidayDate ??= DateTime.Now.Date;
        return OwnCompanyHoliday.Reconstruct(calendarGroupId, holidayDate.Value, holidayClassification);
    }
}
