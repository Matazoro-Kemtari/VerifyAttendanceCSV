using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.AttendanceTableService.ValueObjects;

namespace Wada.AttendanceTableServiceTests.AttendanceTableAggregation
{
    [TestClass]
    public class AttendanceYearTests
    {
        [DataTestMethod]
        [DataRow(2000)]
        [DataRow(9999)]
        public void 正常系_2000から9999年はオブジェクト生成できること(int year)
        {
            // given
            // when
            AttendanceYear actual = new(year);

            // then
            Assert.AreEqual(year, actual.Value);
        }

        [DataTestMethod]
        [DataRow(1999)]
        [DataRow(10000)]
        public void 異常系_年範囲外は例外を返すこと(int year)
        {
            // given
            // when
            var target = () => new AttendanceYear(year);

            // then
            var msg = $"年の値は2000から9999の範囲を超えて指定できません value:{year}";
            var expectedEx = new ArgumentOutOfRangeException(msg);
            var actualEx = Assert.ThrowsException<ArgumentOutOfRangeException>(target);
            Assert.AreEqual(expectedEx.Message, actualEx.Message);
        }
    }

    [TestClass]
    public class AttendanceMonthTests
    {
        [DataTestMethod]
        [DataRow(1)]
        [DataRow(12)]
        public void 正常系_1から12月はオブジェクト生成できること(int month)
        {
            // given
            // when
            AttendanceMonth actual = new(month);

            // then
            Assert.AreEqual(month, actual.Value);
        }

        [DataTestMethod]
        [DataRow(-1)]
        [DataRow(0)]
        [DataRow(13)]
        public void 異常系_月範囲外は例外を返すこと(int month)
        {
            // given
            // when
            var target = () => new AttendanceMonth(month);

            // then
            var msg = $"月の値は1から12の範囲を超えて指定できません value:{month}";
            var expectedEx = new ArgumentOutOfRangeException(msg);
            var actualEx = Assert.ThrowsException<ArgumentOutOfRangeException>(target);
            Assert.AreEqual(expectedEx.Message, actualEx.Message);
        }
    }

    [TestClass]
    public class AttendanceDayTests
    {
        [DataTestMethod]
        [DataRow(2022, 7, 1)]
        [DataRow(2022, 7, 31)]
        public void 正常系_1から31日はオブジェクト生成できること(int year, int month, int day)
        {
            // given
            AttendanceYear attendanceYear = new(year);
            AttendanceMonth attendanceMonth = new(month);

            // when
            AttendanceDay actual = new(attendanceYear, attendanceMonth, day);

            // then
            Assert.AreEqual(day, actual.Value);
        }

        [DataTestMethod]
        [DataRow(2022, 7, 0, 31)]
        [DataRow(2022, 7, 32, 31)]
        [DataRow(2022, 2, 29, 28)]
        [DataRow(2022, 4, 31, 30)]
        public void 異常系_日範囲外は例外を返すこと(int year, int month, int day, int last)
        {
            // given
            AttendanceYear attendanceYear = new(year);
            AttendanceMonth attendanceMonth = new(month);

            // when
            var target = () => new AttendanceDay(attendanceYear, attendanceMonth, day);

            // then
            var msg = $"日の値は1から{last}の範囲を超えて指定できません year:{year}, month:{month}, day:{day}";
            var expectedEx = new ArgumentOutOfRangeException(msg);
            var actualEx = Assert.ThrowsException<ArgumentOutOfRangeException>(target);
            Assert.AreEqual(expectedEx.Message, actualEx.Message);
        }
    }

    [TestClass]
    public class AttendanceTimeTests
    {
        [DataTestMethod]
        [DataRow(0, 0)]
        [DataRow(1, 0)]
        [DataRow(29, 0)]
        [DataRow(30, 30)]
        [DataRow(59, 30)]
        public void 正常系_丸め処理されたオブジェクト生成できること(int real, int round)
        {
            // given
            // when
            DateTime val = new(2022, 4, 1, 8, real, 0);
            AttendanceTime actual = new(val);
            DateTime expected = new(2022, 4, 1, 8, round, 0);

            // then
            Assert.AreEqual(expected, actual.Value);
        }
    }
}