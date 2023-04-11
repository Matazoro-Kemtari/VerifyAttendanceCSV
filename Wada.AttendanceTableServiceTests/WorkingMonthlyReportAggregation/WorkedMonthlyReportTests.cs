using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.AttendanceTableService.AttendanceTableAggregation;
using Wada.AttendanceTableService.WorkingMonthlyReportAggregation;

namespace Wada.AttendanceTableServiceTests.WorkingMonthlyReportAggregation
{
    [TestClass()]
    public class WorkedMonthlyReportTests
    {
        [TestMethod()]
        public void 正常系_勤怠表から就業月報インスタンス作成できること()
        {
            // given

            // then
            AttendanceTable attendanceTable = TestAttendanceTableFactory.Create();
            static uint func(uint id) => 1500u;
            WorkedMonthlyReport actual = WorkedMonthlyReport.CreateForAttendanceTable(attendanceTable, func);

            // when
            Assert.AreEqual(1500u, actual.AttendancePersonalCode);
            Assert.AreEqual(15m, actual.AttendanceDay);
            Assert.AreEqual(2m, actual.HolidayWorkedDay);
            Assert.AreEqual(2m, actual.PaidLeaveDay);
            Assert.AreEqual(1m, actual.AbsenceDay);
            Assert.AreEqual(1m, actual.TransferedAttendanceDay);
            Assert.AreEqual(1m, actual.PaidSpecialLeaveDay);
            Assert.AreEqual(1m, actual.LatenessTime);
            Assert.AreEqual(1m, actual.EarlyLeaveTime);
            Assert.AreEqual(0m, actual.EducationDay);
            Assert.AreEqual(111m, actual.RegularWorkedHour);
            Assert.AreEqual(15m, actual.OvertimeHour);
            Assert.AreEqual(35m, actual.LateNightWorkingHour);
            Assert.AreEqual(8m, actual.LegalHolidayWorkedHour);
            Assert.AreEqual(8m, actual.RegularHolidayWorkedHour);
            Assert.AreEqual(49m, actual.AnomalyHour);
        }
    }
}