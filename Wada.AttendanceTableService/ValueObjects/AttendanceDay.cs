namespace Wada.AttendanceTableService.ValueObjects
{
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

            Value = value;
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
}
