using CsvHelper.Configuration.Attributes;
using Wada.AttendanceTableService.WorkingMonthlyReportAggregation;

namespace Wada.AttendanceCsv.Models;

internal record class EmployeeAttendanceCsv(
    [Index(0)]
    uint AttendancePersonalCode,
    [Index(1)]
    decimal AttendanceDay,
    [Index(2)]
    decimal HolidayWorkedDay,
    [Index(3)]
    decimal RegularWorkedHour,
    [Index(4)]
    int LatenessTime,
    [Index(5)]
    int EarlyLeaveTime,
    [Index(6)]
    decimal PaidLeaveDay,
    [Index(7)]
    decimal TransferedAttendanceDay,
    [Index(8)]
    int PaidSpecialLeaveDay,
    [Index(9)]
    int EducationDay,
    [Index(10)]
    int BusinessSuspensionDay,
    [Index(11)]
    int AbsenceDay,
    [Index(12)]
    decimal OvertimeHour,
    [Index(13)]
    decimal LegalHolidayWorkedHour,
    [Index(14)]
    decimal LateNightWorkingHour,
    [Index(15)]
    decimal RegularHolidayWorkedHour,
    [Index(16)]
    int LunchBoxOrderedTime,
    [Index(17)]
    decimal? AnomalyHour)
{
    internal EmployeeAttendance ToDomainEntity() => new(
        AttendancePersonalCode,
        AttendanceDay,
        HolidayWorkedDay,
        RegularWorkedHour,
        LatenessTime,
        EarlyLeaveTime,
        PaidLeaveDay,
        TransferedAttendanceDay,
        PaidSpecialLeaveDay,
        EducationDay,
        BusinessSuspensionDay,
        AbsenceDay,
        OvertimeHour,
        LegalHolidayWorkedHour,
        LateNightWorkingHour,
        RegularHolidayWorkedHour,
        AnomalyHour);
}
