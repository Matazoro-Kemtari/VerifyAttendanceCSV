using Wada.AttendanceTableService.OwnCompanyCalendarAggregation;

namespace Wada.AttendanceTableService
{
    public interface IOwnCompanyHolidayRepository
    {
        /// <summary>
        /// 指定した年月のカレンダーを取得する
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        IEnumerable<OwnCompanyHoliday> FindByYearMonth(int year, int month);

        /// <summary>
        /// 登録されたカレンダーの最終日付を取得する
        /// </summary>
        /// <returns></returns>
        DateTime MaxDate();

        /// <summary>
        /// 複数のカレンダーを追加する
        /// </summary>
        /// <param name="ownCompanyHolidays"></param>
        void AddRange(IEnumerable<OwnCompanyHoliday> ownCompanyHolidays);
    }
}
