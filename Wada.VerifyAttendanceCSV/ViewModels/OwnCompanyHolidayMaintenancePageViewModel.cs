using GongSolutions.Wpf.DragDrop;
using Livet.Messaging;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Wada.AOP.Logging;
using Wada.Extensions;
using Wada.RegisterOwnCompanyHolidayApplication;
using Wada.VerifyAttendanceCSV.Models;

namespace Wada.VerifyAttendanceCSV.ViewModels;

public class OwnCompanyHolidayMaintenancePageViewModel : BindableBase, IDestructible, IDropTarget
{
    private readonly OwnCompanyHolidayMaintenancePageModel _model = new();
    private readonly IRegisterOwnCompanyHolidayUseCase _registerOwnCompanyHolidayUseCase;
    private readonly IFetchOwnCompanyHolidayMaxDateUseCase _fetchOwnCompanyHolidayMaxDateUseCase;

    private OwnCompanyHolidayMaintenancePageViewModel()
    {
        XlsxFilePath = _model.XlsxFileName
            .ToReactivePropertyAsSynchronized(x => x.Value)
            .SetValidateAttribute(() => XlsxFilePath)
            .AddTo(Disposables);

        CalendarGroupClass = _model.CalendarGroupClass
            .ToReactivePropertyAsSynchronized(x => x.Value)
            .SetValidateAttribute(() => CalendarGroupClass)
            .AddTo(Disposables);

        LastedHeadOfficeHoliday = _model.LastedHeadOfficeHoliday
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Disposables);

        LastedKuwanaOfficeHoliday = _model.LastedKuwanaOfficeHoliday
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Disposables);

        EntryCommand = new[]
        {
            XlsxFilePath.ObserveHasErrors,
            CalendarGroupClass.ObserveHasErrors,
        }
        .CombineLatestValuesAreAllFalse()
        .ToAsyncReactiveCommand()
        .WithSubscribe(() => RegisterOwnCompanyHolidayTableAsync())
        .AddTo(Disposables);
    }

    public OwnCompanyHolidayMaintenancePageViewModel(IRegisterOwnCompanyHolidayUseCase registerOwnCompanyHolidayUseCase, IFetchOwnCompanyHolidayMaxDateUseCase fetchOwnCompanyHolidayMaxDateUseCase)
        : this()
    {
        _registerOwnCompanyHolidayUseCase = registerOwnCompanyHolidayUseCase;
        _fetchOwnCompanyHolidayMaxDateUseCase = fetchOwnCompanyHolidayMaxDateUseCase;

        _fetchOwnCompanyHolidayMaxDateUseCase
        .ExecuteAsyc()
        // 正常終了した場合に継続する
        .ContinueWith(x =>
        {
            _model.LastedHeadOfficeHoliday.Value = x.Result.HeadOffice;
            _model.LastedKuwanaOfficeHoliday.Value = x.Result.KuwanaOffice;
        },
        TaskContinuationOptions.OnlyOnRanToCompletion);
    }

    [Logging]
    public void Destroy() => Disposables.Dispose();

    public void DragOver(IDropInfo dropInfo)
    {
        var dragFiles = ((DataObject)dropInfo.Data).GetFileDropList().Cast<string>();
        dropInfo.Effects = dragFiles.Any(x => Path.GetExtension(x).ToLower() == ".xlsx")
            ? DragDropEffects.Copy : DragDropEffects.None;
    }

    public void Drop(IDropInfo dropInfo)
    {
        var dragFiles = ((DataObject)dropInfo.Data).GetFileDropList().Cast<string>();
        dropInfo.Effects = dragFiles.Any(x => Path.GetExtension(x).ToLower() == ".xlsx")
            ? DragDropEffects.Copy : DragDropEffects.None;

        _model.XlsxFileName.Value =
            dragFiles.FirstOrDefault(x => Path.GetExtension(x).ToLower() == ".xlsx") ?? string.Empty;
    }

    /// <summary>
    /// 休日カレンダーを登録する
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [Logging]
    private async Task RegisterOwnCompanyHolidayTableAsync()
    {
        // 確認
        var confirmMessage = MessageNotificationViaLivet.MakeQuestionMessage(
            $"{CalendarGroupClass.Value.GetEnumDisplayName()}の" +
            $"会社カレンダーを追加します よろしいですか");
        await Messenger.RaiseAsync(confirmMessage);
        if (!(confirmMessage.Response ?? false))
            return;

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            await _registerOwnCompanyHolidayUseCase.ExecuteAsync(XlsxFilePath.Value, CalendarGroupClass.Value);
        }
        catch (UseCaseException ex)
        {
            // エラーメッセージ
            var message = MessageNotificationViaLivet.MakeErrorMessage(ex.Message);
            await Messenger.RaiseAsync(message);
            return;
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }

        // 終わり
        var finishMessage = MessageNotificationViaLivet.MakeInformationMessage("終了しました");
        await Messenger.RaiseAsync(finishMessage);
        _model.Clear();

        _ = _fetchOwnCompanyHolidayMaxDateUseCase.ExecuteAsyc()
            // 正常終了した場合に継続する
            .ContinueWith(x =>
            {
                _model.LastedHeadOfficeHoliday.Value = x.Result.HeadOffice;
                _model.LastedKuwanaOfficeHoliday.Value = x.Result.KuwanaOffice;
            },
            TaskContinuationOptions.OnlyOnRanToCompletion);
    }

    /// <summary>
    /// Disposeが必要なReactivePropertyやReactiveCommandを集約させるための仕掛け
    /// </summary>
    private CompositeDisposable Disposables { get; } = new CompositeDisposable();

    public InteractionMessenger Messenger { get; } = new InteractionMessenger();

    /// <summary>
    /// 休日カレンダーXLSXファイルパス
    /// </summary>
    [Display(Name = "休日カレンダーエクセルファイル")]
    [Required(ErrorMessage = "{0}をドラッグアンドドロップしてください")]
    public ReactiveProperty<string> XlsxFilePath { get; }

    /// <summary>
    /// カレンダーグループ
    /// </summary>
    [Display(Name = "カレンダーグループ")]
    [Required(ErrorMessage = "{0}を選択してください")]
    public ReactiveProperty<CalendarGroupAttempt> CalendarGroupClass { get; }

    /// <summary>
    /// 休日カレンダー最終日 本社
    /// </summary>
    public ReadOnlyReactivePropertySlim<DateTime> LastedHeadOfficeHoliday { get; }

    /// <summary>
    /// 休日カレンダー最終日 桑名
    /// </summary>
    public ReadOnlyReactivePropertySlim<DateTime> LastedKuwanaOfficeHoliday { get; }

    public AsyncReactiveCommand EntryCommand { get; }
}
