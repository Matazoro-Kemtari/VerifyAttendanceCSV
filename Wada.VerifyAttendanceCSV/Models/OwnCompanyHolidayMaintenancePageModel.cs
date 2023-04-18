using Reactive.Bindings;
using Wada.AOP.Logging;

namespace Wada.VerifyAttendanceCSV.Models;

internal record class OwnCompanyHolidayMaintenancePageModel
{
    [Logging]
    internal void Clear() => XlsxFileName.Value = string.Empty;

    public ReactivePropertySlim<string> XlsxFileName { get; } = new();
    public ReactivePropertySlim<CalendarGroup> CalendarGroupClass { get; } = new();
}

/// <summary>
/// 会社カレンダー グループ
/// </summary>
public enum CalendarGroup
{
    HeadOffice,
    MatsuzakaOffice,
}