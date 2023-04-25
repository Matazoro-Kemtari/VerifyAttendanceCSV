using Reactive.Bindings;
using Wada.AOP.Logging;

namespace Wada.VerifyAttendanceCSV.Models;

internal record class MatchedEmployeeNumberMaintenancePageModel
{
    [Logging]
    internal void Clear() => XlsxFileName.Value = string.Empty;

    public ReactivePropertySlim<string> XlsxFileName { get; } = new();
}
