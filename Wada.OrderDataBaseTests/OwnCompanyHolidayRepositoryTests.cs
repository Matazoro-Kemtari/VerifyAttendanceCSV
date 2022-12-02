using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.OrderDataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NLog;
using Microsoft.Extensions.Configuration;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.OwnCompanyCalendarAggregation;
using Wada.AttendanceTableService.ValueObjects;

namespace Wada.OrderDataBase.Tests
{
    [TestClass()]
    public class OwnCompanyHolidayRepositoryTests
    {
        static ILogger? mock_logger;
        static IConfiguration? configuration;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            // NOTE: https://qiita.com/mima_ita/items/55394bcc851eb8b6dc24

            mock_logger = new Mock<ILogger>().Object;

            DotNetEnv.Env.Load(".env");
            configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();
        }

        [TestMethod()]
        public void 正常系_指定年月の自社カレンダーを取得できること()
        {
            // given
            ILogger mock_logger = OwnCompanyHolidayRepositoryTests.mock_logger!;
            IConfiguration configuration = OwnCompanyHolidayRepositoryTests.configuration!;

            // when
            IOwnCompanyHolidayRepository repository = new OwnCompanyHolidayRepository(mock_logger, configuration);
            var actuals = repository.FindByYearMonth(2023, 1);

            // then
            var expecteds = new List<OwnCompanyHoliday>
            {
                new(DateTime.Parse("2023/01/01"), HolidayClassification.LegalHoliday),
                new(DateTime.Parse("2023/01/02"), HolidayClassification.RegularHoliday),
                new(DateTime.Parse("2023/01/03"), HolidayClassification.RegularHoliday),
                new(DateTime.Parse("2023/01/04"), HolidayClassification.RegularHoliday),
                new(DateTime.Parse("2023/01/07"), HolidayClassification.RegularHoliday),
                new(DateTime.Parse("2023/01/08"), HolidayClassification.LegalHoliday),
                new(DateTime.Parse("2023/01/09"), HolidayClassification.RegularHoliday),
                new(DateTime.Parse("2023/01/14"), HolidayClassification.RegularHoliday),
                new(DateTime.Parse("2023/01/15"), HolidayClassification.LegalHoliday),
                new(DateTime.Parse("2023/01/21"), HolidayClassification.RegularHoliday),
                new(DateTime.Parse("2023/01/22"), HolidayClassification.LegalHoliday),
                new(DateTime.Parse("2023/01/28"), HolidayClassification.RegularHoliday),
                new(DateTime.Parse("2023/01/29"), HolidayClassification.LegalHoliday),
            };
            CollectionAssert.AreEquivalent(expecteds, actuals.ToList());
            // CollectionAssertじゃenumの違いみてくれない
            var enumEquals = expecteds
                .Join(
                actuals,
                e => e.HolidayDate,
                a => a.HolidayDate,
                (ex, ac) => ex.HolidayClassification == ac.HolidayClassification);
            Assert.IsTrue(enumEquals.All(x => x));
        }

        [TestMethod]
        public void 異常系_該当がない年月が指定されたとき例外を返すこと()
        {
            // given
            ILogger mock_logger = OwnCompanyHolidayRepositoryTests.mock_logger!;
            IConfiguration configuration = OwnCompanyHolidayRepositoryTests.configuration!;

            // when
            IOwnCompanyHolidayRepository repository = new OwnCompanyHolidayRepository(mock_logger, configuration);
            void target()
            {
                _ = repository.FindByYearMonth(2022, 1);
            }

            // then
            var ex = Assert.ThrowsException<AttendanceTableServiceException>(target);
            var expected = "自社カレンダーに該当がありませんでした 対象年月: 2022年1月";
            Assert.AreEqual(expected, ex.Message);
        }

        [DataTestMethod]
        [DataRow("2023/3/26")]
        public void 正常系_自社カレンダーの最大日が取得できること(string maxDate)
        {
            // given
            ILogger mock_logger = OwnCompanyHolidayRepositoryTests.mock_logger!;
            IConfiguration configuration = OwnCompanyHolidayRepositoryTests.configuration!;

            // when
            IOwnCompanyHolidayRepository repository = new OwnCompanyHolidayRepository(mock_logger, configuration);
            var actual = repository.MaxDate();

            Assert.AreEqual(maxDate, actual.ToString("yyyy/M/d"));
        }

    }
}