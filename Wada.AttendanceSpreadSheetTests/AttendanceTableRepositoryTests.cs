using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.AttendanceTableAggregation;
using Wada.AttendanceTableService.ValueObjects;
using Wada.Data.DesignDepartmentDataBase.Models;
using Wada.Data.DesignDepartmentDataBase.Models.DepartmentCompanyHolidayAggregation;
using Wada.Data.DesignDepartmentDataBase.Models.OwnCompanyCalendarAggregation;
using Wada.Data.DesignDepartmentDataBase.Models.ValueObjects;
using Wada.Data.OrderManagement.Models;
using Wada.Data.OrderManagement.Models.EmployeeAggregation;

namespace Wada.AttendanceSpreadSheet.Tests
{
    [TestClass()]
    public class AttendanceTableRepositoryTests
    {
        [TestMethod()]
        public void 正常系_勤怠表が読み込めること()
        {
            // given
            DotNetEnv.Env.Load(".env");

            IStreamOpener streamOpener = new StreamOpener();
            string path = DotNetEnv.Env.GetString("TEST_XLS_PATH");
            using Stream xlsStream = streamOpener.Open(path);

            Mock<IEmployeeRepository> employeeMock = new();
            employeeMock.Setup(x => x.FindByEmployeeNumberAsync(It.IsAny<uint>()))
                .ReturnsAsync(TestEmployeeFactory.Create());

            Mock< IDepartmentCompanyHolidayRepository> departHoliMock = new();
            var groupId = "__DUMMY__";
            departHoliMock.Setup(x => x.FindByDepartmentIdAsync(It.IsAny<uint>()))
                .ReturnsAsync(TestDepartmentCompanyHolidayFactory.Create(calendarGroupId: groupId));

            Mock<IOwnCompanyHolidayRepository> mock_holiday = new();
            mock_holiday.Setup(x => x.FindByYearMonthAsync(groupId, 2022, 5))
                .ReturnsAsync(new List<OwnCompanyHoliday>{
                    OwnCompanyHoliday.Reconstruct(groupId, DateTime.Parse("2022/5/1"),HolidayClassification.LegalHoliday),
                    OwnCompanyHoliday.Reconstruct(groupId, DateTime.Parse("2022/5/3"),HolidayClassification.RegularHoliday),
                    OwnCompanyHoliday.Reconstruct(groupId, DateTime.Parse("2022/5/4"),HolidayClassification.RegularHoliday),
                    OwnCompanyHoliday.Reconstruct(groupId, DateTime.Parse("2022/5/5"),HolidayClassification.RegularHoliday),
                    OwnCompanyHoliday.Reconstruct(groupId, DateTime.Parse("2022/5/6"),HolidayClassification.RegularHoliday),
                    OwnCompanyHoliday.Reconstruct(groupId, DateTime.Parse("2022/5/7"),HolidayClassification.RegularHoliday),
                    OwnCompanyHoliday.Reconstruct(groupId, DateTime.Parse("2022/5/8"),HolidayClassification.LegalHoliday),
                    OwnCompanyHoliday.Reconstruct(groupId, DateTime.Parse("2022/5/14"),HolidayClassification.RegularHoliday),
                    OwnCompanyHoliday.Reconstruct(groupId, DateTime.Parse("2022/5/15"),HolidayClassification.LegalHoliday),
                    OwnCompanyHoliday.Reconstruct(groupId, DateTime.Parse("2022/5/21"),HolidayClassification.RegularHoliday),
                    OwnCompanyHoliday.Reconstruct(groupId, DateTime.Parse("2022/5/22"),HolidayClassification.LegalHoliday),
                    OwnCompanyHoliday.Reconstruct(groupId, DateTime.Parse("2022/5/28"),HolidayClassification.RegularHoliday),
                    OwnCompanyHoliday.Reconstruct(groupId, DateTime.Parse("2022/5/29"),HolidayClassification.LegalHoliday),
                });

            // when
            IAttendanceTableRepository attendanceTableRepository =
                new AttendanceTableRepository(employeeMock.Object,
                                              departHoliMock.Object,
                                              mock_holiday.Object);
            int month = 5;
            var actual = attendanceTableRepository.ReadByMonth(xlsStream, month);

            // then
            uint employeeNumber = (uint)DotNetEnv.Env.GetInt("EMPLOYEE_NUMBER");
            AttendanceTable expected = TestAttendanceTableFactory.Create(
                employeeNumber,
                new AttendanceYear(2022),
                new AttendanceMonth(month),
                CreateTestRecords());
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.EmployeeNumber, actual.EmployeeNumber);
            Assert.AreEqual(expected.Year, actual.Year);
            Assert.AreEqual(expected.Month, actual.Month);
            CollectionAssert.AreEqual(expected.AttendanceRecords.ToList(), actual.AttendanceRecords.ToList());
            mock_holiday.Verify(x => x.FindByYearMonthAsync(groupId, It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }

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
    }
}