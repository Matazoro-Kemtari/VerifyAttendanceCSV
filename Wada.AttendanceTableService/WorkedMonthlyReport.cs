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
        private WorkedMonthlyReport(
            AttendanceYear year,
            AttendanceMonth month,
            uint attendancePersonalCode,
            decimal attendanceDays,
            decimal holidayWorkedDays,
            decimal paidLeaveDays,
            int absenceDays,
            decimal transferedAttendanceDays,
            int paidSpecialLeaveDays,
            int beLateTimes,
            int earlyLeaveTimes,
            decimal businessSuspensionDays,
            decimal educationDays,
            decimal regularWorkedHours,
            decimal overtimeHours,
            decimal lateNightWorkingHours,
            decimal legalHolidayWorkedHours,
            decimal regularHolidayWorkedHours,
            decimal anomalyHourd,
            int lunchBoxOrderedTimes)
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
            BeLateTimes = beLateTimes;
            EarlyLeaveTimes = earlyLeaveTimes;
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
            int attendanceDays = attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.None
                || x.DayOffClassification == DayOffClassification.AMPaidLeave
                || x.DayOffClassification == DayOffClassification.PMPaidLeave
                || x.DayOffClassification == DayOffClassification.BeLate
                || x.DayOffClassification == DayOffClassification.EarlyLeave
                || x.DayOffClassification == DayOffClassification.TransferedAttendance
                || x.DayOffClassification == DayOffClassification.AMBusinessSuspension
                || x.DayOffClassification == DayOffClassification.PMBusinessSuspension);

            int holidayWorkedDays = attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.HolidayWorked);

            int paidLeaveDays = attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.PaidLeave);

            int paidLeaveHalfDays = attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.AMPaidLeave
                || x.DayOffClassification == DayOffClassification.PMPaidLeave);

            int absenceDays = attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.Absence);

            int transferedAttendanceDays = attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.TransferedAttendance);

            int paidSpecialLeaveDays = attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.PaidSpecialLeave);

            int beLateTimes = attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.BeLate);

            int earlyLeaveTimes = attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.EarlyLeave);

            int businessSuspensionDays = attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.BusinessSuspension);

            int businessSuspensionHalfDays = attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.AMBusinessSuspension
                || x.DayOffClassification == DayOffClassification.PMBusinessSuspension);

            decimal regularWorkedHours = attendanceTable.AttendanceRecords
                .Where(x => x.DayOffClassification == DayOffClassification.None
                || x.DayOffClassification == DayOffClassification.TransferedAttendance)
                .Sum(x =>
                {
                    TimeSpan t = x.EndedTime.Value - x.StartedTime.Value;
                    return t.TotalHours <= 8 ? (decimal)t.TotalHours : 8;
                });

            decimal overtimeHours =

            return new(
                attendanceTable.Year,
                attendanceTable.Month,
                convertParsonalCode(attendanceTable.EmployeeNumber),
                attendanceDays,
                holidayWorkedDays,
                paidLeaveDays + (paidLeaveHalfDays * 0.5f),
                absenceDays,
                transferedAttendanceDays,
                paidSpecialLeaveDays,
                beLateTimes,
                earlyLeaveTimes,
                businessSuspensionDays + (businessSuspensionHalfDays * 0.5f),
                0,// 教育日数は取得できないのでゼロ
                regularWorkedHours,
                );
            //throw new NotImplementedException();
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
        public decimal AttendanceDays { get; init; }

        /// <summary>
        /// 休日出勤数
        /// </summary>
        public decimal HolidayWorkedDays { get; init; }

        /// <summary>
        /// 有休日数
        /// </summary>
        public decimal PaidLeaveDays { get; init; }

        /// <summary>
        /// 欠勤日数
        /// </summary>
        public int AbsenceDays { get; init; }

        /// <summary>
        /// 振休出勤日数
        /// </summary>
        public decimal TransferedAttendanceDays { get; init; }

        /// <summary>
        /// 有休特別休暇日数
        /// </summary>
        public int PaidSpecialLeaveDays { get; init; }

        /// <summary>
        /// 遅刻回数
        /// </summary>
        public int BeLateTimes { get; init; }

        /// <summary>
        /// 早退回数
        /// </summary>
        public int EarlyLeaveTimes { get; init; }

        /// <summary>
        /// 休業日数
        /// </summary>
        public decimal BusinessSuspensionDays { get; init; }

        /// <summary>
        /// 教育日数
        /// </summary>
        public decimal EducationDays { get; } = 0f;

        /// <summary>
        /// 所定時間
        /// </summary>
        public decimal RegularWorkedHours { get; init; }

        /// <summary>
        /// (早出)残業時間
        /// </summary>
        public decimal OvertimeHours { get; init; }

        /// <summary>
        /// 深夜勤務時間
        /// </summary>
        public decimal LateNightWorkingHours { get; init; }

        /// <summary>
        /// 法定休出勤時間
        /// </summary>
        public decimal LegalHolidayWorkedHours { get; init; }

        /// <summary>
        /// 法定外休出勤時間
        /// </summary>
        public decimal RegularHolidayWorkedHours { get; init; }

        /// <summary>
        /// 変則時間
        /// </summary>
        public decimal AnomalyHourd { get; init; }

        /// <summary>
        /// 弁当注文数
        /// </summary>
        public int LunchBoxOrderedTimes { get; init; }
    }
}
