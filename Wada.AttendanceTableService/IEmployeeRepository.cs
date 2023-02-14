using Wada.AttendanceTableService.EmployeeAggregation;

namespace Wada.AttendanceTableService
{
    public interface IEmployeeRepository
    {
        /// <summary>
        /// 社員情報を取得する
        /// </summary>
        IEnumerable<Employee> FetchAll();

        /// <summary>
        /// 社員情報を取得する
        /// </summary>
        /// <param name="employeeNumber"></param>
        /// <returns></returns>
        Employee FetchEmployeeByEmployeeNumber(uint employeeNumber);
    }
}
