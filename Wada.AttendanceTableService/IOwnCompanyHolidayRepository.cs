using Wada.AttendanceTableService.OwnCompanyCalendarAggregation;
using Wada.AttendanceTableService.ValueObjects;

namespace Wada.AttendanceTableService;

public interface IOwnCompanyHolidayRepository
{
    Task<IEnumerable<OwnCompanyHoliday>> FindByYearMonthAsync(string calendarGroupId, int year, int month);

    Task AddRangeAsync(IEnumerable<OwnCompanyHoliday> ownCompanyHolidays);

    Task<IEnumerable<OwnCompanyHoliday>> FindByAfterDateAsync(string calendarGroupId, DateTime date);

    Task RemoveRangeAsync(IEnumerable<OwnCompanyHoliday> ownCompanyHolidays);
}
