using Wada.AttendanceTableService.EmployeeAggregation;

namespace Wada.AttendanceTableService;

public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> FindAllAsync();

    Task<Employee> FindByEmployeeNumberAsync(uint employeeNumber);
}
