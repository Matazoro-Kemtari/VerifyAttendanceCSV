using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Wada.VerifyAttendanceCSV.Views;

namespace Wada.VerifyAttendanceCSV;

public class VerifyAttendanceCSVModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
        var regionManager = containerProvider.Resolve<IRegionManager>();

        regionManager.RegisterViewWithRegion("NavigationRegion", typeof(NavigationPage));
        regionManager.RegisterViewWithRegion("ContentRegion", typeof(OwnCompanyHolidayMaintenancePage));
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<ComparisonAttendanceTablePage>();
        containerRegistry.RegisterForNavigation<MatchedEmployeeNumberMaintenancePage>();
        containerRegistry.RegisterDialog<VerificationResultDialog>();
    }
}