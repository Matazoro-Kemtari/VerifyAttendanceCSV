using GongSolutions.Wpf.DragDrop;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reactive.Disposables;
using Wada.AOP.Logging;
using Wada.VerifyAttendanceCSV.Models;

namespace Wada.VerifyAttendanceCSV.ViewModels
{
    public class OwnCompanyHolidayMaintenancePageViewModel : BindableBase, IDestructible, IDropTarget
    {
        private readonly OwnCompanyHolidayMaintenancePageModel _model = new();

        public OwnCompanyHolidayMaintenancePageViewModel()
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
        /// 休日カレンダーCSVファイルパス
        /// </summary>
        [Display(Name = "休日カレンダーCSVファイル")]
        [Required(ErrorMessage = "{0}をドラッグアンドドロップしてください")]
        public ReactiveProperty<string> CsvFileName { get; }

        public AsyncReactiveCommand EntryCommand { get; }
    }
}
