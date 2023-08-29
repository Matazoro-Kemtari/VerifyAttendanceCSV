namespace Wada.AttendanceTableService.MatchedEmployeeNumberAggregation;

public record class MatchedEmployeeNumber
{
    private MatchedEmployeeNumber(uint employeeNumber, uint attendancePersonalCode)
    {
        EmployeeNumber = employeeNumber;
        AttendancePersonalCode = attendancePersonalCode;
    }

    /// <summary>
    /// ファクトリメソッド
    /// </summary>
    /// <param name="employeeNumber"></param>
    /// <param name="attendancePersonalCode"></param>
    /// <returns></returns>
    public static MatchedEmployeeNumber Create(uint employeeNumber, uint attendancePersonalCode)
        => new(employeeNumber, attendancePersonalCode);

    /// <summary>
    /// インフラ層専用再構築メソッド
    /// </summary>
    /// <param name="employeeNumber"></param>
    /// <param name="attendancePersonalCode"></param>
    /// <returns></returns>
    public static MatchedEmployeeNumber Reconstruct(uint employeeNumber, uint attendancePersonalCode)
        => Create(employeeNumber, attendancePersonalCode);

    /// <summary>
    /// 社員番号
    /// </summary>
    public uint EmployeeNumber { get; }

    /// <summary>
    /// 勤怠個人コード
    /// </summary>
    public uint AttendancePersonalCode { get; }
}

public class TestMatchedEmployeeNumberFactory
{
    public static MatchedEmployeeNumber Create(uint employeeNumber = 1010, uint attendancePersonalCode = 10)
        => MatchedEmployeeNumber.Reconstruct(employeeNumber, attendancePersonalCode);
}
