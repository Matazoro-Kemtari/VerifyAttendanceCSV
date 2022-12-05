using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;
using System.Transactions;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.MatchedEmployeeNumberAggregation;

namespace Wada.DesignDepartmentDataBse.Tests
{
    [TestClass()]
    public class MatchedEmployeeNumberRepositoryTests
    {
        private static ILogger? mock_logger;
        private static IConfiguration? configuration;

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
        public void 正常系_社員番号対応表が取得できること()
        {
            // given
            ILogger mock_logger = MatchedEmployeeNumberRepositoryTests.mock_logger!;
            IConfiguration configuration = MatchedEmployeeNumberRepositoryTests.configuration!;

            // when
            IMatchedEmployeeNumberRepository repository = new MatchedEmployeeNumberRepository(mock_logger, configuration);
            var actuals = repository.FindAll();

            // then
            var expecteds = TestEmployeeNumbers;
            CollectionAssert.AreEquivalent(expecteds.ToList(), actuals.ToList());
        }

        private static IEnumerable<MatchedEmployeeNumber> TestEmployeeNumbers => new List<MatchedEmployeeNumber> {
                MatchedEmployeeNumber.ReConsttuct(1010u,10u),
                MatchedEmployeeNumber.ReConsttuct(1013u,13u),
                MatchedEmployeeNumber.ReConsttuct(1016u,16u),
                MatchedEmployeeNumber.ReConsttuct(1036u,36u),
                MatchedEmployeeNumber.ReConsttuct(1045u,45u),
                MatchedEmployeeNumber.ReConsttuct(1060u,60u),
                MatchedEmployeeNumber.ReConsttuct(1064u,64u),
                MatchedEmployeeNumber.ReConsttuct(1070u,70u),
                MatchedEmployeeNumber.ReConsttuct(1078u,78u),
                MatchedEmployeeNumber.ReConsttuct(1094u,94u),
                MatchedEmployeeNumber.ReConsttuct(1109u,109u),
                MatchedEmployeeNumber.ReConsttuct(1120u,120u),
                MatchedEmployeeNumber.ReConsttuct(1146u,146u),
                MatchedEmployeeNumber.ReConsttuct(1151u,151u),
                MatchedEmployeeNumber.ReConsttuct(1179u,179u),
                MatchedEmployeeNumber.ReConsttuct(1180u,180u),
                MatchedEmployeeNumber.ReConsttuct(1185u,185u),
                MatchedEmployeeNumber.ReConsttuct(1201u,201u),
                MatchedEmployeeNumber.ReConsttuct(1202u,202u),
                MatchedEmployeeNumber.ReConsttuct(1214u,214u),
                MatchedEmployeeNumber.ReConsttuct(1228u,228u),
                MatchedEmployeeNumber.ReConsttuct(1232u,232u),
                MatchedEmployeeNumber.ReConsttuct(1254u,254u),
                MatchedEmployeeNumber.ReConsttuct(1264u,264u),
                MatchedEmployeeNumber.ReConsttuct(1288u,288u),
                MatchedEmployeeNumber.ReConsttuct(1296u,296u),
                MatchedEmployeeNumber.ReConsttuct(1302u,302u),
                MatchedEmployeeNumber.ReConsttuct(1303u,303u),
                MatchedEmployeeNumber.ReConsttuct(1318u,318u),
                MatchedEmployeeNumber.ReConsttuct(1330u,330u),
                MatchedEmployeeNumber.ReConsttuct(1338u,338u),
                MatchedEmployeeNumber.ReConsttuct(1340u,340u),
                MatchedEmployeeNumber.ReConsttuct(1349u,349u),
                MatchedEmployeeNumber.ReConsttuct(1350u,350u),
                MatchedEmployeeNumber.ReConsttuct(1370u,370u),
                MatchedEmployeeNumber.ReConsttuct(1381u,381u),
                MatchedEmployeeNumber.ReConsttuct(1382u,382u),
                MatchedEmployeeNumber.ReConsttuct(1385u,385u),
                MatchedEmployeeNumber.ReConsttuct(1386u,386u),
                MatchedEmployeeNumber.ReConsttuct(1387u,387u),
                MatchedEmployeeNumber.ReConsttuct(1408u,408u),
                MatchedEmployeeNumber.ReConsttuct(1427u,427u),
                MatchedEmployeeNumber.ReConsttuct(1431u,431u),
                MatchedEmployeeNumber.ReConsttuct(1446u,446u),
                MatchedEmployeeNumber.ReConsttuct(1457u,457u),
                MatchedEmployeeNumber.ReConsttuct(1463u,463u),
                MatchedEmployeeNumber.ReConsttuct(1469u,469u),
                MatchedEmployeeNumber.ReConsttuct(1479u,479u),
                MatchedEmployeeNumber.ReConsttuct(1491u,491u),
                MatchedEmployeeNumber.ReConsttuct(1503u,503u),
                MatchedEmployeeNumber.ReConsttuct(1509u,509u),
                MatchedEmployeeNumber.ReConsttuct(1511u,511u),
                MatchedEmployeeNumber.ReConsttuct(1512u,1039u),
                MatchedEmployeeNumber.ReConsttuct(1513u,2008u),
                MatchedEmployeeNumber.ReConsttuct(1517u,517u),
                MatchedEmployeeNumber.ReConsttuct(1518u,2007u),
                MatchedEmployeeNumber.ReConsttuct(1519u,519u),
                MatchedEmployeeNumber.ReConsttuct(1520u,520u),
                MatchedEmployeeNumber.ReConsttuct(1523u,1055u),
                MatchedEmployeeNumber.ReConsttuct(1525u,525u),
                MatchedEmployeeNumber.ReConsttuct(1526u,526u),
                MatchedEmployeeNumber.ReConsttuct(1527u,527u),
                MatchedEmployeeNumber.ReConsttuct(1528u,528u),
                MatchedEmployeeNumber.ReConsttuct(2009u,2009u),
            };

        [TestMethod()]
        public void 正常系_複数の社員番号対応表を登録できること()
        {
            // given
            ILogger mock_logger = MatchedEmployeeNumberRepositoryTests.mock_logger!;
            IConfiguration configuration = MatchedEmployeeNumberRepositoryTests.configuration!;

            // when
            var expecteds = TestEmployeeNumbers;
            IMatchedEmployeeNumberRepository repository = new MatchedEmployeeNumberRepository(mock_logger, configuration);
            using TransactionScope scope = new();
            repository.RemoveAll();
            repository.AddRange(expecteds);
            var actuals = repository.FindAll();

            // then
            CollectionAssert.AreEquivalent(expecteds.ToList(), actuals.ToList());
        }
    }
}