using Wada.AttendanceTableService;
using Wada.AttendanceTableService.OwnCompanyCalendarAggregation;
using Wada.AttendanceTableService.ValueObjects;

namespace Wada.Data.DesignDepartmentDataBase;

public class OwnCompanyHolidayRepository : IOwnCompanyHolidayRepository
{
    public Task AddRangeAsync(IEnumerable<OwnCompanyHoliday> ownCompanyHolidays)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<OwnCompanyHoliday>> FindByAfterDateAsync(string calendarGroupId, DateTime date)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<OwnCompanyHoliday>> FindByYearMonthAsync(string calendarGroupId, int year, int month)
    {
        throw new NotImplementedException();
    }

    public Task<string> FindCalendarGroupIdAsync(CalendarGroupClassification calendarGroupClass)
    {
        throw new NotImplementedException();
    }

    public Task RemoveRangeAsync(IEnumerable<OwnCompanyHoliday> ownCompanyHolidays)
    {
        throw new NotImplementedException();
    }
}
