using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.WorkingMonthlyReportAggregation;

namespace Wada.AttendanceCSV.Tests
{
    [TestClass()]
    public class EmployeeAttendanceRepositoryTests
    {
        [TestMethod()]
        public void 正常系_勤怠CSVファイルが読み込めること()
        {
            // given
            // テストデータを読み込む
            using StreamReader reader = new StreamReader(
                new MemoryStream(
                    Encoding.UTF8.GetBytes(attendanceCSVData)))
                ?? throw new ArgumentNullException(
                    "StreamReader作るときに失敗した");

            // when
            IEmployeeAttendanceRepository employeeAttendanceRepository
                = new EmployeeAttendanceRepository();
            IEnumerable<WorkedMonthlyReport> actuals =
                employeeAttendanceRepository.ReadAll(reader);

            // then
            Assert.AreEqual(5, actuals.Count());
            List<WorkedMonthlyReport> expecteds = new()
            {
                WorkedMonthlyReport.CreateForAttendanceCSV(new EmployeeAttendance(202u, 20m, 1m, 160m, 0, 0, 0m, 0m, 0, 0, 0, 0, 33m, 0m, 65m, 8m, 0m)),
                WorkedMonthlyReport.CreateForAttendanceCSV(new EmployeeAttendance(1u, 19m, 0m, 152m, 0, 0, 0m, 0m, 0, 0, 0, 0, 0m, 0m, 0m, 0m, 0m)),
                WorkedMonthlyReport.CreateForAttendanceCSV(new EmployeeAttendance(201u, 19.5m, 0m, 156m, 0, 0, 0.5m, 0m, 0, 0, 0, 0, 0m, 0m, 0m, 0m, 0m)),
                WorkedMonthlyReport.CreateForAttendanceCSV(new EmployeeAttendance(214u, 18.5m, 1m, 148m, 0, 0, 1.5m, 0, 0, 0, 0, 0, 5.5m, 0m, 0m, 1m, 0m)),
                WorkedMonthlyReport.CreateForAttendanceCSV(new EmployeeAttendance(446u, 18m, 0m, 116.5m, 0, 0, 2m, 0, 0, 0, 0, 0, 0m, 0m, 0m, 0m, 0m)),
                WorkedMonthlyReport.CreateForAttendanceCSV(new EmployeeAttendance(254u, 15m, 0m, 102m, 0, 4, 0, 0m, 0, 0, 0, 5, 0m, 0m, 0m, 0m, 0m)),
            };
            // IDを消して比較
            CollectionAssert.AreEquivalent(
                expecteds.Select(x => new
                {
                    x.AttendancePersonalCode,
                    x.AttendanceDay,
                    x.HolidayWorkedDay,
                    x.PaidLeaveDay,
                    x.AbsenceDay,
                    x.TransferedAttendanceDay,
                    x.PaidSpecialLeaveDay,
                    x.LatenessTime,
                    x.EarlyLeaveTime,
                    x.BusinessSuspensionDay,
                    x.EducationDay,
                    x.RegularWorkedHour,
                    x.OvertimeHour,
                    x.LateNightWorkingHour,
                    x.LegalHolidayWorkedHour,
                    x.RegularHolidayWorkedHour,
                    x.AnomalyHour,
                })
                .Where(x => x.AttendancePersonalCode != 1)
                .Where(x => x.AttendancePersonalCode != 2)
                .Where(x => x.AttendancePersonalCode != 52)
                .ToList(),
                actuals.Select(x => new
                {
                    x.AttendancePersonalCode,
                    x.AttendanceDay,
                    x.HolidayWorkedDay,
                    x.PaidLeaveDay,
                    x.AbsenceDay,
                    x.TransferedAttendanceDay,
                    x.PaidSpecialLeaveDay,
                    x.LatenessTime,
                    x.EarlyLeaveTime,
                    x.BusinessSuspensionDay,
                    x.EducationDay,
                    x.RegularWorkedHour,
                    x.OvertimeHour,
                    x.LateNightWorkingHour,
                    x.LegalHolidayWorkedHour,
                    x.RegularHolidayWorkedHour,
                    x.AnomalyHour,
                }).ToList());
        }

        internal static readonly string attendanceCSVData =
    @"202,20,1,160,0,0,0,0,0,0,0,0,33,0,65,8,0,0
1,19,0,152,0,0,0,0,0,0,0,0,0,0,0,0,19,0
201,19.5,0,156,0,0,0.5,0,0,0,0,0,0,0,0,0,15,0
214,18.5,1,148,0,0,1.5,0,0,0,0,0,5.5,0,0,1,0,0
446,18,0,116.5,0,0,2,0,0,0,0,0,0,0,0,0,0,0
254,15,0,102,0,4,0,0,0,0,0,5,0,0,0,0,0,0
";

        [DataTestMethod]
        [DataRow("")]
        [DataRow("\n")]
        public void 異常系_勤怠CSVファイルが0行の時例外を返すこと(string text)
        {
            // given
            // テストデータを読み込む
#pragma warning disable CA2208 // 引数の例外を正しくインスタンス化します
            using StreamReader reader = new StreamReader(
                new MemoryStream(
                    Encoding.UTF8.GetBytes(text)))
                ?? throw new ArgumentNullException(
                    "StreamReader作るときに失敗した");
#pragma warning restore CA2208 // 引数の例外を正しくインスタンス化します

            // when
            IEmployeeAttendanceRepository employeeAttendanceRepository
                = new EmployeeAttendanceRepository();
            void target()
            {
                _ = employeeAttendanceRepository.ReadAll(reader);
            }

            // then
            var msg = "CSVファイルにデータがありません";
            var ex = Assert.ThrowsException<AttendanceTableServiceException>(target);
            Assert.AreEqual(msg, ex.Message);
        }
    }
}