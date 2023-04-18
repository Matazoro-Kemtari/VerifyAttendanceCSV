using Reactive.Bindings;
using Wada.AOP.Logging;
using Wada.RegisterOwnCompanyHolidayApplication;

namespace Wada.VerifyAttendanceCSV.Models;

internal record class OwnCompanyHolidayMaintenancePageModel
{
    [Logging]
    internal void Clear() => XlsxFileName.Value = string.Empty;

    public ReactivePropertySlim<string> XlsxFileName { get; } = new();
    public ReactivePropertySlim<CalendarGroupAttempt> CalendarGroupClass { get; } = new();
}
