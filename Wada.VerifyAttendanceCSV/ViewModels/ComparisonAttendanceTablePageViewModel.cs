using GongSolutions.Wpf.DragDrop;
using Microsoft.Extensions.Configuration;
using NLog;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using Wada.DetermineDifferenceApplication;
using Wada.RegisterOwnCompanyHolidayApplication;
using Wada.VerifyAttendanceCSV.Models;
using Wada.VerifyAttendanceCSV.Views;

namespace Wada.VerifyAttendanceCSV.ViewModels;

public class ComparisonAttendanceTablePageViewModel : BindableBase, IDestructible
{
    private readonly ComparisonAttendanceTablePageModel _model = new();
    private readonly IRegionManager _regionManager;
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IMessageNotification _message;
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

        NextViewCommand = new AsyncReactiveCommand()
            .WithSubscribe(() => VerifyAttendance())
            .AddTo(Disposables);

        RemoveDirectoryItemCommand = new AsyncReactiveCommand()
            .WithSubscribe(() => RemoveDirectoryItem())
            .AddTo(Disposables);
    }

    public ComparisonAttendanceTablePageViewModel(IRegionManager regionManager, ILogger logger, IConfiguration configuration, IMessageNotification message, IDetermineDifferenceUseCase determineDifferenceUseCase, IFetchOwnCompanyHolidayMaxDateUseCase fetchOwnCompanyHolidayMaxDateUseCase)
        : this()
    {
        _regionManager = regionManager;
        _logger = logger;
        _configuration = configuration;
        _message = message;
        _determineDifferenceUseCase = determineDifferenceUseCase;
        _fetchOwnCompanyHolidayMaxDateUseCase = fetchOwnCompanyHolidayMaxDateUseCase;

        _model.CSVPath.Value = _configuration["applicationConfiguration:CSVPath"];
        _model.XlsxPaths.AddRangeOnScheduler(
            _configuration.GetSection("applicationConfiguration:XLSXPaths").Get<string[]>());

        _ = Task.Run(async () =>
        {
            var d = await _fetchOwnCompanyHolidayMaxDateUseCase.ExecuteAsyc();
            maxDate.Value = d;
        });
        LastedHolidayDate = maxDate.ToReactivePropertySlimAsSynchronized(x => x.Value)
            .AddTo(Disposables);
    }

    private async Task VerifyAttendance()
    {
        DetermineDifferenceUseCaseDTO differenceDTP;
        try
        {
            differenceDTP = await _determineDifferenceUseCase.ExecuteAsync(
                CSVPath.Value,
                XlsxPaths,
                TargetDate.Value);
        }
        catch (EmployeeNumberNotFoundException ex)
        {
            string msg = $"{ex.Message}\n今は登録機能がないので 平野まで連絡ください";
            _logger.Error(msg, ex);
            _message.ShowExclamationMessage(msg, "注意");
            return;
        }
        string responceMsg =
            $"CSVファイル: {differenceDTP.CSVCount}件\n" +
            $"勤務表: {differenceDTP.XLSXCount}件\n" +
            $"{differenceDTP.DetermineDifferenceEmployeesDTOs.Count()}件違います\n" +
            $"{string.Join("\n--\n", differenceDTP.DetermineDifferenceEmployeesDTOs.Select(x => $"社員番号: {x.AttendancePersonalCode}, 氏名: {x.EmployeeName}, 項目: {String.Join("/", x.Differences)}"))}";
        _logger.Info(responceMsg);
        _message.ShowInformationMessage(responceMsg, "勤怠表照合");
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

    // TODO:正式ではMODEL作る
    private readonly ReactivePropertySlim<DateTime> maxDate = new();

    public void Destroy() => Disposables.Dispose();

    /// <summary>
    /// Disposeが必要なReactivePropertyやReactiveCommandを集約させるための仕掛け
    /// </summary>
    private CompositeDisposable Disposables { get; } = new CompositeDisposable();

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
    public ReactivePropertySlim<DateTime> LastedHolidayDate { get; }
}

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
