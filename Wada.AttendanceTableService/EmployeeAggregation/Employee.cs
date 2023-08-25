namespace Wada.AttendanceTableService.EmployeeAggregation;

public record class Employee
{
    private Employee(uint employeeNumber, string name, uint? departmentID, uint? achievementClassificationId)
    {
        EmployeeNumber = employeeNumber;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        DepartmentId = departmentID;
        AchievementClassificationId = achievementClassificationId;
    }

    // ファクトリメソッド
    // 受注管理にデータを追加する必要性が出たら追加する

    /// <summary>
    /// インフラ層専用再構築メソッド
    /// </summary>
    /// <param name="id"></param>
    /// <param name="achievementProcessName"></param>
    /// <returns></returns>
    public static Employee Reconstruct(uint employeeNumber, string name, uint? departmentID, uint? achievementClassificationId)
        => new(employeeNumber, name, departmentID, achievementClassificationId);

    /// <summary>
    /// 社員番号
    /// </summary>
    public uint EmployeeNumber { get; }

    /// <summary>
    /// 氏名
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 部署ID
    /// </summary>
    public uint? DepartmentId { get; }

    /// <summary>
    /// 実績工程ID
    /// </summary>
    public uint? AchievementClassificationId { get; }
}

public class TestEmployeeFactory
{
    public static Employee Create(
        uint employeeNumber = 4001u,
        string name = "本社　無人",
        uint? departmentID = 4,
        uint? achievementClassificationId = 3u)
        => Employee.Reconstruct(employeeNumber, name, departmentID, achievementClassificationId);
}
