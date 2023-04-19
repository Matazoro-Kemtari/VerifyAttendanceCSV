using Prism.Mvvm;
using Prism.Navigation;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Reactive.Disposables;
using Wada.VerifyAttendanceCSV.Views;

namespace Wada.VerifyAttendanceCSV.ViewModels;

public class NavigationPageViewModel : BindableBase, IDestructible
{
    public NavigationPageViewModel(IRegionManager regionManager)
    {
        ComparisonViewCommand = new ReactiveCommand()
            .WithSubscribe(() => regionManager.RequestNavigate("ContentRegion", nameof(ComparisonAttendanceTablePage)))
            .AddTo(Disposables);

        EmployeeViewCommand = new ReactiveCommand()
            .WithSubscribe(() => regionManager.RequestNavigate("ContentRegion", nameof(MatchedEmployeeNumberMaintenancePage)))
            .AddTo(Disposables);

        CalendarViewCommand= new ReactiveCommand()
            .WithSubscribe(() => regionManager.RequestNavigate("ContentRegion", nameof(OwnCompanyHolidayMaintenancePage)))
            .AddTo(Disposables);
    }

    public void Destroy() => Disposables.Dispose();

    /// <summary>
    /// Disposeが必要なReactivePropertyやReactiveCommandを集約させるための仕掛け
    /// </summary>
    private CompositeDisposable Disposables { get; } = new CompositeDisposable();

    public ReactiveCommand ComparisonViewCommand { get; }
    public ReactiveCommand EmployeeViewCommand { get; }
    public ReactiveCommand CalendarViewCommand { get; }
}
