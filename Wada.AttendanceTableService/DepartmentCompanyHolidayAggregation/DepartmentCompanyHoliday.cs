namespace Wada.AttendanceTableService.DepartmentCompanyHolidayAggregation;

public record class DepartmentCompanyHoliday
{
    private DepartmentCompanyHoliday(uint departmentId, string calendarGroupId)
    {
        DepartmentId = departmentId;
        CalendarGroupId = calendarGroupId;
    }

    /// <summary>
    /// ファクトリメソッド
    /// </summary>
    /// <param name="departmentId"></param>
    /// <param name="calendarGroupId"></param>
    /// <returns></returns>
    public static DepartmentCompanyHoliday Create(uint departmentId, string calendarGroupId)
        => new(departmentId, calendarGroupId);

    /// <summary>
    /// インフラ層専用再構築メソッド
    /// </summary>
    /// <param name="departmentId"></param>
    /// <param name="calendarGroupId"></param>
    /// <returns></returns>
    public static DepartmentCompanyHoliday Reconstruct(uint departmentId, string calendarGroupId)
        => Create(departmentId, calendarGroupId);

    /// <summary>
    /// 部署ID
    /// </summary>
    public uint DepartmentId { get; }

    /// <summary>
    /// カレンダーグループID
    /// </summary>
    public string CalendarGroupId { get; }
}

public class TestDepartmentCompanyHolidayFactory
{
    public static DepartmentCompanyHoliday Create(uint departmentId = 4, string calendarGroupId = "__Dummy__")
    {
        return DepartmentCompanyHoliday.Reconstruct(departmentId, calendarGroupId);
    }
}
