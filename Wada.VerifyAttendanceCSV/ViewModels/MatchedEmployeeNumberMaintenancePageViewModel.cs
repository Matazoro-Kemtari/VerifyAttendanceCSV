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
using Wada.RegisterEmployeeNumberTableApplication;
using Wada.VerifyAttendanceCSV.Models;

namespace Wada.VerifyAttendanceCSV.ViewModels;

public class MatchedEmployeeNumberMaintenancePageViewModel : BindableBase, IDestructible, IDropTarget
{
    private readonly MatchedEmployeeNumberMaintenancePageModel _model = new();
    private readonly IRegisterEmployeeNumberTableUseCase _registerEmployeeNumberTableUseCase;

    private MatchedEmployeeNumberMaintenancePageViewModel()
    {
        XlsxFilePath = _model.XlsxFileName
            .ToReactivePropertyAsSynchronized(x => x.Value)
            .SetValidateAttribute(() => XlsxFilePath)
            .AddTo(Disposables);

        EntryCommand = XlsxFilePath.ObserveHasErrors
            .Inverse()
            .ToAsyncReactiveCommand()
            .WithSubscribe(() => RegisterEmployeeNumberTableAsync())
            .AddTo(Disposables);
    }

    public MatchedEmployeeNumberMaintenancePageViewModel(IRegisterEmployeeNumberTableUseCase registerEmployeeNumberTableUseCase)
        : this()
    {
        _registerEmployeeNumberTableUseCase = registerEmployeeNumberTableUseCase;
    }

    [Logging]
    public void Destroy() => Disposables.Dispose();

    public void DragOver(IDropInfo dropInfo)
    {
        var dragFiles = ((DataObject)dropInfo.Data).GetFileDropList().Cast<string>();
        dropInfo.Effects = dragFiles.Any(x => Path.GetExtension(x).ToLower() == ".xlsx")
            ? DragDropEffects.Copy : DragDropEffects.None;
    }

    [Logging]
    public void Drop(IDropInfo dropInfo)
    {
        var dragFiles = ((DataObject)dropInfo.Data).GetFileDropList().Cast<string>();
        dropInfo.Effects = dragFiles.Any(x => Path.GetExtension(x).ToLower() == ".xlsx")
            ? DragDropEffects.Copy : DragDropEffects.None;

        _model.XlsxFileName.Value =
            dragFiles.FirstOrDefault(x => Path.GetExtension(x).ToLower() == ".xlsx") ?? string.Empty;
    }

    /// <summary>
    /// 社員番号対応表を登録する
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [Logging]
    private async Task RegisterEmployeeNumberTableAsync()
    {
        // 確認
        var confirmMessage = MessageNotificationViaLivet.MakeQuestionMessage(
            "社員番号対応表を追加します よろしいですか");
        await Messenger.RaiseAsync(confirmMessage);
        if (!(confirmMessage.Response ?? false))
            return;

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            await _registerEmployeeNumberTableUseCase.ExecuteAsync(XlsxFilePath.Value);
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
    }

    /// <summary>
    /// Disposeが必要なReactivePropertyやReactiveCommandを集約させるための仕掛け
    /// </summary>
    private CompositeDisposable Disposables { get; } = new CompositeDisposable();

    public InteractionMessenger Messenger { get; } = new InteractionMessenger();

    /// <summary>
    /// 社員番号対応XLSXファイルパス
    /// </summary>
    [Display(Name = "社員番号対応エクセルファイル")]
    [Required(ErrorMessage = "{0}をドラッグアンドドロップしてください")]
    public ReactiveProperty<string> XlsxFilePath { get; }

    public AsyncReactiveCommand EntryCommand { get; }
}
