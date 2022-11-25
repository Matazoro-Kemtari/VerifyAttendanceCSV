using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wada.AttendanceTableService
{
    [Equals(DoNotAddEqualityOperators = true), ToString]
    public class WorkedMonthlyReport
    {
        private WorkedMonthlyReport(AttendanceYear year, AttendanceMonth month, uint attendancePersonalCode, float attendanceDays, float holidayWorkedDays, float paidLeaveDays, int absenceDays, float transferedAttendanceDays, int paidSpecialLeaveDays, int beLateDays, int earlyLeaveDays, float businessSuspensionDays, float educationDays, float regularWorkedHours, float overtimeHours, float lateNightWorkingHours, float legalHolidayWorkedHours, float regularHolidayWorkedHours, float anomalyHourd, int lunchBoxOrderedTimes)
        {
            ID = Ulid.NewUlid();
            Year = year ?? throw new ArgumentNullException(nameof(year));
            Month = month ?? throw new ArgumentNullException(nameof(month));
            AttendancePersonalCode = attendancePersonalCode;
            AttendanceDays = attendanceDays;
            HolidayWorkedDays = holidayWorkedDays;
            PaidLeaveDays = paidLeaveDays;
            AbsenceDays = absenceDays;
            TransferedAttendanceDays = transferedAttendanceDays;
            PaidSpecialLeaveDays = paidSpecialLeaveDays;
            BeLateDays = beLateDays;
            EarlyLeaveDays = earlyLeaveDays;
            BusinessSuspensionDays = businessSuspensionDays;
            EducationDays = educationDays;
            RegularWorkedHours = regularWorkedHours;
            OvertimeHours = overtimeHours;
            LateNightWorkingHours = lateNightWorkingHours;
            LegalHolidayWorkedHours = legalHolidayWorkedHours;
            RegularHolidayWorkedHours = regularHolidayWorkedHours;
            AnomalyHourd = anomalyHourd;
            LunchBoxOrderedTimes = lunchBoxOrderedTimes;
        }

        public static WorkedMonthlyReport CreateForAttendanceTable(AttendanceTable attendanceTable, Func<uint, uint> convertParsonalCode)
        {
            var attendanceDays = attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.None
                || x.DayOffClassification == DayOffClassification.AMPaidLeave
                || x.DayOffClassification == DayOffClassification.PMPaidLeave
                || x.DayOffClassification == DayOffClassification.BeLate
                || x.DayOffClassification == DayOffClassification.EarlyLeave);
            var attendanceHalfDays = attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.AMPaidLeave
                || x.DayOffClassification == DayOffClassification.PMPaidLeave
                || x.DayOffClassification == DayOffClassification.
            return new(
                attendanceTable.Year,
                attendanceTable.Month,
                convertParsonalCode(attendanceTable.EmployeeNumber),
                attendanceDays);
            throw new NotImplementedException();
        }

        public Ulid ID { get; }

        /// <summary>
        /// 年
        /// </summary>
        public AttendanceYear Year { get; init; }

        /// <summary>
        /// 月
        /// </summary>
        public AttendanceMonth Month { get; init; }

        /// <summary>
        /// 勤怠個人コード
        /// </summary>
        public uint AttendancePersonalCode { get; init; }

        /// <summary>
        /// 出勤日数
        /// </summary>
        public float AttendanceDays { get; init; }

        /// <summary>
        /// 休日出勤数
        /// </summary>
        public float HolidayWorkedDays { get; init; }

        /// <summary>
        /// 有休日数
        /// </summary>
        public float PaidLeaveDays { get; init; }

        /// <summary>
        /// 欠勤日数
        /// </summary>
        public int AbsenceDays { get; init; }

        /// <summary>
        /// 振休出勤日数
        /// </summary>
        public float TransferedAttendanceDays { get; init; }

        /// <summary>
        /// 有休特別休暇日数
        /// </summary>
        public int PaidSpecialLeaveDays { get; init; }

        /// <summary>
        /// 遅刻回数
        /// </summary>
        public int BeLateDays { get; init; }

        /// <summary>
        /// 早退回数
        /// </summary>
        public int EarlyLeaveDays { get; init; }

        /// <summary>
        /// 休業日数
        /// </summary>
        public float BusinessSuspensionDays { get; init; }

        /// <summary>
        /// 教育日数
        /// </summary>
        public float EducationDays { get; } = 0f;

        /// <summary>
        /// 所定時間
        /// </summary>
        public float RegularWorkedHours { get; init; }

        /// <summary>
        /// (早出)残業時間
        /// </summary>
        public float OvertimeHours { get; init; }

        /// <summary>
        /// 深夜勤務時間
        /// </summary>
        public float LateNightWorkingHours { get; init; }

        /// <summary>
        /// 法定休出勤時間
        /// </summary>
        public float LegalHolidayWorkedHours { get; init; }

        /// <summary>
        /// 法定外休出勤時間
        /// </summary>
        public float RegularHolidayWorkedHours { get; init; }

        /// <summary>
        /// 変則時間
        /// </summary>
        public float AnomalyHourd { get; init; }

        /// <summary>
        /// 弁当注文数
        /// </summary>
        public int LunchBoxOrderedTimes { get; init; }
    }
}
