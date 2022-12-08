using DetermineDifferenceApplication;
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
using System.Configuration;
using System.Linq;
using System.Reactive.Disposables;

namespace Wada.VerifyAttendanceCSV.ViewModels
{
    public class ComparisonAttendanceTablePageViewModel : BindableBase, IDestructible
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IMessageNotification _message;
        private readonly IDetermineDifferenceUseCase _determineDifferenceUseCase;

        public ComparisonAttendanceTablePageViewModel(ILogger logger, IConfiguration configuration, IMessageNotification message, IDetermineDifferenceUseCase determineDifferenceUseCase)
        {
            _logger = logger;
            _configuration = configuration;
            _message = message;
            _determineDifferenceUseCase = determineDifferenceUseCase;

            CSVPath = new ReactiveProperty<string>(_configuration["applicationConfiguration:CSVPath"]);
            XLSXPath = new ReactiveProperty<string>(_configuration.GetValue("applicationConfiguration:XLSXPath", string.Empty));
            DateTime _date = DateTime.Now;
            Year = new ReactiveProperty<uint>((uint)_date.Year);
            Month = new ReactiveProperty<uint>((uint)_date.Month);

            NextViewCommand = new AsyncReactiveCommand()
                .WithSubscribe(async () =>
                {
                    Dictionary<uint, List<string>> responce;
                    try
                    {
                        responce = await determineDifferenceUseCase.ExecuteAsync(CSVPath.Value, XLSXPath.Value.Split("\n"), (int)Year.Value, (int)Month.Value);
                    }
                    catch (EmployeeNumberNotFoundException ex)
                    {
                        string msg = $"{ex.Message}\n今は登録機能がないので 平野まで連絡ください";
                        _logger.Error(msg, ex);
                        _message.ShowExclamationMessage(ex.Message, "注意");
                        return;
                    }
                    _message.ShowInformationMessage(
                        $"{responce.Count}件違います\n" +
                        $"{responce.Select(x => $"社員番号: {x.Key}; 項目: {x.Value}\n")}",
                        "勤怠表照合");
                })
                .AddTo(Disposables);
        }

        public void Destroy() => Disposables.Dispose();

        /// <summary>
        /// Disposeが必要なReactivePropertyやReactiveCommandを集約させるための仕掛け
        /// </summary>
        private CompositeDisposable Disposables { get; } = new CompositeDisposable();

        public AsyncReactiveCommand NextViewCommand { get; }

        public DelegateCommand PreviousViewCommand { get; }

        public ReactiveProperty<string> CSVPath { get; }

        public ReactiveProperty<string> XLSXPath { get; }
        public ReactiveProperty<uint> Year { get; }
        public ReactiveProperty<uint> Month { get; }
    }
}
