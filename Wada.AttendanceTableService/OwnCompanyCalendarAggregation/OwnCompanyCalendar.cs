using Wada.AttendanceTableService.ValueObjects;

namespace Wada.AttendanceTableService.OwnCompanyCalendarAggregation
{
    [Equals(DoNotAddEqualityOperators = true), ToString]
    public class OwnCompanyCalendar
    {
        public OwnCompanyCalendar(DateTime date, HolidayClassification holidayClassification)
        {
            Date = date;
            HolidayClassification = holidayClassification;
        }

        /// <summary>
        /// 日付
        /// </summary>
        public DateTime Date { get; init; }

        /// <summary>
        /// 休日区分
        /// </summary>
        [IgnoreDuringEquals]
        public HolidayClassification HolidayClassification { get; init; }
    }
}
