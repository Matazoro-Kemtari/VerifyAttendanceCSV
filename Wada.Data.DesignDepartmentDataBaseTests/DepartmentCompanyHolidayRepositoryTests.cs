using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.DepartmentCompanyHolidayAggregation;

namespace Wada.Data.DesignDepartmentDataBase.Tests
{
    [TestClass()]
    public class DepartmentCompanyHolidayRepositoryTests
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
        public async Task 正常系_所属カレンダーグループが取得できること()
        {
            // given
            // when
            IDepartmentCompanyHolidayRepository repository = new DepartmentCompanyHolidayRepository(_configuration!);
            var actual = await repository.FindByDepartmentIdAsync(1u);

            // then
            var expected = "01GW8E3ENDPWX0FXW0788VR63J";
            Assert.AreEqual(expected, actual.CalendarGroupId);
        }

        [TestMethod()]
        public async Task 異常系_部署コードがないとき例外を返すこと()
        {
            // given
            // when
            IDepartmentCompanyHolidayRepository repository = new DepartmentCompanyHolidayRepository(_configuration!);
            Task target() => repository.FindByDepartmentIdAsync(0u);

            // then
            var ex = await Assert.ThrowsExceptionAsync<DepartmentCompanyHolidayException>(target);
            var message = "所属カレンダーグループに該当がありませんでした 部署ID: 0";
            Assert.AreEqual(message, ex.Message);
        }
    }
}