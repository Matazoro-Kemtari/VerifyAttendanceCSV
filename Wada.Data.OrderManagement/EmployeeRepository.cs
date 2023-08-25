using Wada.AttendanceTableService;
using Wada.AttendanceTableService.EmployeeAggregation;

namespace Wada.Data.OrderManagement;

public class EmployeeRepository : IEmployeeRepository
{
    public Task<IEnumerable<Employee>> FindAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Employee> FindByEmployeeNumberAsync(uint employeeNumber)
    {
        throw new NotImplementedException();
    }
}