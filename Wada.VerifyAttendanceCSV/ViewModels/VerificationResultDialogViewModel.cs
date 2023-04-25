using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using Wada.VerifyAttendanceCSV.Models;

namespace Wada.VerifyAttendanceCSV.ViewModels;

public class VerificationResultDialogViewModel : BindableBase, IDialogAware, IDestructible
{
    private readonly VerificationResultDialogModel _model = new();

    public VerificationResultDialogViewModel()
    {
        AttendanceCsvLength = _model.AttendanceCsvLength
            .AddTo(Disposables);

        AttendanceSpreadLength = _model.AttendanceSpreadLength
            .AddTo(Disposables);

        Difference = _model.Difference
            .AddTo(Disposables);

        DifferencialDetails = _model.DifferencialDetails
            .ToReadOnlyReactiveCollection(x => new DifferencialDetailViewModel(x))
            .AddTo(Disposables);

        ExecCommand = new ReactiveCommand()
            .WithSubscribe(() =>
                // ダイアログクローズイベントをキック
                RequestClose?.Invoke(new DialogResult(ButtonResult.OK)))
            .AddTo(Disposables);

    }

    public string Title => "比較結果";

    public event Action<IDialogResult> RequestClose;

    public bool CanCloseDialog() => true;

    public void OnDialogClosed() { }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        var result = parameters.GetValue<VerificationResultRequest>(nameof(VerificationResultRequest));
        _model.PopulateFrom(result);
    }

    /// <summary>オブジェクトを破棄します</summary>
    public void Destroy() => Disposables.Dispose();

    /// <summary>
    /// Disposeが必要なReactivePropertyやReactiveCommandを集約させるための仕掛け
    /// </summary>
    private CompositeDisposable Disposables { get; } = new CompositeDisposable();

    /// <summary>
    /// 勤怠CSVファイルのレコード数
    /// </summary>
    public ReactivePropertySlim<int> AttendanceCsvLength { get; }

    /// <summary>
    /// 勤務表のレコード数
    /// </summary>
    public ReactivePropertySlim<int> AttendanceSpreadLength { get; }

    /// <summary>
    /// 両レコード数の差
    /// </summary>
    public ReactivePropertySlim<int> Difference { get; }

    /// <summary>
    /// 差分詳細
    /// </summary>
    public ReadOnlyReactiveCollection<DifferencialDetailViewModel> DifferencialDetails { get; }

    public ReactiveCommand ExecCommand { get; }
}

public class DifferencialDetailViewModel : BindableBase, IDestructible
{
    private readonly DifferencialDetailModel _model;

    public DifferencialDetailViewModel(DifferencialDetailModel model)
    {
        _model = model;

        EmployeeNumber = _model.EmployeeNumber
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Disposables);
        EmployeeName = _model.EmployeeName
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Disposables);
        Differences = _model.Differences
            .ToReadOnlyReactiveCollection()
            .AddTo(Disposables);
    }

    /// <summary>オブジェクトを破棄します</summary>
    public void Destroy() => Disposables.Dispose();

    /// <summary>
    /// Disposeが必要なReactivePropertyやReactiveCommandを集約させるための仕掛け
    /// </summary>
    private CompositeDisposable Disposables { get; } = new CompositeDisposable();

    /// <summary>
    /// 社員番号
    /// </summary>
    public ReadOnlyReactivePropertySlim<uint> EmployeeNumber { get; }

    /// <summary>
    /// 社員名
    /// </summary>
    public ReadOnlyReactivePropertySlim<string> EmployeeName { get; }

    /// <summary>
    /// 相違点
    /// </summary>
    public ReadOnlyReactiveCollection<string> Differences { get; }
}

internal record class VerificationResultRequest(int AttendanceCsvLength, int AttendanceSpreadLength, int Difference, IEnumerable<DifferencialDetailRequest> DifferencialDetails);
internal record class DifferencialDetailRequest(uint EmployeeNumber, string EmployeeName, IEnumerable<string> Differences);