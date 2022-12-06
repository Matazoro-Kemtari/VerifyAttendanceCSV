using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;
using System.Text;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.AttendanceTableAggregation;
using Wada.AttendanceTableService.MatchedEmployeeNumberAggregation;
using Wada.AttendanceTableService.ValueObjects;
using Wada.AttendanceTableService.WorkingMonthlyReportAggregation;

namespace DetermineDifferenceApplication.Tests
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
            string[] filePaths = DotNetEnv.Env.GetString("TEST_XLS_PATH").Split(';');
            uint employeeNumber = (uint)DotNetEnv.Env.GetInt("EMPLOYEE_NUMBER");
            uint attendancePersonalCode = (uint)DotNetEnv.Env.GetInt("PERSONAL_CODE");

            // ロガーモック
            Mock<ILogger> mock_logger = new();

            // ストリームリーダーモック
            Mock<IStreamReaderOpener> mock_stream_reader = new();
            mock_stream_reader.Setup(x => x.Open(It.IsAny<string>()))
                .Returns(new StreamReader(
                    new MemoryStream(
                        Encoding.UTF8.GetBytes(attendanceCSVData))));

            // ストリームモック
            Mock<IStreamOpener> mock_stream = new();
            mock_stream.SetupSequence(x => x.Open(It.IsAny<string>()))
                .Returns(File.Open(filePaths[0], FileMode.Open))
                .Returns(File.Open(filePaths[1], FileMode.Open));

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

            // CSVの読み込みモック
            Mock<IEmployeeAttendanceRepository> mock_csv = new();
            mock_csv.Setup(x => x.ReadAll(It.IsAny<StreamReader>()))
                .Returns(attendanceCSVReturns(attendancePersonalCode));

            // 勤怠表の読み込みモック
            AttendanceTable[] spreads =new AttendanceTable[]
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
                mock_csv.Object,
                mock_spread.Object);

            var actual = await determineDifference.ExecuteAsync("dummy", paths, 2022, 5);

            // then
            Assert.IsTrue(actual.Count == 0);
            mock_stream_reader.Verify(x => x.Open(It.IsAny<string>()), Times.Once);
            mock_csv.Verify(x => x.ReadAll(It.IsAny<StreamReader>()), Times.Once);
            mock_stream.Verify(x => x.Open(It.IsAny<string>()), Times.Exactly(2));
            mock_spread.Verify(x => x.ReadByMonth(It.IsAny<Stream>(), It.IsAny<int>()), Times.Exactly(2));
        }

        internal static readonly string attendanceCSVData =
    @"4,16,1,128,0,0,2,1,0,0,0,0,0,0,0,8,0,0
2007,16,1,128,0,0,2,1,0,0,0,0,0,0,0,8,0,0
";

        internal static List<WorkedMonthlyReport> attendanceCSVReturns(uint cd) => new()
        {
            WorkedMonthlyReport.CreateForAttendanceCSV(new EmployeeAttendance(4u, 16m, 0m, 128m, 0, 0, 2m, 1m, 0, 0m, 0m, 0, 0m, 0m, 0m, 0m, 0, 0m)),
            WorkedMonthlyReport.CreateForAttendanceCSV(new EmployeeAttendance(cd, 16m, 0m, 128m, 0, 0, 2m, 1m, 0, 0m, 0m, 0, 0m, 0m, 0m, 0m, 0, 0m)),
            WorkedMonthlyReport.CreateForAttendanceCSV(new EmployeeAttendance(994u, 16m, 0m, 128m, 0, 0, 2m, 1m, 0, 0m, 0m, 0, 0m, 0m, 0m, 0m, 0, 0m)),
        };

        private static ICollection<AttendanceRecord> CreateTestRecords()
        {
            ICollection<AttendanceRecord> records = new List<AttendanceRecord>
            {
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022),new AttendanceMonth(5),2),HolidayClassification.None,DayOffClassification.PaidLeave,null,null,null,OrderedLunchBox.None),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 9), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/09 08:00")), new AttendanceTime(DateTime.Parse("2022/05/09 17:00")), new TimeSpan(1, 0, 0), OrderedLunchBox.None),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 10), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/10 08:00")), new AttendanceTime(DateTime.Parse("2022/05/10 17:00")), new TimeSpan(1, 0, 0), OrderedLunchBox.None),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 11), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/11 08:00")), new AttendanceTime(DateTime.Parse("2022/05/11 17:00")), new TimeSpan(1, 0, 0), OrderedLunchBox.None),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 12), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/12 08:00")), new AttendanceTime(DateTime.Parse("2022/05/12 17:00")), new TimeSpan(1, 0, 0), OrderedLunchBox.None),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 13), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/13 08:00")), new AttendanceTime(DateTime.Parse("2022/05/13 17:00")), new TimeSpan(1, 0, 0), OrderedLunchBox.None),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 14), HolidayClassification.RegularHoliday, DayOffClassification.TransferedAttendance, new AttendanceTime(DateTime.Parse("2022/05/14 08:00")), new AttendanceTime(DateTime.Parse("2022/05/14 17:00")), new TimeSpan(1, 0, 0), OrderedLunchBox.None),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 16), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/16 08:00")), new AttendanceTime(DateTime.Parse("2022/05/16 17:00")), new TimeSpan(1, 0, 0), OrderedLunchBox.None),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 17), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/17 08:00")), new AttendanceTime(DateTime.Parse("2022/05/17 17:00")), new TimeSpan(1, 0, 0), OrderedLunchBox.None),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 18), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/18 08:00")), new AttendanceTime(DateTime.Parse("2022/05/18 17:00")), new TimeSpan(1, 0, 0), OrderedLunchBox.None),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 19), HolidayClassification.None, DayOffClassification.SubstitutedHoliday, null, null, null, OrderedLunchBox.None),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 20), HolidayClassification.None, DayOffClassification.PaidLeave, null, null, null, OrderedLunchBox.None),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 23), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/23 08:00")), new AttendanceTime(DateTime.Parse("2022/05/23 17:00")), new TimeSpan(1, 0, 0), OrderedLunchBox.None),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 24), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/24 08:00")), new AttendanceTime(DateTime.Parse("2022/05/24 17:00")), new TimeSpan(1, 0, 0), OrderedLunchBox.None),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 25), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/25 08:00")), new AttendanceTime(DateTime.Parse("2022/05/25 17:00")), new TimeSpan(1, 0, 0), OrderedLunchBox.None),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 26), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/26 08:00")), new AttendanceTime(DateTime.Parse("2022/05/26 17:00")), new TimeSpan(1, 0, 0), OrderedLunchBox.None),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 27), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/27 08:00")), new AttendanceTime(DateTime.Parse("2022/05/27 17:00")), new TimeSpan(1, 0, 0), OrderedLunchBox.None),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 30), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/30 08:00")), new AttendanceTime(DateTime.Parse("2022/05/30 17:00")), new TimeSpan(1, 0, 0), OrderedLunchBox.None),
                new AttendanceRecord(new AttendanceDay(new AttendanceYear(2022), new AttendanceMonth(5), 31), HolidayClassification.None, DayOffClassification.None, new AttendanceTime(DateTime.Parse("2022/05/31 08:00")), new AttendanceTime(DateTime.Parse("2022/05/31 17:00")), new TimeSpan(1, 0, 0), OrderedLunchBox.None),
            };
            return records;
        }
    }
}