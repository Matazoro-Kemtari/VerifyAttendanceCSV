namespace Wada.AttendanceTableService.MatchedEmployeeNumberAggregation;

/// <summary>
/// 社員番号対応表エンティティ
/// </summary>
/// <param name="EmployeeNumbers"></param>
/// <param name="AttendancePersonalCode"></param>
public record class MatchedEmployeeNumber(uint EmployeeNumbers, uint AttendancePersonalCode);

public class TestMatchedEmployeeNumberFactory
{
    public static MatchedEmployeeNumber Create(uint employeeNumber = 1010, uint attendancePersonalCode = 10)
        => new(employeeNumber, attendancePersonalCode);
}