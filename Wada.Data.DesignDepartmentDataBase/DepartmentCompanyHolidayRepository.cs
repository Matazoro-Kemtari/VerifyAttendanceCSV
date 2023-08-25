using Wada.AttendanceTableService;
using Wada.AttendanceTableService.DepartmentCompanyHolidayAggregation;

namespace Wada.Data.DesignDepartmentDataBase;

public class DepartmentCompanyHolidayRepository : IDepartmentCompanyHolidayRepository
{
    public Task<DepartmentCompanyHoliday> FindByDepartmentIdAsync(uint departmentId)
    {
        throw new NotImplementedException();
    }
}