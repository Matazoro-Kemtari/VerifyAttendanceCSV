using Reactive.Bindings;
using Wada.AOP.Logging;

namespace Wada.VerifyAttendanceCSV.Models;

internal record class MatchedEmployeeNumberMaintenancePageModel
{
    [Logging]
    internal void Clear() => CsvFileName.Value = string.Empty;

    public ReactivePropertySlim<string> CsvFileName { get; } = new();
}
