using Wada.AttendanceTableService.OwnCompanyCalendarAggregation;

namespace Wada.AttendanceTableService
{
    public interface IOwnCompanyCalendarRepository
    {
        /// <summary>
        /// 指定した年月のカレンダーを取得する
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        IEnumerable<OwnCompanyCalendar> FindByYearMonth(int year, int month);

        /// <summary>
        /// 登録されたカレンダーの最終日付を取得する
        /// </summary>
        /// <returns></returns>
        DateTime MaxDate();
    }
}
