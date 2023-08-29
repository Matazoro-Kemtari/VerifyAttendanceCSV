using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;
using System.Transactions;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.MatchedEmployeeNumberAggregation;

namespace Wada.Data.DesignDepartmentDataBase.Tests
{
    [TestClass()]
    public class MatchedEmployeeNumberRepositoryTests
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
        public async Task 正常系_社員番号対応表が取得できること()
        {
            // given
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
            var loggerMock = Mock.Of<ILogger>();

            IMatchedEmployeeNumberRepository repository = new MatchedEmployeeNumberRepository(_configuration!, loggerMock);

            // 使われていない番号でテストデータ作成
            var before = await repository.FindAllAsync();
            var matchedEmployees = Enumerable.Range(1, 10)
                                             .Where(x => !before.Any(y => y.AttendancePersonalCode == x))
                                             .Select(x => TestMatchedEmployeeNumberFactory.Create(
                                                 employeeNumber: (uint)x + 1000u,
                                                 attendancePersonalCode: (uint)x));
            if (!matchedEmployees.Any())
                Assert.Fail("テストデータ未作成");

            // when
            await repository.AddRangeAsync(matchedEmployees);
            var additional = await repository.FindAllAsync();
            await repository.RemoveRangeAsync(matchedEmployees);
            var removed = await repository.FindAllAsync();

            // then
            matchedEmployees.ToList().ForEach(x => Assert.IsTrue(additional.Any(y => x == y)));
            matchedEmployees.ToList().ForEach(x => Assert.IsFalse(removed.Any(y => x == y)));
            CollectionAssert.AreEqual(before.ToArray(), removed.ToArray());
        }

        [TestMethod]
        public async Task 異常系_追加に失敗したとき例外を返すこと()
        {
            // given
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
            var loggerMock = Mock.Of<ILogger>();

            IMatchedEmployeeNumberRepository repository = new MatchedEmployeeNumberRepository(_configuration!, loggerMock);
            var before = await repository.FindAllAsync();

            // when
            Task target() => repository.AddRangeAsync(before);

            // then
            var ex = await Assert.ThrowsExceptionAsync<MatchedEmployeeNumberAggregationException>(target);
            var message = "社員番号対応表に追加できませんでした";
            Assert.AreEqual(message, ex.Message);
        }

        [TestMethod]
        public async Task 異常系_削除に失敗したとき例外を返すこと()
        {
            // given
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
            var loggerMock = Mock.Of<ILogger>();

            IMatchedEmployeeNumberRepository repository = new MatchedEmployeeNumberRepository(_configuration!, loggerMock);
            // 使われていない番号でテストデータ作成
            var before = await repository.FindAllAsync();
            var matchedEmployees = Enumerable.Range(1, 10)
                                             .Where(x => !before.Any(y => y.AttendancePersonalCode == x))
                                             .Select(x => TestMatchedEmployeeNumberFactory.Create(
                                                 employeeNumber: (uint)x + 1000u,
                                                 attendancePersonalCode: (uint)x));
            if (!matchedEmployees.Any())
                Assert.Fail("テストデータ未作成");

            // when
            Task target() => repository.RemoveRangeAsync(matchedEmployees);

            // then
            var ex = await Assert.ThrowsExceptionAsync<MatchedEmployeeNumberAggregationException>(target);
            var message = "社員番号対応表から削除できませんでした";
            Assert.AreEqual(message, ex.Message);
        }
    }
}