using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Wada.VerifyAttendanceCSV.ViewModels;
using Wada.VerifyAttendanceCSV.Views;

namespace Wada.VerifyAttendanceCSV
{
    public class VerifyAttendanceCSVModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();

            regionManager?.RegisterViewWithRegion("ContentRegion", typeof(ComparisonAttendanceTablePage));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            _ = containerRegistry.Register<IMessageNotification, MessageNotificationViaMessagebox>();
        }
    }
}