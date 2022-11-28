using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.AttendanceTableService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wada.AttendanceTableService.Tests
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
            Assert.AreEqual(attendanceTable.Year, actual.Year);
            Assert.AreEqual(attendanceTable.Month, actual.Month);
            Assert.AreEqual(1500u, actual.AttendancePersonalCode);
            Assert.AreEqual(22m, actual.AttendanceDays);
            Assert.AreEqual(2m, actual.HolidayWorkedDays);
            Assert.AreEqual(2m, actual.PaidLeaveDays);
            Assert.AreEqual(1m, actual.AbsenceDays);
            Assert.AreEqual(1m, actual.TransferedAttendanceDays);
            Assert.AreEqual(1m, actual.PaidSpecialLeaveDays);
            Assert.AreEqual(1m, actual.BeLateTimes);
            Assert.AreEqual(1m, actual.EarlyLeaveTimes);
            Assert.AreEqual(2m, actual.BusinessSuspensionDays);
            Assert.AreEqual(0m, actual.EducationDays);
            Assert.AreEqual(115m, actual.RegularWorkedHours);
            Assert.AreEqual(15m, actual.OvertimeHours);
            Assert.AreEqual(35m, actual.LateNightWorkingHours);
            Assert.AreEqual(8m, actual.LegalHolidayWorkedHours);
            Assert.AreEqual(8m, actual.RegularHolidayWorkedHours);
            Assert.AreEqual(49m, actual.AnomalyHourd);
            Assert.AreEqual(16, actual.LunchBoxOrderedTimes);
        }
    }
}