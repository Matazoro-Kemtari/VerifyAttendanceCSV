using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wada.AOP.Logging;

namespace Wada.VerifyAttendanceCSV.Models;

public record class ComparisonAttendanceTablePageModel
{
    [Logging]
    internal void Clear()
    {
        CSVPath.Value = string.Empty;
        XlsxPaths.Clear();
    }

    public ReactivePropertySlim<string> CSVPath { get; } = new();
    public ReactiveCollection<string> XlsxPaths { get; } = new();
    public ReactivePropertySlim<DateTime> TargetDate { get; } = new();
    public ReactivePropertySlim<DateTime> LastedHolidayDate { get; } = new();
}
