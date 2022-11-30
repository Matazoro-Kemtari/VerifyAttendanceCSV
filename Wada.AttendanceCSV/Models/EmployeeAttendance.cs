using CsvHelper.Configuration.Attributes;

namespace Wada.AttendanceCSV.Models
{
    internal record class EmployeeAttendance(
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
        decimal EducationDay,
        [Index(10)]
        decimal BusinessSuspensionDay,
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
        decimal AnomalyHour);
}
