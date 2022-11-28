using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            int attendanceDays = CountAttendanceDays(attendanceTable);
            int holidayWorkedDays = CountHolidayWorkedDays(attendanceTable);
            decimal paidLeaveDays = CountPaidLeaveDays(attendanceTable);
            int absenceDays = CountAbsenceDays(attendanceTable);
            int transferedAttendanceDays = CountTransferedAttendanceDays(attendanceTable);
            int paidSpecialLeaveDays = CountPaidSpecialLeaveDays(attendanceTable);
            int beLateTimes = CountBeLateTimes(attendanceTable);
            int earlyLeaveTimes = CounTearlyLeaveTimes(attendanceTable);
            decimal businessSuspensionDays = CountBusinessSuspensionDays(attendanceTable);
            decimal regularWorkedHours = CountRegularWorkedHours(attendanceTable);
            decimal overtimeHours = CountOvertimeHours(attendanceTable);
            decimal lateNightWorkingHours = CountLateNightWorkingHours(attendanceTable);
            decimal legalHolidayWorkedHours = CountLegalHolidayWorkedHours(attendanceTable);
            decimal regularHolidayWorkedHours = CountRegularHolidayWorkedHours(attendanceTable);
            decimal anomalyHourd = CountAnomalyHourd(attendanceTable);
            int lunchBoxOrderedTimes = CountLunchBoxOrderedTimes(attendanceTable);

            return new(
                attendanceTable.Year,
                attendanceTable.Month,
                convertParsonalCode(attendanceTable.EmployeeNumber),
                attendanceDays,
                holidayWorkedDays,
                paidLeaveDays,
                absenceDays,
                transferedAttendanceDays,
                paidSpecialLeaveDays,
                beLateTimes,
                earlyLeaveTimes,
                businessSuspensionDays,
                0,// 教育日数は取得できないのでゼロ
                regularWorkedHours,
                overtimeHours,
                lateNightWorkingHours,
                legalHolidayWorkedHours,
                regularHolidayWorkedHours,
                anomalyHourd,
                lunchBoxOrderedTimes
                );
        }

        /// <summary>
        /// 弁当注文数を返す
        /// </summary>
        /// <param name="attendanceTable"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static int CountLunchBoxOrderedTimes(AttendanceTable attendanceTable)
        {
            return attendanceTable.AttendanceRecords
                .Count(x => x.OrderedLunchBox == OrderedLunchBox.Orderd);
        }

        /// <summary>
        /// 変則時間を返す
        /// </summary>
        /// <param name="attendanceTable"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static decimal CountAnomalyHourd(AttendanceTable attendanceTable)
        {
            // 計算対象(深夜)時間帯のリスト:計算対象日の00:00が起点
            List<TimeSpanPair> anomalyWorks = new() {
                new TimeSpanPair(){ TSStart = new TimeSpan(0,0,0), TSEnd = new TimeSpan(8,0,0) },   // 00:00-08:00
                new TimeSpanPair(){ TSStart = new TimeSpan(22,0,0), TSEnd = new TimeSpan(24,0,0) }  // 22:00-24:00
            };
            decimal anomalyHourd = attendanceTable.AttendanceRecords
                .Where(x => x.StartedTime != null)
                .Where(x => x.EndedTime != null)
                .Sum(x =>
                (decimal)CalcTimeBetweenWork(
                    x.StartedTime!.Value, x.EndedTime!.Value, anomalyWorks
                    ).TotalHours);
            return anomalyHourd;
        }

        /// <summary>
        /// 法定外休日出勤時間を返す
        /// </summary>
        /// <param name="attendanceTable"></param>
        /// <returns></returns>
        private static decimal CountRegularHolidayWorkedHours(AttendanceTable attendanceTable)
        {
            return attendanceTable.AttendanceRecords
                .Where(x => x.DayOffClassification == DayOffClassification.HolidayWorked)
                .Where(x => x.HolidayClassification == HolidayClassification.RegularHoliday)
                .Sum(x =>
                {
                    TimeSpan t = new();
                    if (x.StartedTime != null && x.EndedTime != null)
                        t = x.EndedTime.Value - x.StartedTime.Value;
                    if (x.RestTime.HasValue)
                        t -= x.RestTime.Value;
                    return (decimal)t.TotalHours;
                });
        }

        /// <summary>
        /// 法定休日出勤時間を返す
        /// </summary>
        /// <param name="attendanceTable"></param>
        /// <returns></returns>
        private static decimal CountLegalHolidayWorkedHours(AttendanceTable attendanceTable)
        {
            return attendanceTable.AttendanceRecords
                .Where(x => x.DayOffClassification == DayOffClassification.HolidayWorked)
                .Where(x => x.HolidayClassification == HolidayClassification.LegalHoliday)
                .Sum(x =>
                {
                    TimeSpan t = new();
                    if (x.StartedTime != null && x.EndedTime != null)
                        t = x.EndedTime.Value - x.StartedTime.Value;
                    if (x.RestTime.HasValue)
                        t -= x.RestTime.Value;
                    return (decimal)t.TotalHours;
                });
        }

        // 計算対象時間帯の開始・終了時刻のペア情報クラス
        public class TimeSpanPair
        {
            public TimeSpan TSStart { get; set; }
            public TimeSpan TSEnd { get; set; }
        }

        /// <summary>
        /// 深夜勤務時間取得(複数日をまたぐ連続勤務も計算可)
        /// </summary>
        /// <param name="startTime">出勤日付時刻</param>
        /// <param name="endTime">退勤日付時刻</param>
        /// <returns>深夜勤務時間</returns>
        private static TimeSpan CalcTimeBetweenWork(DateTime startTime, DateTime endTime, List<TimeSpanPair> timeBetweens)
        {
            TimeSpan result = new(0, 0, 0, 0); // 深夜勤務時間報告初期値00:00
            DateTime workStart = startTime;             // 該当日毎の勤務開始時刻
            do
            {
                DateTime workDate = workStart.Date;          // 該当日の00:00を計算ベース値として設定
                                                             // 計算対象(深夜)時間帯リストの要素数でループする
                foreach (TimeSpanPair tsp in timeBetweens)
                {
                    DateTime tspStart = workDate.Add(tsp.TSStart); // 計算対象時間帯の開始時刻設定
                    DateTime tspEnd = workDate.Add(tsp.TSEnd);     // 計算対象時間帯の終了時刻設定

                    // 勤務時間が計算対象時間帯に入っているか判定
                    if (!((workStart >= tspEnd) || (endTime <= tspStart)))
                    {
                        tspStart = (tspStart < workStart) ? workStart : tspStart;   // 実際の開始時刻に調整
                        tspEnd = (tspEnd > endTime) ? endTime : tspEnd;             // 実際の終了時刻に調整
                        result += (tspEnd - tspStart);                              // 計算対象時間帯の勤務時間を計算して報告値に加算
                    }
                }
                workStart = workDate.AddDays(1.0);  // 翌日の勤務開始時刻を00:00とする
            } while (workStart < endTime);          // 勤務時間が残っている間はループする
            return result;
        }

        /// <summary>
        /// 深夜残業時間を返す
        /// </summary>
        /// <param name="attendanceTable"></param>
        /// <returns></returns>
        private static decimal CountLateNightWorkingHours(AttendanceTable attendanceTable)
        {
            // 計算対象(深夜)時間帯のリスト:計算対象日の00:00が起点
            List<TimeSpanPair> nightWorks = new() {
                new TimeSpanPair(){ TSStart = new TimeSpan(0,0,0), TSEnd = new TimeSpan(5,0,0) },   // 00:00-05:00
                new TimeSpanPair(){ TSStart = new TimeSpan(22,0,0), TSEnd = new TimeSpan(24,0,0) }  // 22:00-24:00
            };
            TimeSpan lowerLateNight = new(22, 0, 0);
            TimeSpan higherLateNight = new(5, 0, 0);
            decimal lateNightWorkingHours = attendanceTable.AttendanceRecords
                .Where(x => x.StartedTime != null)
                .Where(x => x.EndedTime != null)
                .Sum(x =>
                (decimal)CalcTimeBetweenWork(
                    x.StartedTime!.Value, x.EndedTime!.Value, nightWorks
                    ).TotalHours);
            return lateNightWorkingHours;
        }

        /// <summary>
        /// 残業時間を返す
        /// </summary>
        /// <param name="attendanceTable"></param>
        /// <returns></returns>
        private static decimal CountOvertimeHours(AttendanceTable attendanceTable)
        {
            return attendanceTable.AttendanceRecords
                .Where(x => x.DayOffClassification == DayOffClassification.None
                || x.DayOffClassification == DayOffClassification.TransferedAttendance)
                .Sum(x =>
                {
                    TimeSpan t = new();
                    if (x.StartedTime != null && x.EndedTime != null)
                        t = x.EndedTime.Value - x.StartedTime.Value;
                    if (x.RestTime.HasValue)
                        t -= x.RestTime.Value;
                    return t.TotalHours > 8d ? (decimal)t.TotalHours - 8m : 0m;
                });
        }

        /// <summary>
        /// 所定時間を返す
        /// </summary>
        /// <param name="attendanceTable"></param>
        /// <returns></returns>
        private static decimal CountRegularWorkedHours(AttendanceTable attendanceTable)
        {
            return attendanceTable.AttendanceRecords
                .Where(x => x.StartedTime != null)
                .Where(x => x.EndedTime != null)
                .Where(x => x.DayOffClassification == DayOffClassification.None
                || x.DayOffClassification == DayOffClassification.TransferedAttendance
                || x.DayOffClassification == DayOffClassification.AMPaidLeave
                || x.DayOffClassification == DayOffClassification.PMPaidLeave
                || x.DayOffClassification == DayOffClassification.BeLate
                || x.DayOffClassification == DayOffClassification.EarlyLeave
                || x.DayOffClassification == DayOffClassification.AMBusinessSuspension
                || x.DayOffClassification == DayOffClassification.PMBusinessSuspension)
                .Sum(x =>
                {
                    TimeSpan t = x.EndedTime!.Value - x.StartedTime!.Value;

                    if (x.RestTime.HasValue)
                        t -= x.RestTime.Value;

                    return t.TotalHours < 8d ? (decimal)t.TotalHours : 8m;
                });
        }

        /// <summary>
        /// 休業日数を返す
        /// </summary>
        /// <param name="attendanceTable"></param>
        /// <returns></returns>
        private static decimal CountBusinessSuspensionDays(AttendanceTable attendanceTable)
        {
            int businessSuspensionDays = attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.BusinessSuspension);
            int businessSuspensionHalfDays = attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.AMBusinessSuspension
                || x.DayOffClassification == DayOffClassification.PMBusinessSuspension);

            return businessSuspensionDays + (businessSuspensionHalfDays * 0.5m);
        }

        /// <summary>
        /// 早退回数を返す
        /// </summary>
        /// <param name="attendanceTable"></param>
        /// <returns></returns>
        private static int CounTearlyLeaveTimes(AttendanceTable attendanceTable)
        {
            return attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.EarlyLeave);
        }

        /// <summary>
        /// 遅刻回数を返す
        /// </summary>
        /// <param name="attendanceTable"></param>
        /// <returns></returns>
        private static int CountBeLateTimes(AttendanceTable attendanceTable)
        {
            return attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.BeLate);
        }

        /// <summary>
        /// 有休の特別休暇日数を返す
        /// </summary>
        /// <param name="attendanceTable"></param>
        /// <returns></returns>
        private static int CountPaidSpecialLeaveDays(AttendanceTable attendanceTable)
        {
            return attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.PaidSpecialLeave);
        }

        /// <summary>
        /// 振替出勤日数を返す
        /// </summary>
        /// <param name="attendanceTable"></param>
        /// <returns></returns>
        private static int CountTransferedAttendanceDays(AttendanceTable attendanceTable)
        {
            return attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.TransferedAttendance);
        }

        /// <summary>
        /// 欠勤日数を返す
        /// </summary>
        /// <param name="attendanceTable"></param>
        /// <returns></returns>
        private static int CountAbsenceDays(AttendanceTable attendanceTable)
        {
            return attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.Absence);
        }

        /// <summary>
        /// 有給休暇日数を返す
        /// </summary>
        /// <param name="attendanceTable"></param>
        /// <returns></returns>
        private static decimal CountPaidLeaveDays(AttendanceTable attendanceTable)
        {
            int paidLeaveDays = attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.PaidLeave);
            int paidLeaveHalfDays = attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.AMPaidLeave
                || x.DayOffClassification == DayOffClassification.PMPaidLeave);
            return paidLeaveDays + (paidLeaveHalfDays * 0.5m);
        }

        /// <summary>
        /// 休日出勤日数を返す (法定＋法定外)
        /// </summary>
        /// <param name="attendanceTable"></param>
        /// <returns></returns>
        private static int CountHolidayWorkedDays(AttendanceTable attendanceTable)
        {
            return attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.HolidayWorked);
        }

        /// <summary>
        /// 出勤日数を返す
        /// </summary>
        /// <param name="attendanceTable"></param>
        /// <returns></returns>
        private static int CountAttendanceDays(AttendanceTable attendanceTable)
        {
            return attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.None
                || x.DayOffClassification == DayOffClassification.AMPaidLeave
                || x.DayOffClassification == DayOffClassification.PMPaidLeave
                || x.DayOffClassification == DayOffClassification.BeLate
                || x.DayOffClassification == DayOffClassification.EarlyLeave
                || x.DayOffClassification == DayOffClassification.TransferedAttendance
                || x.DayOffClassification == DayOffClassification.AMBusinessSuspension
                || x.DayOffClassification == DayOffClassification.PMBusinessSuspension);
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
        public decimal EducationDays { get; } = 0m;

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
