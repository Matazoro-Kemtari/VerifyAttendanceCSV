using Wada.AttendanceTableService.DepartmentCompanyHolidayAggregation;

namespace Wada.AttendanceTableService;

public interface IDepartmentCompanyHolidayRepository
{
    Task<DepartmentCompanyHoliday> FindByDepartmentIdAsync(uint departmentId);
}
