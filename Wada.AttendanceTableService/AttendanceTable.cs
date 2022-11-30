namespace Wada.AttendanceTableService
{
    [Equals(DoNotAddEqualityOperators = true), ToString]
    public class AttendanceTable
    {
        public AttendanceTable(uint employeeNumber, AttendanceYear year, AttendanceMonth month)
        {
            ID = Ulid.NewUlid();
            EmployeeNumber = employeeNumber;
            Year = year ?? throw new ArgumentNullException(nameof(year));
            Month = month ?? throw new ArgumentNullException(nameof(month));
            AttendanceRecords = new List<AttendanceRecord>();
        }

        public Ulid ID { get; }

        /// <summary>
        /// 社員番号
        /// </summary>
        [IgnoreDuringEquals]
        public uint EmployeeNumber { get; init; }

        /// <summary>
        /// 年
        /// </summary>
        public AttendanceYear Year { get; init; }

        /// <summary>
        /// 月
        /// </summary>
        public AttendanceMonth Month { get; init; }

        /// <summary>
        /// 勤怠
        /// </summary>
        public ICollection<AttendanceRecord> AttendanceRecords { get; init; }
    }

    public record class AttendanceYear
    {
        public AttendanceYear(int value)
        {
            if (value < 2000 || value > 9999)
            {
                var msg = $"年の値は2000から9999の範囲を超えて指定できません value:{value}";
                throw new ArgumentOutOfRangeException(msg);
            }

            Value = value;
        }

        public int Value { get; init; }
    }

    public record class AttendanceMonth
    {
        public AttendanceMonth(int value)
        {
            if (value < 1 || value > 12)
            {
                var msg = $"月の値は1から12の範囲を超えて指定できません value:{value}";
                throw new ArgumentOutOfRangeException(msg);
            }

            Value = (int)value;
        }

        public int Value { get; init; }
    }

    public record class AttendanceDay
    {
        public AttendanceDay(AttendanceYear year, AttendanceMonth month, int day)
        {
            var last = new DateTime(year.Value, month.Value, 1).AddMonths(1).AddDays(-1).Day;
            if (day < 1 || day > last)
            {
                var msg = $"日の値は1から{last}の範囲を超えて指定できません year:{year.Value}, month:{month.Value}, day:{day}";
                throw new ArgumentOutOfRangeException(msg);
            }

            Value = day;
        }

        public int Value { get; init; }
    }

    [Equals(DoNotAddEqualityOperators = true), ToString]
    public class AttendanceRecord
    {
        public AttendanceRecord(AttendanceDay attendanceDay, HolidayClassification holidayClassification, DayOffClassification dayOffClassification, AttendanceTime? startedTime, AttendanceTime? endedTime, TimeSpan? restTime, OrderedLunchBox orderedLunchBox)
        {
            AttendanceDay = attendanceDay ?? throw new ArgumentNullException(nameof(attendanceDay));
            HolidayClassification = holidayClassification;
            DayOffClassification = dayOffClassification;
            StartedTime = startedTime;
            EndedTime = endedTime;
            RestTime = restTime;
            OrderedLunchBox = orderedLunchBox;
        }

        /// <summary>
        /// 勤怠日
        /// </summary>
        public AttendanceDay AttendanceDay { get; init; }

        /// <summary>
        /// 休日区分 カレンダー上の定義 振替を追従はしていない
        /// </summary>
        [IgnoreDuringEquals]
        public HolidayClassification HolidayClassification { get; init; }

        /// <summary>
        /// 休暇区分
        /// </summary>
        [IgnoreDuringEquals]
        public DayOffClassification DayOffClassification { get; init; }

        /// <summary>
        /// 始業時間
        /// </summary>
        [IgnoreDuringEquals]
        public AttendanceTime? StartedTime { get; init; }

        /// <summary>
        /// 終業時間
        /// </summary>
        [IgnoreDuringEquals]
        public AttendanceTime? EndedTime { get; init; }

        /// <summary>
        /// 休憩時間
        /// </summary>
        public TimeSpan? RestTime { get; init; }

        /// <summary>
        /// 弁当注文
        /// </summary>
        [IgnoreDuringEquals]
        public OrderedLunchBox OrderedLunchBox { get; init; }
    }

    public enum HolidayClassification
    {
        None,

        /// <summary>
        /// 法定休日
        /// </summary>
        LegalHoliday,

        /// <summary>
        /// 法定外休日
        /// </summary>
        RegularHoliday,
    }

    public enum DayOffClassification
    {
        None,

        /// <summary>
        /// 有給休暇
        /// </summary>
        PaidLeave,

        /// <summary>
        /// 午前有給休暇
        /// </summary>
        AMPaidLeave,

        /// <summary>
        /// 午後有給休暇
        /// </summary>
        PMPaidLeave,

        /// <summary>
        /// 振替休日
        /// </summary>
        SubstitutedHoliday,

        /// <summary>
        /// 振替出勤
        /// </summary>
        TransferedAttendance,

        /// <summary>
        /// 休日出勤
        /// </summary>
        HolidayWorked,

        /// <summary>
        /// 特別休暇(有給)
        /// </summary>
        PaidSpecialLeave,

        /// <summary>
        /// 特別休暇(無給)
        /// </summary>
        UnpaidSpecialLeave,

        /// <summary>
        /// 欠勤
        /// </summary>
        Absence,

        /// <summary>
        /// 遅刻
        /// </summary>
        Lateness,

        /// <summary>
        /// 早退
        /// </summary>
        EarlyLeave,

        /// <summary>
        /// 休業
        /// </summary>
        BusinessSuspension,

        /// <summary>
        /// 午前休業
        /// </summary>
        AMBusinessSuspension,

        /// <summary>
        /// 午後休業
        /// </summary>
        PMBusinessSuspension,
    }

    public record class AttendanceTime
    {
        public AttendanceTime(DateTime value)
        {
            // 丸め処理
            var min = value.Minute;
            if (min > 0 && min < 30)
                min = 0;
            else if (min > 30)
                min = 30;
            var hour = value.Hour;

            Value = value.Date.AddHours(hour).AddMinutes(min);
        }

        public DateTime Value { get; init; }
    }

    public enum OrderedLunchBox
    {
        None,
        Orderd,
    }

    public class TestAttendanceTableFactory
    {
        public static AttendanceTable Create(
            uint employeeNumber = 500,
            AttendanceYear? year = null,
            AttendanceMonth? month = null,
            ICollection<AttendanceRecord>? AttendanceRecords = null)
        {
            year ??= new AttendanceYear(2022);
            month ??= new AttendanceMonth(5);
            AttendanceTable at = new(employeeNumber, year, month);
            if (AttendanceRecords == null)
                AddRecord(at.AttendanceRecords);
            else
                AttendanceRecords.ToList().ForEach(x => at.AttendanceRecords.Add(x));

            return at;
        }

        static void AddRecord(ICollection<AttendanceRecord> AttendanceRecords)
        {
            Dictionary<int, DayOffClassification> dayOffMap = new()
            {
                {1,DayOffClassification.None},
                {2,DayOffClassification.PaidLeave},
                {3,DayOffClassification.AMPaidLeave},
                {4,DayOffClassification.TransferedAttendance},
                {5,DayOffClassification.HolidayWorked},
                {6,DayOffClassification.PMPaidLeave},
                {7,DayOffClassification.SubstitutedHoliday},
                {8,DayOffClassification.PaidSpecialLeave},
                {9,DayOffClassification.UnpaidSpecialLeave},
                {10,DayOffClassification.Absence},
                {11,DayOffClassification.HolidayWorked},
                {12,DayOffClassification.None},
                {13,DayOffClassification.Lateness},
                {14,DayOffClassification.EarlyLeave},
                {15,DayOffClassification.BusinessSuspension},
                {16,DayOffClassification.AMBusinessSuspension},
                {17,DayOffClassification.PMBusinessSuspension},
                {18,DayOffClassification.None},
                {19,DayOffClassification.None},
                {20,DayOffClassification.None},
                {21,DayOffClassification.None},
                {22,DayOffClassification.None},
                {23,DayOffClassification.None},
                {24,DayOffClassification.None},
                {25,DayOffClassification.None},
                {26,DayOffClassification.None},
                {27,DayOffClassification.None},
                {28,DayOffClassification.None},
                {29,DayOffClassification.None},
                {30,DayOffClassification.None},
            };
            Dictionary<int, DateTime?[]> attendance = new()
            {
                {1,new DateTime?[] {DateTime.Parse("2022/06/01 08:00:00"),(DateTime?)DateTime.Parse("2022/06/01 17:00:00") }},
                {2,new DateTime?[] {null,null}},
                {3,new DateTime?[] {DateTime.Parse("2022/06/03 13:00:00"),DateTime.Parse("2022/06/03 17:00:00")}},
                {4,new DateTime?[] {DateTime.Parse("2022/06/04 08:00:00"),DateTime.Parse("2022/06/04 17:00:00")}},
                {5,new DateTime?[] {DateTime.Parse("2022/06/05 08:00:00"),DateTime.Parse("2022/06/05 17:00:00")}},
                {6,new DateTime?[] {DateTime.Parse("2022/06/06 08:00:00"),DateTime.Parse("2022/06/06 12:00:00")}},
                {7,new DateTime?[] {null,null}},
                {8,new DateTime?[] {null,null}},
                {9,new DateTime?[] {null,null}},
                {10,new DateTime?[] {null,null}},
                {11,new DateTime?[] {DateTime.Parse("2022/06/11 08:00:00"),DateTime.Parse("2022/06/11 17:00:00")}},
                {12,new DateTime?[] {null,null}},
                {13,new DateTime?[] {DateTime.Parse("2022/06/13 08:30:00"),DateTime.Parse("2022/06/13 17:00:00")}},
                {14,new DateTime?[] {DateTime.Parse("2022/06/14 08:00:00"),DateTime.Parse("2022/06/14 16:30:00")}},
                {15,new DateTime?[] {null,null}},
                {16,new DateTime?[] {DateTime.Parse("2022/06/16 13:00:00"),DateTime.Parse("2022/06/16 17:00:00")}},
                {17,new DateTime?[] {DateTime.Parse("2022/06/17 08:00:00"),DateTime.Parse("2022/06/17 12:00:00")}},
                {18,new DateTime?[] {null,null}},
                {19,new DateTime?[] {null,null}},
                {20,new DateTime?[] {DateTime.Parse("2022/06/20 22:00:00"),DateTime.Parse("2022/06/21 10:00:01")}},
                {21,new DateTime?[] {DateTime.Parse("2022/06/21 22:00:00"),DateTime.Parse("2022/06/22 07:00:01")}},
                {22,new DateTime?[] {DateTime.Parse("2022/06/22 22:00:00"),DateTime.Parse("2022/06/23 08:00:01")}},
                {23,new DateTime?[] {DateTime.Parse("2022/06/23 22:00:00"),DateTime.Parse("2022/06/24 09:00:01")}},
                {24,new DateTime?[] {DateTime.Parse("2022/06/24 22:00:00"),DateTime.Parse("2022/06/25 10:00:01")}},
                {25,new DateTime?[] {null,null}},
                {26,new DateTime?[] {null,null}},
                {27,new DateTime?[] {DateTime.Parse("2022/06/27 08:00:00"),DateTime.Parse("2022/06/27 19:00:00")}},
                {28,new DateTime?[] {DateTime.Parse("2022/06/28 08:00:00"),DateTime.Parse("2022/06/28 20:00:00")}},
                {29,new DateTime?[] {DateTime.Parse("2022/06/29 08:00:00"),DateTime.Parse("2022/06/29 17:00:00")}},
                {30,new DateTime?[] {DateTime.Parse("2022/06/30 08:00:00"),DateTime.Parse("2022/06/30 18:00:00")}},
            };
            for (int i = 0; i < 30; i++)
            {
                DateTime addDay = new(2022, 6, i + 1);

                var _aY = new AttendanceYear(addDay.Year);
                var _aM = new AttendanceMonth(addDay.Month);

                HolidayClassification holiday;
                switch (addDay.DayOfWeek)
                {
                    case DayOfWeek.Sunday:
                        holiday = HolidayClassification.LegalHoliday;
                        break;
                    case DayOfWeek.Saturday:
                        holiday = HolidayClassification.RegularHoliday;
                        break;
                    default:
                        holiday = HolidayClassification.None;
                        break;
                }
#pragma warning disable CS8602 // null 参照の可能性があるものの逆参照です。
#pragma warning disable CS8629 // Null 許容値型は Null になる場合があります。
                AttendanceRecord ar = new(
                    new AttendanceDay(_aY, _aM, addDay.Day),
                    holiday,
                    dayOffMap.GetValueOrDefault(i + 1),
                    attendance.GetValueOrDefault(i + 1)[0].HasValue ? new AttendanceTime(attendance.GetValueOrDefault(i + 1)[0].Value) : null,
                    attendance.GetValueOrDefault(i + 1)[1].HasValue ? new AttendanceTime(attendance.GetValueOrDefault(i + 1)[1].Value) : null,
                    attendance.GetValueOrDefault(i + 1)[0].HasValue ? new TimeSpan(1, 0, 0) : null,
                    holiday == HolidayClassification.None & attendance.GetValueOrDefault(i + 1)[0].HasValue ? OrderedLunchBox.Orderd : OrderedLunchBox.None
                    );
#pragma warning restore CS8629 // Null 許容値型は Null になる場合があります。
#pragma warning restore CS8602 // null 参照の可能性があるものの逆参照です。
                AttendanceRecords.Add(ar);
            }
        }
    }
}