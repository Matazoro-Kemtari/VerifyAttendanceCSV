using Microsoft.Extensions.Configuration;
using Wada.AOP.Logging;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.EmployeeAggregation;

namespace Wada.OrderDataBase
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly IConfiguration _configuration;

        public EmployeeRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Logging]
        public IEnumerable<Employee> FetchAll()
        {
            DBConfig dbConfig = new(_configuration);
            using var dbContext = new DBContext(dbConfig);

            return dbContext.Employees
                .Select(x => new Employee((uint)x.EmployeeNumber, x.Name, x.DepartmentID))
                .ToList();
        }

        [Logging]
        public Employee FetchEmployeeByEmployeeNumber(uint employeeNumber)
        {
            DBConfig dbConfig = new(_configuration);
            using var dbContext = new DBContext(dbConfig);

            try
            {
                return dbContext.Employees.Where(x => x.EmployeeNumber == employeeNumber)
                                          .Select(x => new Employee((uint)x.EmployeeNumber, x.Name, x.DepartmentID))
                                          .First();
            }
            catch (InvalidOperationException ex)
            {
                throw new AttendanceTableServiceException(
                    $"社員番号を確認してください 受注管理に登録されていません 社員番号: {employeeNumber}", ex);
            }
        }
    }
}
