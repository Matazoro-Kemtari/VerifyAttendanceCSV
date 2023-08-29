using Wada.Data.DesignDepartmentDataBase;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;
using System.Transactions;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.OwnCompanyCalendarAggregation;
using Wada.AttendanceTableService.ValueObjects;

namespace Wada.Data.DesignDepartmentDataBase.Tests
{
    [TestClass()]
    public class OwnCompanyHolidayRepositoryTests
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
        public async Task 正常系_指定年月の自社カレンダーを取得できること()
        {
            // given
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
            var loggerMock = Mock.Of<ILogger>();

            var newCalId = Ulid.NewUlid();
            var holiday = DateTime.Now.Date;
            var additionalCals = new[]
            {
                TestOwnCompanyHolidayFactory.Create(
                    calendarGroupId: newCalId.ToString(),
                    holidayDate: holiday,
                    holidayClassification: HolidayClassification.LegalHoliday),
            };

            // when
            IOwnCompanyHolidayRepository repository = new OwnCompanyHolidayRepository(_configuration!, loggerMock);
            await repository.AddRangeAsync(additionalCals);
            var expected = await repository.FindByYearMonthAsync(newCalId.ToString(), holiday.Year, holiday.Month);
            await repository.RemoveRangeAsync(additionalCals);

            // then
            CollectionAssert.AreEquivalent(additionalCals.ToArray(), expected.ToArray());
        }

        [TestMethod]
        public async Task 異常系_該当レコードがないとき例外を返すこと()
        {
            // given
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
            var loggerMock = Mock.Of<ILogger>();

            var newCalId = Ulid.NewUlid();
            var holiday = DateTime.Now;

            // when
            IOwnCompanyHolidayRepository repository = new OwnCompanyHolidayRepository(_configuration!, loggerMock);
            Task target() => repository.FindByYearMonthAsync(newCalId.ToString(), holiday.Year, holiday.Month);

            // then
            var ex = await Assert.ThrowsExceptionAsync<OwnCompanyCalendarAggregationException>(target);
            var message = "自社カレンダーに該当がありませんでした " +
                $"カレンダーグループ: {newCalId}, 対象年月: {holiday.Year}年{holiday.Month}月";
            Assert.AreEqual(message, ex.Message);
        }

        [TestMethod()]
        public async Task 異常系_追加に失敗したとき例外を返すこと()
        {
            // given
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
            var loggerMock = Mock.Of<ILogger>();

            var calId = "01GW8E3ENDPWX0FXW0788VR63J";
            var holiday = new DateTime(2023, 1, 1);
            IOwnCompanyHolidayRepository repository = new OwnCompanyHolidayRepository(_configuration!, loggerMock);
            var additionalCals = await repository.FindByYearMonthAsync(calId, holiday.Year, holiday.Month);

            // when
            Task target() => repository.AddRangeAsync(additionalCals!);

            // then
            var ex = await Assert.ThrowsExceptionAsync<OwnCompanyCalendarAggregationException>(target);
            var message = "自社カレンダーに追加できませんでした";
            Assert.AreEqual(message, ex.Message);
        }

        [TestMethod()]
        public async Task 異常系_削除に失敗したとき例外を返すこと()
        {
            // given
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
            var loggerMock = Mock.Of<ILogger>();

            var calId = Ulid.NewUlid();
            var holiday = new DateTime(2023, 1, 1);
            var removables = new[] {
                TestOwnCompanyHolidayFactory.Create(
                    calId.ToString(),
                    holiday,
                    HolidayClassification.LegalHoliday),
            };

            // when
            IOwnCompanyHolidayRepository repository = new OwnCompanyHolidayRepository(_configuration!, loggerMock);
            Task target() => repository.RemoveRangeAsync(removables!);

            // then
            var ex = await Assert.ThrowsExceptionAsync<OwnCompanyCalendarAggregationException>(target);
            var message = "自社カレンダーから削除できませんでした";
            Assert.AreEqual(message, ex.Message);
        }

        [TestMethod()]
        public async Task 正常系_指定日以降の自社カレンダーが取得できること()
        {
            // given
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
            var loggerMock = Mock.Of<ILogger>();

            var calId = "01GW8E3ENDPWX0FXW0788VR63J";
            var targetDate = DateTime.Now.Date;
            IOwnCompanyHolidayRepository repository = new OwnCompanyHolidayRepository(_configuration!, loggerMock);
            // 既存を消す
            var removable = await repository.FindByAfterDateAsync(OwnCompanyHoliday.GetCalendarGroupId(CalendarGroupClassification.HeadOffice), targetDate.AddMonths(-10));
            if (removable.Any())
                await repository.RemoveRangeAsync(removable);
            // テストカレンダー作成
            var additionalCals = Enumerable.Range(-10, 20).Select(d =>
                TestOwnCompanyHolidayFactory.Create(
                    calendarGroupId: calId,
                    holidayDate: targetDate.AddMonths(d),
                    holidayClassification: HolidayClassification.LegalHoliday));
            await repository.AddRangeAsync(additionalCals);

            // when
            var expected = await repository.FindByAfterDateAsync(OwnCompanyHoliday.GetCalendarGroupId(CalendarGroupClassification.HeadOffice), targetDate);

            // then
            CollectionAssert.AreEquivalent(
                additionalCals.Where(x => x.HolidayDate >= targetDate).ToList(),
                expected.ToList());
        }
    }
}