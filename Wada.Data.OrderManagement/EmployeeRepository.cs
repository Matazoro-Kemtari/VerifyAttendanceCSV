using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.EmployeeAggregation;
using Wada.DataBase.EFCore.OrderManagement;

namespace Wada.Data.OrderManagement;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly IConfiguration _configuration;

    public EmployeeRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IEnumerable<Employee>> FindAllAsync()
    {
        using var dbContext = new OrderManagementContext(_configuration);
        return await dbContext.Employees.Select(
            x => Employee.Reconstruct((uint)x.EmployeeNumber,
                                      x.Name,
                                      (uint?)x.DepartmentID,
                                      (uint?)x.AchievementProcessId))
                         .ToListAsync();
    }

    public async Task<Employee> FindByEmployeeNumberAsync(uint employeeNumber)
    {
        using var dbContext = new OrderManagementContext(_configuration);
        try
        {
            return await dbContext.Employees.Where(x => x.EmployeeNumber == (int)employeeNumber)
                                            .Select(x => Employee.Reconstruct((uint)x.EmployeeNumber,
                                                                              x.Name,
                                                                              (uint?)x.DepartmentID,
                                                                              (uint?)x.AchievementProcessId))
                                            .FirstAsync();
        }
        catch (InvalidOperationException ex)
        {
            throw new EmployeeAggregationException(
                $"社員番号を確認してください 受注管理に登録されていません 社員番号: {employeeNumber}", ex);
        }
    }
}