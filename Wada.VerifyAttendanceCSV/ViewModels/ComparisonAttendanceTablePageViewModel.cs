using DetermineDifferenceApplication;
using Microsoft.Extensions.Configuration;
using NLog;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RegisterOwnCompanyHolidayApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace Wada.VerifyAttendanceCSV.ViewModels
{
    public class ComparisonAttendanceTablePageViewModel : BindableBase, IDestructible
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IMessageNotification _message;
        private readonly IDetermineDifferenceUseCase _determineDifferenceUseCase;
        private readonly IFetchOwnCompanyHolidayMaxDateUseCase _fetchOwnCompanyHolidayMaxDateUseCase;

        public ComparisonAttendanceTablePageViewModel(ILogger logger, IConfiguration configuration, IMessageNotification message, IDetermineDifferenceUseCase determineDifferenceUseCase, IFetchOwnCompanyHolidayMaxDateUseCase fetchOwnCompanyHolidayMaxDateUseCase)
        {
            _logger = logger;
            _configuration = configuration;
            _message = message;
            _determineDifferenceUseCase = determineDifferenceUseCase;
            _fetchOwnCompanyHolidayMaxDateUseCase = fetchOwnCompanyHolidayMaxDateUseCase;


            CSVPath = new ReactiveProperty<string>(_configuration["applicationConfiguration:CSVPath"]);
            XLSXPath = new ReactiveProperty<string>(_configuration.GetValue("applicationConfiguration:XLSXPath", string.Empty));
            DateTime _date = DateTime.Now.AddMonths(-1);
            Year = new ReactiveProperty<uint>((uint)_date.Year);
            Month = new ReactiveProperty<uint>((uint)_date.Month);

            NextViewCommand = new AsyncReactiveCommand()
                .WithSubscribe(async () =>
                {
                    int csvCount, xlsxCount;
                    Dictionary<uint, List<string>> responce;
                    DetermineDifferenceUseCaseDTO differenceDTP;
                    try
                    {
                        differenceDTP = await _determineDifferenceUseCase.ExecuteAsync(CSVPath.Value, XLSXPath.Value.Split("\n"), (int)Year.Value, (int)Month.Value);
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
                    logger.Info(responceMsg);
                    _message.ShowInformationMessage(responceMsg, "勤怠表照合");
                })
                .AddTo(Disposables);

            _ = Task.Run(async () =>
            {
                var d = await _fetchOwnCompanyHolidayMaxDateUseCase.ExecuteAsyc();
                maxDate.Value = d;
            });
            HolidayMaxDate = maxDate.ToReactivePropertySlimAsSynchronized(x => x.Value)
                .AddTo(Disposables);
        }

        // TODO:正式ではMODEL作る
        private readonly ReactivePropertySlim<DateTime> maxDate = new();


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
        public ReactivePropertySlim<DateTime> HolidayMaxDate { get; }
    }
}
