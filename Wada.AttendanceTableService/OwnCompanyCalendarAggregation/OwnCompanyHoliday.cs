using Wada.AttendanceTableService.ValueObjects;

namespace Wada.AttendanceTableService.OwnCompanyCalendarAggregation
{
    [Equals(DoNotAddEqualityOperators = true), ToString]
    public class OwnCompanyHoliday
    {
        private OwnCompanyHoliday(DateTime holidayDate, HolidayClassification holidayClassification)
        {
            HolidayDate = holidayDate;
            HolidayClassification = holidayClassification;
        }

        public static OwnCompanyHoliday ReConstruct(DateTime holidayDate, HolidayClassification holidayClassification) => new(holidayDate, holidayClassification);

        /// <summary>
        /// 日付
        /// </summary>
        public DateTime HolidayDate { get; init; }

        /// <summary>
        /// 休日区分
        /// </summary>
        [IgnoreDuringEquals]
        public HolidayClassification HolidayClassification { get; init; }
    }
}
