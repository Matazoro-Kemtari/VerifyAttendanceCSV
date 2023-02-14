using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.AttendanceTableService;

namespace Wada.OrderDataBase.Tests
{
    [TestClass()]
    public class EmployeeRepositoryTests
    {
        private static IConfiguration? _configuration;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            // NOTE: https://qiita.com/mima_ita/items/55394bcc851eb8b6dc24

            DotNetEnv.Env.Load(".env");
            _configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();
        }

        [TestMethod()]
        public void 正常系_社員情報が取得できること()
        {
            // given
            // when
            IEmployeeRepository employeeRepository = new EmployeeRepository(_configuration!);
            var employee = employeeRepository.FetchEmployeeByEmployeeNumber(4001u);

            // then
            Assert.IsNotNull(employee);
            Assert.AreEqual("本社　無人", employee.Name);
            Assert.AreEqual(4, employee.DepartmentID);
        }

        [TestMethod()]
        public void 異常系_該当社員がいないとき例外を返すこと()
        {
            // given
            // when
            IEmployeeRepository employeeRepository = new EmployeeRepository(_configuration!);
            void target()
            {
                _ = employeeRepository.FetchEmployeeByEmployeeNumber(0u);
            }

            // then
            var ex = Assert.ThrowsException<AttendanceTableServiceException>(target);
            var expected = "社員番号を確認してください 受注管理に登録されていません 社員番号: 0";
            Assert.AreEqual(expected, ex.Message);
        }
    }
}