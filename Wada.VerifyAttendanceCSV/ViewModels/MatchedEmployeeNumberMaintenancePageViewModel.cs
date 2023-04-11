using GongSolutions.Wpf.DragDrop;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows;
using Wada.AOP.Logging;
using Wada.VerifyAttendanceCSV.Models;

namespace Wada.VerifyAttendanceCSV.ViewModels;

public class MatchedEmployeeNumberMaintenancePageViewModel : BindableBase, IDestructible, IDropTarget
{
    private readonly MatchedEmployeeNumberMaintenancePageModel _model = new();

    public MatchedEmployeeNumberMaintenancePageViewModel()
    {
        CsvFileName = _model.CsvFileName
            .ToReactivePropertyAsSynchronized(x => x.Value)
            .SetValidateAttribute(() => CsvFileName)
            .AddTo(Disposables);

        EntryCommand = CsvFileName.ObserveHasErrors
            .Inverse()
            .ToAsyncReactiveCommand()
            .WithSubscribe(() => RegisterEmployeeNumberTableAsync())
            .AddTo(Disposables);
    }

    [Logging]
    public void Destroy() => Disposables.Dispose();

    public void DragOver(IDropInfo dropInfo)
    {
        var dragFiles = ((DataObject)dropInfo.Data).GetFileDropList().Cast<string>();
        dropInfo.Effects = dragFiles.Any(x => Path.GetExtension(x) == ".csv")
            ? DragDropEffects.Copy : DragDropEffects.None;
    }

    [Logging]
    public void Drop(IDropInfo dropInfo)
    {
        var dragFiles = ((DataObject)dropInfo.Data).GetFileDropList().Cast<string>();
        dropInfo.Effects = dragFiles.Any(x => Path.GetExtension(x) == ".csv")
            ? DragDropEffects.Copy : DragDropEffects.None;

        _model.CsvFileName.Value =
            dragFiles.FirstOrDefault(x => Path.GetExtension(x) == ".csv") ?? string.Empty;
    }

    /// <summary>
    /// 社員番号対応表を登録する
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [Logging]
    private Task RegisterEmployeeNumberTableAsync()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Disposeが必要なReactivePropertyやReactiveCommandを集約させるための仕掛け
    /// </summary>
    private CompositeDisposable Disposables { get; } = new CompositeDisposable();

    /// <summary>
    /// 社員番号対応CSVファイルパス
    /// </summary>
    [Display(Name = "社員番号対応CSVファイル")]
    [Required(ErrorMessage = "{0}をドラッグアンドドロップしてください")]
    public ReactiveProperty<string> CsvFileName { get; }

    public AsyncReactiveCommand EntryCommand { get; }
}
