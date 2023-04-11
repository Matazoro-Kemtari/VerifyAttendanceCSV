using GongSolutions.Wpf.DragDrop;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive.Disposables;
using System.Transactions;
using System.Windows.Input;
using System.Windows.Navigation;
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
            .ToAsyncReactiveCommand()
            .AddTo(Disposables);
    }

    [Logging]
    public void Destroy() => Disposables.Dispose();

    public void DragOver(IDropInfo dropInfo)
    {
        throw new NotImplementedException();
    }

    public void Drop(IDropInfo dropInfo)
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
