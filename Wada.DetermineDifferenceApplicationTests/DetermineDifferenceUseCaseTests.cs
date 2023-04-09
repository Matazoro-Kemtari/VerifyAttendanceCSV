using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.AttendanceTableAggregation;
using Wada.AttendanceTableService.MatchedEmployeeNumberAggregation;
using Wada.AttendanceTableService.ValueObjects;
using Wada.AttendanceTableService.WorkingMonthlyReportAggregation;

namespace Wada.DetermineDifferenceApplication.Tests
{
    [TestClass()]
    public class DetermineDifferenceUseCaseTests
    {
        [TestMethod()]
        public async Task 正常系_ユーズケースを実行するとリポジトリが実行されること()
        {
            // given
            DotNetEnv.Env.Load(".env");
            string[] paths = DotNetEnv.Env.GetString("TEST_XLS_PATHS").Split(';');
            uint employeeNumber = (uint)DotNetEnv.Env.GetInt("EMPLOYEE_NUMBER");
            uint attendancePersonalCode = (uint)DotNetEnv.Env.GetInt("PERSONAL_CODE");

            // ロガーモック
            Mock<ILogger> mock_logger = new();

            // ストリームリーダーモック
            Mock<IStreamReaderOpener> mock_stream_reader = new();

            // ストリームモック
            Mock<IStreamOpener> mock_stream = new();

            // 社員番号対応表モック
            Mock<IMatchedEmployeeNumberRepository> mock_match_employee = new();
            mock_match_employee.Setup(x => x.FindAll())
                .Returns(new List<MatchedEmployeeNumber>
                {
                    MatchedEmployeeNumber.ReConsttuct(1001u, 1u),
                    MatchedEmployeeNumber.ReConsttuct(1002u, 2u),
                    MatchedEmployeeNumber.ReConsttuct(1003u, 3u),
                    MatchedEmployeeNumber.ReConsttuct(1004u, 4u),
                    MatchedEmployeeNumber.ReConsttuct(employeeNumber, attendancePersonalCode),
                });

            // S社員モック
            Mock<IEmployeeRepository> mock_employee = new();

            // CSVの読み込みモック
            Mock<IEmployeeAttendanceRepository> mock_csv = new();
            mock_csv.Setup(x => x.ReadAll(It.IsAny<StreamReader>()))
                .Returns(AttendanceCSVReturns(attendancePersonalCode));

            // 勤怠表の読み込みモック
            AttendanceTable[] spreads = new AttendanceTable[]
            {
                TestAttendanceTableFactory.Create(
                    employeeNumber,
                    new AttendanceYear(2022),
                    new AttendanceMonth(5),
                    CreateTestRecords()),
                TestAttendanceTableFactory.Create(
                    1004u,
                    new AttendanceYear(2022),
                    new AttendanceMonth(5),
                    CreateTestRecords()),
            };
            Mock<IAttendanceTableRepository> mock_spread = new();
            mock_spread.SetupSequence(x => x.ReadByMonth(It.IsAny<Stream>(), It.IsAny<int>()))
                .Returns(spreads[0])
                .Returns(spreads[1]);

            // when
            IDetermineDifferenceUseCase determineDifference =
            new DetermineDifferenceUseCase(
                mock_logger.Object,
                mock_stream_reader.Object,
                mock_stream.Object,
                mock_match_employee.Object,
                mock_employee.Object,
                mock_csv.Object,
                mock_spread.Object);

            var differenceDTO = await determineDifference.ExecuteAsync("dummy", paths, 2022, 5);

            // then
            Assert.IsTrue(!differenceDTO.DetermineDifferenceEmployeesDTOs.Any());
            mock_employee.Verify(x => x.FetchAll(), Times.Once);
            mock_stream_reader.Verify(x => x.Open(It.IsAny<string>()), Times.Once);
            mock_csv.Verify(x => x.ReadAll(It.IsAny<StreamReader>()), Times.Once);
            mock_stream.Verify(x => x.Open(It.IsAny<string>()), Times.Exactly(2));
            mock_spread.Verify(x => x.ReadByMonth(It.IsAny<Stream>(), It.IsAny<int>()), Times.Exactly(2));
        }

        internal static List<WorkedMonthlyReport> AttendanceCSVReturns(uint cd) => new()
        {
            WorkedMonthlyReport.CreateForAttendanceCSV(new EmployeeAttendance(4u, 16m, 0m, 128m, 0, 0, 2m, 1m, 0, 0, 0, 0, 0m, 0m, 0m, 0m, 0m)),
            WorkedMonthlyReport.CreateForAttendanceCSV(new EmployeeAttendance(cd, 16m, 0m, 128m, 0, 0, 2m, 1m, 0, 0, 0, 0, 0m, 0m, 0m, 0m, 0m)),
        };
        internal static List<WorkedMonthlyReport> LackAttendanceCSVReturns(uint cd) => new()
        {
            WorkedMonthlyReport.CreateForAttendanceCSV(new EmployeeAttendance(cd, 16m, 0m, 128m, 0, 0, 2m, 1m, 0, 0, 0, 0, 0m, 0m, 0m, 0m, 0m)),
        };

        private static ICollection<AttendanceRecord> CreateTestRecords()
        {
            ICollection<AttendanceRecord> records = new List<AttendanceRecord>
            {
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022),new AttendanceMonth(5),2),HolidayClassification.None,DayOffClassification.PaidLeave,null,null,null),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 9), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/09 08:00")), new AttendanceTime(DateTime.Parse("2022/05/09 17:00")), new TimeSpan(1, 0, 0)),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 10), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/10 08:00")), new AttendanceTime(DateTime.Parse("2022/05/10 17:00")), new TimeSpan(1, 0, 0)),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 11), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/11 08:00")), new AttendanceTime(DateTime.Parse("2022/05/11 17:00")), new TimeSpan(1, 0, 0)),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 12), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/12 08:00")), new AttendanceTime(DateTime.Parse("2022/05/12 17:00")), new TimeSpan(1, 0, 0)),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 13), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/13 08:00")), new AttendanceTime(DateTime.Parse("2022/05/13 17:00")), new TimeSpan(1, 0, 0)),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 14), HolidayClassification.RegularHoliday, DayOffClassification.TransferedAttendance, new AttendanceTime(DateTime.Parse("2022/05/14 08:00")), new AttendanceTime(DateTime.Parse("2022/05/14 17:00")), new TimeSpan(1, 0, 0)),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 16), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/16 08:00")), new AttendanceTime(DateTime.Parse("2022/05/16 17:00")), new TimeSpan(1, 0, 0)),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 17), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/17 08:00")), new AttendanceTime(DateTime.Parse("2022/05/17 17:00")), new TimeSpan(1, 0, 0)),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 18), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/18 08:00")), new AttendanceTime(DateTime.Parse("2022/05/18 17:00")), new TimeSpan(1, 0, 0)),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 19), HolidayClassification.None, DayOffClassification.SubstitutedHoliday, null, null, null),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 20), HolidayClassification.None, DayOffClassification.PaidLeave, null, null, null),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 23), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/23 08:00")), new AttendanceTime(DateTime.Parse("2022/05/23 17:00")), new TimeSpan(1, 0, 0)),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 24), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/24 08:00")), new AttendanceTime(DateTime.Parse("2022/05/24 17:00")), new TimeSpan(1, 0, 0)),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 25), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/25 08:00")), new AttendanceTime(DateTime.Parse("2022/05/25 17:00")), new TimeSpan(1, 0, 0)),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 26), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/26 08:00")), new AttendanceTime(DateTime.Parse("2022/05/26 17:00")), new TimeSpan(1, 0, 0)),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 27), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/27 08:00")), new AttendanceTime(DateTime.Parse("2022/05/27 17:00")), new TimeSpan(1, 0, 0)),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 30), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/30 08:00")), new AttendanceTime(DateTime.Parse("2022/05/30 17:00")), new TimeSpan(1, 0, 0)),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 31), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/31 08:00")), new AttendanceTime(DateTime.Parse("2022/05/31 17:00")), new TimeSpan(1, 0, 0)),
            };
            return records;
        }

        [TestMethod()]
        public async Task 正常系_CSVの方が件数が少ない場合差分情報を返すこと()
        {
            // given
            DotNetEnv.Env.Load(".env");
            string[] paths = DotNetEnv.Env.GetString("TEST_XLS_PATHS").Split(';');
            uint employeeNumber = (uint)DotNetEnv.Env.GetInt("EMPLOYEE_NUMBER");
            uint attendancePersonalCode = (uint)DotNetEnv.Env.GetInt("PERSONAL_CODE");

            // ロガーモック
            Mock<ILogger> mock_logger = new();

            // ストリームリーダーモック
            Mock<IStreamReaderOpener> mock_stream_reader = new();

            // ストリームモック
            Mock<IStreamOpener> mock_stream = new();

            // 社員番号対応表モック
            Mock<IMatchedEmployeeNumberRepository> mock_match_employee = new();
            mock_match_employee.Setup(x => x.FindAll())
                .Returns(new List<MatchedEmployeeNumber>
                {
                    MatchedEmployeeNumber.ReConsttuct(1001u, 1u),
                    MatchedEmployeeNumber.ReConsttuct(1002u, 2u),
                    MatchedEmployeeNumber.ReConsttuct(1003u, 3u),
                    MatchedEmployeeNumber.ReConsttuct(1004u, 4u),
                    MatchedEmployeeNumber.ReConsttuct(employeeNumber, attendancePersonalCode),
                });

            // S社員モック
            Mock<IEmployeeRepository> mock_employee = new();

            // CSVの読み込みモック
            Mock<IEmployeeAttendanceRepository> mock_csv = new();
            mock_csv.Setup(x => x.ReadAll(It.IsAny<StreamReader>()))
                .Returns(LackAttendanceCSVReturns(attendancePersonalCode));

            // 勤怠表の読み込みモック
            AttendanceTable[] spreads = new AttendanceTable[]
            {
                TestAttendanceTableFactory.Create(
                    employeeNumber,
                    new AttendanceYear(2022),
                    new AttendanceMonth(5),
                    CreateTestRecords()),
                TestAttendanceTableFactory.Create(
                    1004u,
                    new AttendanceYear(2022),
                    new AttendanceMonth(5),
                    CreateTestRecords()),
            };
            Mock<IAttendanceTableRepository> mock_spread = new();
            mock_spread.SetupSequence(x => x.ReadByMonth(It.IsAny<Stream>(), It.IsAny<int>()))
                .Returns(spreads[0])
                .Returns(spreads[1]);

            // when
            IDetermineDifferenceUseCase determineDifference =
            new DetermineDifferenceUseCase(
                mock_logger.Object,
                mock_stream_reader.Object,
                mock_stream.Object,
                mock_match_employee.Object,
                mock_employee.Object,
                mock_csv.Object,
                mock_spread.Object);

            var differenceDTO = await determineDifference.ExecuteAsync("dummy", paths, 2022, 5);

            // then
            Assert.IsTrue(differenceDTO.DetermineDifferenceEmployeesDTOs.Count() == 1);
            Assert.AreEqual(14,
                differenceDTO.DetermineDifferenceEmployeesDTOs
                .First(x => x.AttendancePersonalCode == 4u)
                .Differences.Count());
            mock_stream_reader.Verify(x => x.Open(It.IsAny<string>()), Times.Once);
            mock_csv.Verify(x => x.ReadAll(It.IsAny<StreamReader>()), Times.Once);
            mock_stream.Verify(x => x.Open(It.IsAny<string>()), Times.Exactly(2));
            mock_spread.Verify(x => x.ReadByMonth(It.IsAny<Stream>(), It.IsAny<int>()), Times.Exactly(2));
        }

        [TestMethod()]
        public async Task 正常系_Excelの方が件数が少ない場合差分情報を返すこと()
        {
            // given
            DotNetEnv.Env.Load(".env");
            string[] paths = DotNetEnv.Env.GetString("TEST_XLS_PATHS").Split(';');
            uint employeeNumber = (uint)DotNetEnv.Env.GetInt("EMPLOYEE_NUMBER");
            uint attendancePersonalCode = (uint)DotNetEnv.Env.GetInt("PERSONAL_CODE");

            // ロガーモック
            Mock<ILogger> mock_logger = new();

            // ストリームリーダーモック
            Mock<IStreamReaderOpener> mock_stream_reader = new();

            // ストリームモック
            Mock<IStreamOpener> mock_stream = new();

            // 社員番号対応表モック
            Mock<IMatchedEmployeeNumberRepository> mock_match_employee = new();
            mock_match_employee.Setup(x => x.FindAll())
                .Returns(new List<MatchedEmployeeNumber>
                {
                    MatchedEmployeeNumber.ReConsttuct(1001u, 1u),
                    MatchedEmployeeNumber.ReConsttuct(1002u, 2u),
                    MatchedEmployeeNumber.ReConsttuct(1003u, 3u),
                    MatchedEmployeeNumber.ReConsttuct(1004u, 4u),
                    MatchedEmployeeNumber.ReConsttuct(employeeNumber, attendancePersonalCode),
                });

            // S社員モック
            Mock<IEmployeeRepository> mock_employee = new();

            // CSVの読み込みモック
            Mock<IEmployeeAttendanceRepository> mock_csv = new();
            mock_csv.Setup(x => x.ReadAll(It.IsAny<StreamReader>()))
                .Returns(AttendanceCSVReturns(attendancePersonalCode));

            // 勤怠表の読み込みモック
            AttendanceTable[] spreads = new AttendanceTable[]
            {
                TestAttendanceTableFactory.Create(
                    employeeNumber,
                    new AttendanceYear(2022),
                    new AttendanceMonth(5),
                    CreateTestRecords()),
            };
            Mock<IAttendanceTableRepository> mock_spread = new();
            mock_spread.Setup(x => x.ReadByMonth(It.IsAny<Stream>(), It.IsAny<int>()))
                .Returns(spreads[0]);

            // when
            IDetermineDifferenceUseCase determineDifference =
            new DetermineDifferenceUseCase(
                mock_logger.Object,
                mock_stream_reader.Object,
                mock_stream.Object,
                mock_match_employee.Object,
                mock_employee.Object,
                mock_csv.Object,
                mock_spread.Object);

            var differenceDTO = await determineDifference.ExecuteAsync("dummy", paths, 2022, 5);

            // then
            Assert.IsTrue(differenceDTO.DetermineDifferenceEmployeesDTOs.Count() == 1);
            Assert.AreEqual(14,
                differenceDTO.DetermineDifferenceEmployeesDTOs
                .First(x => x.AttendancePersonalCode == 4u)
                .Differences.Count());
            mock_stream_reader.Verify(x => x.Open(It.IsAny<string>()), Times.Once);
            mock_csv.Verify(x => x.ReadAll(It.IsAny<StreamReader>()), Times.Once);
            mock_stream.Verify(x => x.Open(It.IsAny<string>()), Times.Exactly(2));
            mock_spread.Verify(x => x.ReadByMonth(It.IsAny<Stream>(), It.IsAny<int>()), Times.Exactly(2));
        }

        [TestMethod]
        public async Task 異常系_社員番号対応表に無い社員番号を検索すると例外を返すこと()
        {
            // given
            DotNetEnv.Env.Load(".env");
            string[] paths = DotNetEnv.Env.GetString("TEST_XLS_PATHS").Split(';');
            uint employeeNumber = (uint)DotNetEnv.Env.GetInt("EMPLOYEE_NUMBER");
            uint attendancePersonalCode = (uint)DotNetEnv.Env.GetInt("PERSONAL_CODE");

            // ロガーモック
            Mock<ILogger> mock_logger = new();

            // ストリームリーダーモック
            Mock<IStreamReaderOpener> mock_stream_reader = new();

            // ストリームモック
            Mock<IStreamOpener> mock_stream = new();

            // 社員番号対応表モック
            Mock<IMatchedEmployeeNumberRepository> mock_match_employee = new();
            mock_match_employee.Setup(x => x.FindAll())
                .Returns(new List<MatchedEmployeeNumber>
                {
                    MatchedEmployeeNumber.ReConsttuct(1001u, 1u),
                    MatchedEmployeeNumber.ReConsttuct(1002u, 2u),
                    MatchedEmployeeNumber.ReConsttuct(1003u, 3u),
                    MatchedEmployeeNumber.ReConsttuct(1004u, 4u),
                });

            // S社員モック
            Mock<IEmployeeRepository> mock_employee = new();

            // CSVの読み込みモック
            Mock<IEmployeeAttendanceRepository> mock_csv = new();
            mock_csv.Setup(x => x.ReadAll(It.IsAny<StreamReader>()))
                .Returns(AttendanceCSVReturns(attendancePersonalCode));

            // 勤怠表の読み込みモック
            AttendanceTable spreads =
                TestAttendanceTableFactory.Create(
                    employeeNumber,
                    new AttendanceYear(2022),
                    new AttendanceMonth(5),
                    CreateTestRecords());
            Mock<IAttendanceTableRepository> mock_spread = new();
            mock_spread.Setup(x => x.ReadByMonth(It.IsAny<Stream>(), It.IsAny<int>()))
                .Returns(spreads);

            // when
            IDetermineDifferenceUseCase determineDifference =
            new DetermineDifferenceUseCase(
                mock_logger.Object,
                mock_stream_reader.Object,
                mock_stream.Object,
                mock_match_employee.Object,
                mock_employee.Object,
                mock_csv.Object,
                mock_spread.Object);

            async Task target()
            {
                await determineDifference.ExecuteAsync("dummy", paths, 2022, 5);
            }

            // then
            var msg = $"社員番号対応表に該当がありません 社員番号: {employeeNumber}";
            var ex = await Assert.ThrowsExceptionAsync<EmployeeNumberNotFoundException>(target);
            Assert.AreEqual(msg, ex.Message);
        }
    }
}