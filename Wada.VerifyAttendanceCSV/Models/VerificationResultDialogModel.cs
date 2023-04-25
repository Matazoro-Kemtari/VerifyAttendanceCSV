using Reactive.Bindings;
using System.Collections.ObjectModel;
using System.Linq;
using Wada.VerifyAttendanceCSV.ViewModels;

namespace Wada.VerifyAttendanceCSV.Models;

internal record class VerificationResultDialogModel
{
    internal void PopulateFrom(VerificationResultRequest request)
    {
        AttendanceCsvLength.Value = request.AttendanceCsvLength;
        AttendanceSpreadLength.Value = request.AttendanceSpreadLength;
        Difference.Value = request.Difference;
        DifferencialDetails.Clear();
        DifferencialDetails.AddRange(request.DifferencialDetails.Select(x =>
        {
            var _detail = new DifferencialDetailModel();
            _detail.PopulateFrom(x);
            return _detail;
        }));
    }

    public ReactivePropertySlim<int> AttendanceCsvLength { get; init; } = new();

    public ReactivePropertySlim<int> AttendanceSpreadLength { get; init; } = new();

    public ReactivePropertySlim<int> Difference { get; init; } = new();

    public ReactiveCollection<DifferencialDetailModel> DifferencialDetails { get; init; } = new();
}

public record class DifferencialDetailModel
{
    internal void PopulateFrom(DifferencialDetailRequest request)
    {
        EmployeeNumber.Value = request.EmployeeNumber;
        EmployeeName.Value = request.EmployeeName;
        Differences.Clear();
        Differences.AddRange(request.Differences);
    }
    public ReactivePropertySlim<uint> EmployeeNumber { get; init; } = new();
    public ReactivePropertySlim<string> EmployeeName { get; init; } = new();
    public ReactiveCollection<string> Differences { get; init; } = new();
}