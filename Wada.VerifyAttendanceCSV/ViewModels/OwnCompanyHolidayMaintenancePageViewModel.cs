using GongSolutions.Wpf.DragDrop;
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
using Wada.AOP.Logging;
using Wada.RegisterOwnCompanyHolidayApplication;
using Wada.VerifyAttendanceCSV.Models;

namespace Wada.VerifyAttendanceCSV.ViewModels
{
    public class OwnCompanyHolidayMaintenancePageViewModel : BindableBase, IDestructible, IDropTarget
    {
        private readonly OwnCompanyHolidayMaintenancePageModel _model = new();

        public OwnCompanyHolidayMaintenancePageViewModel()
        {
            XlsxFilePath = _model.XlsxFileName
                .ToReactivePropertyAsSynchronized(x => x.Value)
                .SetValidateAttribute(() => XlsxFilePath)
                .AddTo(Disposables);

            CalendarGroupClass = _model.CalendarGroupClass
                .ToReactivePropertyAsSynchronized(x => x.Value)
                .SetValidateAttribute(() => CalendarGroupClass)
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
        private Task RegisterOwnCompanyHolidayTableAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Disposeが必要なReactivePropertyやReactiveCommandを集約させるための仕掛け
        /// </summary>
        private CompositeDisposable Disposables { get; } = new CompositeDisposable();

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

        public AsyncReactiveCommand EntryCommand { get; }
    }
}
