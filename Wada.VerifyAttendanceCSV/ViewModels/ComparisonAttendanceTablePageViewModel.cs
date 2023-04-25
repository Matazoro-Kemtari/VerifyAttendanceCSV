using GongSolutions.Wpf.DragDrop;
using Livet.Messaging;
using Microsoft.Extensions.Configuration;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.TinyLinq;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Wada.DetermineDifferenceApplication;
using Wada.RegisterOwnCompanyHolidayApplication;
using Wada.VerifyAttendanceCSV.Models;
using Wada.VerifyAttendanceCSV.Views;

namespace Wada.VerifyAttendanceCSV.ViewModels;

public class ComparisonAttendanceTablePageViewModel : BindableBase, IDestructible
{
    private readonly ComparisonAttendanceTablePageModel _model = new();
    private readonly IDialogService _dialogService;
    private readonly IConfiguration _configuration;
    private readonly IDetermineDifferenceUseCase _determineDifferenceUseCase;
    private readonly IFetchOwnCompanyHolidayMaxDateUseCase _fetchOwnCompanyHolidayMaxDateUseCase;

    private ComparisonAttendanceTablePageViewModel()
    {
        AttendanceCsvDropHandler = new(_model);
        XlsxDirectoryDropHandler = new(_model);

        // 勤怠CSVファイル
        CSVPath = _model.CSVPath
            .ToReactivePropertyAsSynchronized(x => x.Value)
            .SetValidateAttribute(() => CSVPath)
            .AddTo(Disposables);

        // 勤務表ディレクトリ
        XlsxPaths = _model.XlsxPaths
            .ToReadOnlyReactiveCollection(x => x)
            .AddTo(Disposables);

        XlsxListSelectedIndex = new ReactivePropertySlim<int>()
            .AddTo(Disposables);

        // 処理対象日
        TargetDates = new ReactiveCollection<DateTime>()
        .AddTo(Disposables);
        DateTime _date = DateTime.Now.Date;
        TargetDates.AddRangeOnScheduler(new[]
        {
            _date.AddMonths(-2),
            _date.AddMonths(-1),
            _date,
        });

        TargetDate = _model.TargetDate
            .ToReactivePropertyAsSynchronized(x => x.Value)
            .AddTo(Disposables);

        NextViewCommand = new[]
        {
            CSVPath.ObserveHasErrors,
            XlsxPaths.ObserveProperty(x => x.Count)
                     .Select(c=>c<=0),
        }
        .CombineLatestValuesAreAllFalse()
        .ToAsyncReactiveCommand()
        .WithSubscribe(() => VerifyAttendance())
        .AddTo(Disposables);

        RemoveDirectoryItemCommand = new AsyncReactiveCommand()
            .WithSubscribe(() => RemoveDirectoryItem())
            .AddTo(Disposables);
    }

    public ComparisonAttendanceTablePageViewModel(
        IDialogService dialogService,
        IConfiguration configuration,
        IDetermineDifferenceUseCase determineDifferenceUseCase,
        IFetchOwnCompanyHolidayMaxDateUseCase fetchOwnCompanyHolidayMaxDateUseCase)
        : this()
    {
        _dialogService = dialogService;
        _configuration = configuration;
        _determineDifferenceUseCase = determineDifferenceUseCase;
        _fetchOwnCompanyHolidayMaxDateUseCase = fetchOwnCompanyHolidayMaxDateUseCase;

        _model.CSVPath.Value = _configuration["applicationConfiguration:CSVPath"];
        _model.XlsxPaths.AddRangeOnScheduler(
            _configuration.GetSection("applicationConfiguration:XLSXPaths").Get<string[]>());

        _fetchOwnCompanyHolidayMaxDateUseCase.ExecuteAsyc()
                                             // 正常終了した場合に継続する
                                             .ContinueWith(x => _model.LastedHoliday.Value = x.Result.Min(),
                                                           TaskContinuationOptions.OnlyOnRanToCompletion);

        LastedHoliday = _model.LastedHoliday
            .ToReactivePropertySlimAsSynchronized(x => x.Value)
            .AddTo(Disposables);
    }

    private async Task VerifyAttendance()
    {
        DetermineDifferenceUseCaseDTO differenceDTP;
        try
        {
            Mouse.OverrideCursor = Cursors.Wait;

            differenceDTP = await _determineDifferenceUseCase.ExecuteAsync(

                CSVPath.Value,
                XlsxPaths,
                TargetDate.Value);
        }
        catch (EmployeeNumberNotFoundException ex)
        {
            var errorMessage = MessageNotificationViaLivet.MakeExclamationMessage(
                $"{ex.Message}\n今は登録機能がないので 平野まで連絡ください");
            await Messenger.RaiseAsync(errorMessage);
            return;
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }

        // データクラスの詰め替え
        var request = new VerificationResultRequest(
                differenceDTP.CSVCount,
                differenceDTP.XLSXCount,
                differenceDTP.DetermineDifferenceEmployeesDTOs.Count(),
                differenceDTP.DetermineDifferenceEmployeesDTOs.Select(
                    x => new DifferencialDetailRequest(
                        x.EmployeeNumber,
                        x.EmployeeName,
                        x.Differences)));

        // ダイアログのパラメータにセット
        var parameters = new DialogParameters
            {
                { nameof(VerificationResultRequest), request }
            };

        // ダイアログ表示
        IDialogResult dialogResult;
        _dialogService.ShowDialog(nameof(VerificationResultDialog), parameters, res => dialogResult = res);
    }

    /// <summary>
    /// 勤務表ディレクトリの削除
    /// </summary>
    /// <returns></returns>
    private Task RemoveDirectoryItem() => Task.Run(() =>
    {
        if (_model.XlsxPaths.Any())
            _model.XlsxPaths.RemoveAtOnScheduler(XlsxListSelectedIndex.Value);
    });

    public void Destroy() => Disposables.Dispose();

    /// <summary>
    /// Disposeが必要なReactivePropertyやReactiveCommandを集約させるための仕掛け
    /// </summary>
    private CompositeDisposable Disposables { get; } = new CompositeDisposable();

    public InteractionMessenger Messenger { get; } = new InteractionMessenger();

    public AsyncReactiveCommand NextViewCommand { get; }

    public AsyncReactiveCommand RemoveDirectoryItemCommand { get; }

    /// <summary>
    /// 勤怠CSVファイルパス
    /// </summary>
    [Display(Name = "勤怠CSVファイル")]
    [Required(ErrorMessage = "{0}をドラッグアンドドロップしてください")]
    public ReactiveProperty<string> CSVPath { get; }

    /// <summary>
    /// 勤怠CSV DropHandler
    /// </summary>
    public CsvDropHandler AttendanceCsvDropHandler { get; }

    /// <summary>
    /// 勤務表エクセル保存フォルダ
    /// </summary>
    public ReadOnlyReactiveCollection<string> XlsxPaths { get; }

    /// <summary>
    /// 選択Index
    /// </summary>
    public ReactivePropertySlim<int> XlsxListSelectedIndex { get; }

    /// <summary>
    /// 勤務表エクセルフォルダ DropHandler
    /// </summary>
    public XlsxDirectoryDropHandler XlsxDirectoryDropHandler { get; }

    /// <summary>
    /// 処理対象日リスト
    /// </summary>
    public ReactiveCollection<DateTime> TargetDates { get; }

    /// <summary>
    /// 処理対象日
    /// </summary>
    public ReactiveProperty<DateTime> TargetDate { get; }

    /// <summary>
    /// 休日カレンダー最終日
    /// </summary>
    public ReactivePropertySlim<DateTime> LastedHoliday { get; }
}

/// <summary>
/// 勤怠CSVファイルのDrop Handler
/// </summary>
public class CsvDropHandler : IDropTarget
{
    private readonly ComparisonAttendanceTablePageModel _model;

    public CsvDropHandler(ComparisonAttendanceTablePageModel model)
    {
        _model = model;
    }

    public void DragOver(IDropInfo dropInfo)
    {
        var dragFiles = ((DataObject)dropInfo.Data).GetFileDropList().Cast<string>();
        dropInfo.Effects = dragFiles.Any(x => Path.GetExtension(x).ToLower() == ".csv")
            ? DragDropEffects.Copy
            : DragDropEffects.None;
    }

    public void Drop(IDropInfo dropInfo)
    {
        var dragFiles = ((DataObject)dropInfo.Data).GetFileDropList().Cast<string>();
        dropInfo.Effects = dragFiles.Any(x => Path.GetExtension(x).ToLower() == ".csv")
            ? DragDropEffects.Copy
            : DragDropEffects.None;

        _model.CSVPath.Value =
            dragFiles.FirstOrDefault(x => Path.GetExtension(x).ToLower() == ".csv") ?? string.Empty;
    }
}

/// <summary>
/// 勤務表ディレクトリのDrop Handler
/// </summary>
public class XlsxDirectoryDropHandler : IDropTarget
{
    private readonly ComparisonAttendanceTablePageModel _model;

    public XlsxDirectoryDropHandler(ComparisonAttendanceTablePageModel model)
    {
        _model = model;
    }

    public void DragOver(IDropInfo dropInfo)
    {
        var dragPaths = ((DataObject)dropInfo.Data).GetFileDropList().Cast<string>();
        dropInfo.Effects = dragPaths.Any(x => File.GetAttributes(x).HasFlag(FileAttributes.Directory))
            ? DragDropEffects.Move
            : DragDropEffects.None;
    }

    public void Drop(IDropInfo dropInfo)
    {
        var dragPaths = ((DataObject)dropInfo.Data).GetFileDropList().Cast<string>();
        dropInfo.Effects = dragPaths.Any(x => File.GetAttributes(x).HasFlag(FileAttributes.Directory))
            ? DragDropEffects.Move
            : DragDropEffects.None;

        dragPaths.Where(x => File.GetAttributes(x).HasFlag(FileAttributes.Directory))
                 .ToList()
                 .ForEach(x => _model.XlsxPaths.AddOnScheduler(x));
    }
}
