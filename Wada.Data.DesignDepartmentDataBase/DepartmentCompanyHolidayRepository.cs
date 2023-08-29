using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.DepartmentCompanyHolidayAggregation;
using Wada.DataBase.EFCore.DesignDepartment;

namespace Wada.Data.DesignDepartmentDataBase;

public class DepartmentCompanyHolidayRepository : IDepartmentCompanyHolidayRepository
{
    private readonly IConfiguration _configuration;

    public DepartmentCompanyHolidayRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<DepartmentCompanyHoliday> FindByDepartmentIdAsync(uint departmentId)
    {
        using var dbContext = new DesignDepartmentContext(_configuration);
        try
        {
            return await dbContext.DepartmentCompanyHolidays
                .Where(x => x.DepartmentId == departmentId)
                .Select(x => DepartmentCompanyHoliday.Reconstruct(
                    (uint)x.DepartmentId, x.CalendarGroupId))
                .SingleAsync();
        }
        catch (InvalidOperationException ex)
        {
            throw new DepartmentCompanyHolidayException(
                "所属カレンダーグループに該当がありませんでした" +
                $" 部署ID: {departmentId}", ex);
        }
    }
}