namespace Wada.AttendanceTableService.WorkingMonthlyReportAggregation
{
    /// <summary>
    /// WorkedMonthlyReportFactoryメソッド引数用データクラス
    /// </summary>
    /// <param name="AttendancePersonalCode"></param>
    /// <param name="AttendanceDay"></param>
    /// <param name="HolidayWorkedDay"></param>
    /// <param name="RegularWorkedHour"></param>
    /// <param name="LatenessTime"></param>
    /// <param name="EarlyLeaveTime"></param>
    /// <param name="PaidLeaveDay"></param>
    /// <param name="TransferedAttendanceDay"></param>
    /// <param name="PaidSpecialLeaveDay"></param>
    /// <param name="EducationDay"></param>
    /// <param name="BusinessSuspensionDay"></param>
    /// <param name="AbsenceDay"></param>
    /// <param name="OvertimeHour"></param>
    /// <param name="LegalHolidayWorkedHour"></param>
    /// <param name="LateNightWorkingHour"></param>
    /// <param name="RegularHolidayWorkedHour"></param>
    /// <param name="AnomalyHour"></param>
    public record class EmployeeAttendance(
        uint AttendancePersonalCode,
        decimal AttendanceDay,
        decimal HolidayWorkedDay,
        decimal RegularWorkedHour,
        int LatenessTime,
        int EarlyLeaveTime,
        decimal PaidLeaveDay,
        decimal TransferedAttendanceDay,
        int PaidSpecialLeaveDay,
        int EducationDay,
        int BusinessSuspensionDay,
        int AbsenceDay,
        decimal OvertimeHour,
        decimal LegalHolidayWorkedHour,
        decimal LateNightWorkingHour,
        decimal RegularHolidayWorkedHour,
        decimal? AnomalyHour);
}
