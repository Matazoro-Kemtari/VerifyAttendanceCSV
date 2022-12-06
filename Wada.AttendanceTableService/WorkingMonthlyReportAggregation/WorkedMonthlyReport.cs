using Wada.AttendanceTableService.AttendanceTableAggregation;
using Wada.AttendanceTableService.ValueObjects;

namespace Wada.AttendanceTableService.WorkingMonthlyReportAggregation
{
    [Equals(DoNotAddEqualityOperators = true), ToString]
    public class WorkedMonthlyReport
    {
        private WorkedMonthlyReport(
            uint attendancePersonalCode,
            decimal attendanceDay,
            decimal holidayWorkedDay,
            decimal paidLeaveDay,
            int absenceDay,
            decimal transferedAttendanceDay,
            int paidSpecialLeaveDay,
            int latenessTime,
            int earlyLeaveTime,
            decimal businessSuspensionDay,
            decimal educationDay,
            decimal regularWorkedHour,
            decimal overtimeHour,
            decimal lateNightWorkingHour,
            decimal legalHolidayWorkedHour,
            decimal regularHolidayWorkedHour,
            decimal anomalyHour,
            int lunchBoxOrderedTime)
        {
            ID = Ulid.NewUlid();
            AttendancePersonalCode = attendancePersonalCode;
            AttendanceDay = attendanceDay;
            HolidayWorkedDay = holidayWorkedDay;
            PaidLeaveDay = paidLeaveDay;
            AbsenceDay = absenceDay;
            TransferedAttendanceDay = transferedAttendanceDay;
            PaidSpecialLeaveDay = paidSpecialLeaveDay;
            LatenessTime = latenessTime;
            EarlyLeaveTime = earlyLeaveTime;
            BusinessSuspensionDay = businessSuspensionDay;
            EducationDay = educationDay;
            RegularWorkedHour = regularWorkedHour;
            OvertimeHour = overtimeHour;
            LateNightWorkingHour = lateNightWorkingHour;
            LegalHolidayWorkedHour = legalHolidayWorkedHour;
            RegularHolidayWorkedHour = regularHolidayWorkedHour;
            AnomalyHour = anomalyHour;
            LunchBoxOrderedTime = lunchBoxOrderedTime;
        }

        /// <summary>
        /// 勤怠表エクセルからのFactoryメソッド
        /// </summary>
        /// <param name="attendanceTable"></param>
        /// <param name="convertParsonalCode"></param>
        /// <returns></returns>
        public static WorkedMonthlyReport CreateForAttendanceTable(AttendanceTable attendanceTable, Func<uint, uint> convertParsonalCode)
        {
            int attendanceDay = CountAttendanceDay(attendanceTable);
            int holidayWorkedDay = CountHolidayWorkedDay(attendanceTable);
            decimal paidLeaveDay = CountPaidLeaveDay(attendanceTable);
            int absenceDay = CountAbsenceDay(attendanceTable);
            int transferedAttendanceDay = CountTransferedAttendanceDay(attendanceTable);
            int paidSpecialLeaveDay = CountPaidSpecialLeaveDay(attendanceTable);
            int latenessTime = CountLatenessTime(attendanceTable);
            int earlyLeaveTime = CounTearlyLeaveTime(attendanceTable);
            decimal businessSuspensionDay = CountBusinessSuspensionDay(attendanceTable);
            decimal regularWorkedHour = CountRegularWorkedHour(attendanceTable);
            decimal overtimeHour = CountOvertimeHour(attendanceTable);
            decimal lateNightWorkingHour = CountLateNightWorkingHour(attendanceTable);
            decimal legalHolidayWorkedHour = CountLegalHolidayWorkedHour(attendanceTable);
            decimal regularHolidayWorkedHour = CountRegularHolidayWorkedHour(attendanceTable);
            decimal anomalyHour = CountAnomalyHour(attendanceTable);
            int lunchBoxOrderedTime = CountLunchBoxOrderedTime(attendanceTable);

            return new(
                convertParsonalCode(attendanceTable.EmployeeNumber),
                attendanceDay,
                holidayWorkedDay,
                paidLeaveDay,
                absenceDay,
                transferedAttendanceDay,
                paidSpecialLeaveDay,
                latenessTime,
                earlyLeaveTime,
                businessSuspensionDay,
                0,// 教育日数は取得できないのでゼロ
                regularWorkedHour,
                overtimeHour,
                lateNightWorkingHour,
                legalHolidayWorkedHour,
                regularHolidayWorkedHour,
                anomalyHour,
                lunchBoxOrderedTime
                );
        }

        public static WorkedMonthlyReport CreateForAttendanceCSV(EmployeeAttendance employeeAttendance)
        {
            return new(
                employeeAttendance.AttendancePersonalCode,
                employeeAttendance.AttendanceDay,
                employeeAttendance.HolidayWorkedDay,
                employeeAttendance.PaidLeaveDay,
                employeeAttendance.AbsenceDay,
                employeeAttendance.TransferedAttendanceDay,
                employeeAttendance.PaidSpecialLeaveDay,
                employeeAttendance.LatenessTime,
                employeeAttendance.EarlyLeaveTime,
                employeeAttendance.BusinessSuspensionDay,
                employeeAttendance.EducationDay,
                employeeAttendance.RegularWorkedHour,
                employeeAttendance.OvertimeHour,
                employeeAttendance.LateNightWorkingHour,
                employeeAttendance.LegalHolidayWorkedHour,
                employeeAttendance.RegularHolidayWorkedHour,
                employeeAttendance.AnomalyHour,
                employeeAttendance.LunchBoxOrderedTime);
        }
        /// <summary>
        /// 弁当注文数を返す
        /// </summary>
        /// <param name="attendanceTable"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static int CountLunchBoxOrderedTime(AttendanceTable attendanceTable)
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
        private static decimal CountAnomalyHour(AttendanceTable attendanceTable)
        {
            // 計算対象(深夜)時間帯のリスト:計算対象日の00:00が起点
            List<TimeSpanPair> anomalyWorks = new() {
                new TimeSpanPair(){ TSStart = new TimeSpan(0,0,0), TSEnd = new TimeSpan(8,0,0) },   // 00:00-08:00
                new TimeSpanPair(){ TSStart = new TimeSpan(22,0,0), TSEnd = new TimeSpan(24,0,0) }  // 22:00-24:00
            };
            decimal anomalyHour = attendanceTable.AttendanceRecords
                .Where(x => x.StartedTime != null)
                .Where(x => x.EndedTime != null)
                .Sum(x =>
                (decimal)CalcTimeBetweenWork(
                    x.StartedTime!.Value, x.EndedTime!.Value, anomalyWorks
                    ).TotalHours);
            return anomalyHour;
        }

        /// <summary>
        /// 法定外休日出勤時間を返す
        /// </summary>
        /// <param name="attendanceTable"></param>
        /// <returns></returns>
        private static decimal CountRegularHolidayWorkedHour(AttendanceTable attendanceTable)
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
        private static decimal CountLegalHolidayWorkedHour(AttendanceTable attendanceTable)
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
                    if (!(workStart >= tspEnd || endTime <= tspStart))
                    {
                        tspStart = tspStart < workStart ? workStart : tspStart;   // 実際の開始時刻に調整
                        tspEnd = tspEnd > endTime ? endTime : tspEnd;             // 実際の終了時刻に調整
                        result += tspEnd - tspStart;                              // 計算対象時間帯の勤務時間を計算して報告値に加算
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
        private static decimal CountLateNightWorkingHour(AttendanceTable attendanceTable)
        {
            // 計算対象(深夜)時間帯のリスト:計算対象日の00:00が起点
            List<TimeSpanPair> nightWorks = new() {
                new TimeSpanPair(){ TSStart = new TimeSpan(0,0,0), TSEnd = new TimeSpan(5,0,0) },   // 00:00-05:00
                new TimeSpanPair(){ TSStart = new TimeSpan(22,0,0), TSEnd = new TimeSpan(24,0,0) }  // 22:00-24:00
            };
            TimeSpan lowerLateNight = new(22, 0, 0);
            TimeSpan higherLateNight = new(5, 0, 0);
            decimal lateNightWorkingHour = attendanceTable.AttendanceRecords
                .Where(x => x.StartedTime != null)
                .Where(x => x.EndedTime != null)
                .Sum(x =>
                (decimal)CalcTimeBetweenWork(
                    x.StartedTime!.Value, x.EndedTime!.Value, nightWorks
                    ).TotalHours);
            return lateNightWorkingHour;
        }

        /// <summary>
        /// 残業時間を返す
        /// </summary>
        /// <param name="attendanceTable"></param>
        /// <returns></returns>
        private static decimal CountOvertimeHour(AttendanceTable attendanceTable)
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
        private static decimal CountRegularWorkedHour(AttendanceTable attendanceTable)
        {
            return attendanceTable.AttendanceRecords
                .Where(x => x.StartedTime != null)
                .Where(x => x.EndedTime != null)
                .Where(x => x.DayOffClassification == DayOffClassification.None
                || x.DayOffClassification == DayOffClassification.TransferedAttendance
                || x.DayOffClassification == DayOffClassification.AMPaidLeave
                || x.DayOffClassification == DayOffClassification.PMPaidLeave
                || x.DayOffClassification == DayOffClassification.Lateness
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
        private static decimal CountBusinessSuspensionDay(AttendanceTable attendanceTable)
        {
            int businessSuspensionDays = attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.BusinessSuspension);
            int businessSuspensionHalfDays = attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.AMBusinessSuspension
                || x.DayOffClassification == DayOffClassification.PMBusinessSuspension);

            return businessSuspensionDays + businessSuspensionHalfDays * 0.5m;
        }

        /// <summary>
        /// 早退回数を返す
        /// </summary>
        /// <param name="attendanceTable"></param>
        /// <returns></returns>
        private static int CounTearlyLeaveTime(AttendanceTable attendanceTable)
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
        private static int CountLatenessTime(AttendanceTable attendanceTable)
        {
            return attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.Lateness);
        }

        /// <summary>
        /// 有休の特別休暇日数を返す
        /// </summary>
        /// <param name="attendanceTable"></param>
        /// <returns></returns>
        private static int CountPaidSpecialLeaveDay(AttendanceTable attendanceTable)
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
        private static int CountTransferedAttendanceDay(AttendanceTable attendanceTable)
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
        private static int CountAbsenceDay(AttendanceTable attendanceTable)
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
        private static decimal CountPaidLeaveDay(AttendanceTable attendanceTable)
        {
            int paidLeaveDays = attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.PaidLeave);
            int paidLeaveHalfDays = attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.AMPaidLeave
                || x.DayOffClassification == DayOffClassification.PMPaidLeave);
            return paidLeaveDays + paidLeaveHalfDays * 0.5m;
        }

        /// <summary>
        /// 休日出勤日数を返す (法定＋法定外)
        /// </summary>
        /// <param name="attendanceTable"></param>
        /// <returns></returns>
        private static int CountHolidayWorkedDay(AttendanceTable attendanceTable)
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
        private static int CountAttendanceDay(AttendanceTable attendanceTable)
        {
            return attendanceTable.AttendanceRecords
                .Count(x =>
                x.DayOffClassification == DayOffClassification.None
                || x.DayOffClassification == DayOffClassification.AMPaidLeave
                || x.DayOffClassification == DayOffClassification.PMPaidLeave
                || x.DayOffClassification == DayOffClassification.Lateness
                || x.DayOffClassification == DayOffClassification.EarlyLeave
                || x.DayOffClassification == DayOffClassification.TransferedAttendance
                || x.DayOffClassification == DayOffClassification.AMBusinessSuspension
                || x.DayOffClassification == DayOffClassification.PMBusinessSuspension);
        }

        public Ulid ID { get; }

        /// <summary>
        /// 勤怠個人コード
        /// </summary>
        public uint AttendancePersonalCode { get; init; }

        /// <summary>
        /// 出勤日数
        /// </summary>
        public decimal AttendanceDay { get; init; }

        /// <summary>
        /// 休日出勤数
        /// </summary>
        public decimal HolidayWorkedDay { get; init; }

        /// <summary>
        /// 有休日数
        /// </summary>
        public decimal PaidLeaveDay { get; init; }

        /// <summary>
        /// 欠勤日数
        /// </summary>
        public int AbsenceDay { get; init; }

        /// <summary>
        /// 振休出勤日数
        /// </summary>
        public decimal TransferedAttendanceDay { get; init; }

        /// <summary>
        /// 有休特別休暇日数
        /// </summary>
        public int PaidSpecialLeaveDay { get; init; }

        /// <summary>
        /// 遅刻回数
        /// </summary>
        public int LatenessTime { get; init; }

        /// <summary>
        /// 早退回数
        /// </summary>
        public int EarlyLeaveTime { get; init; }

        /// <summary>
        /// 休業日数
        /// </summary>
        public decimal BusinessSuspensionDay { get; init; }

        /// <summary>
        /// 教育日数
        /// </summary>
        public decimal EducationDay { get; } = 0m;

        /// <summary>
        /// 所定時間
        /// </summary>
        public decimal RegularWorkedHour { get; init; }

        /// <summary>
        /// (早出)残業時間
        /// </summary>
        public decimal OvertimeHour { get; init; }

        /// <summary>
        /// 深夜勤務時間
        /// </summary>
        public decimal LateNightWorkingHour { get; init; }

        /// <summary>
        /// 法定休出勤時間
        /// </summary>
        public decimal LegalHolidayWorkedHour { get; init; }

        /// <summary>
        /// 法定外休出勤時間
        /// </summary>
        public decimal RegularHolidayWorkedHour { get; init; }

        /// <summary>
        /// 変則時間
        /// </summary>
        public decimal AnomalyHour { get; init; }

        /// <summary>
        /// 弁当注文数
        /// </summary>
        public int LunchBoxOrderedTime { get; init; }
    }
}
