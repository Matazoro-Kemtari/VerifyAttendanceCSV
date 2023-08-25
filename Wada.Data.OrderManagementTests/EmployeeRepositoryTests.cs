using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.Data.OrderManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Wada.AttendanceTableService.EmployeeAggregation;
using Wada.AttendanceTableService;

namespace Wada.Data.OrderManagement.Tests
{
    [TestClass()]
    public class EmployeeRepositoryTests
    {
        private static IConfiguration? _configuration;

        [ClassInitialize]
        public static void ClassInit(TestContext _)
        {
            // NOTE: https://qiita.com/mima_ita/items/55394bcc851eb8b6dc24

            DotNetEnv.Env.Load(".env");
            _configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();
        }

        [TestMethod()]
        public async Task 正常系_社員情報が取得できること()
        {
            // given
            // when
            IEmployeeRepository employeeRepository = new EmployeeRepository(_configuration!);
            var allTask  =employeeRepository.FindAllAsync();
            var numberTask = employeeRepository.FindByEmployeeNumberAsync(4001u);
            await Task.WhenAll(allTask, numberTask);
            var empAll = allTask.Result;
            var employee = numberTask.Result;

            // then
            Assert.IsNotNull(empAll);
            Assert.IsNotNull(employee);
            Assert.AreEqual("本社　無人", employee.Name);
            Assert.AreEqual(4u, employee.DepartmentId);
            Assert.AreEqual(employee, empAll.Single(x => x.EmployeeNumber == employee.EmployeeNumber));
        }

        [TestMethod()]
        public async Task 異常系_該当社員がいないとき例外を返すこと()
        {
            // given
            // when
            IEmployeeRepository employeeRepository = new EmployeeRepository(_configuration!);
            Task target()
                => employeeRepository.FindByEmployeeNumberAsync(0u);

            // then
            var ex = await Assert.ThrowsExceptionAsync<EmployeeAggregationException>(target);
            var expected = "社員番号を確認してください 受注管理に登録されていません 社員番号: 0";
            Assert.AreEqual(expected, ex.Message);
        }
    }
}