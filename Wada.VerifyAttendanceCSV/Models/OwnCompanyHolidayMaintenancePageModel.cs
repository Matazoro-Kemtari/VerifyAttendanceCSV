using Reactive.Bindings;
using Wada.AOP.Logging;

namespace Wada.VerifyAttendanceCSV.Models
{
    internal class OwnCompanyHolidayMaintenancePageModel
    {
        [Logging]
        internal void Clear() => CsvFileName.Value = string.Empty;

        public ReactivePropertySlim<string> CsvFileName { get; } = new();
    }
}
