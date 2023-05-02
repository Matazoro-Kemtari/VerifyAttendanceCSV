using Microsoft.Extensions.Configuration;
using NLog;
using Prism.Ioc;
using Prism.Modularity;
using System.IO;
using System.Windows;
using VerifyAttendanceCSV.Views;
using Wada.AttendanceCsv;
using Wada.AttendanceSpreadSheet;
using Wada.AttendanceTableService;
using Wada.Data.DesignDepartmentDataBase;
using Wada.Data.DesignDepartmentDataBase.Models;
using Wada.Data.OrderManagement;
using Wada.Data.OrderManagement.Models;
using Wada.DetermineDifferenceApplication;
using Wada.IO;
using Wada.MatchedEmployeeNumberSpreadSheet;
using Wada.OwnCompanyHolidaySpreadSheet;
using Wada.RegisterEmployeeNumberTableApplication;
using Wada.RegisterOwnCompanyHolidayApplication;
using Wada.StoreApplicationConfiguration;
using Wada.StoreSelectedXlsxDirectoriesApplication;
using Wada.VerifyAttendanceCSV;

namespace VerifyAttendanceCSV
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 環境変数を読み込む
            DotNetEnv.Env.Load(".env");

            // DI 設定
            _ = containerRegistry.Register<IConfiguration>(_ => MyConfigurationBuilder());
            // DI logger
            _ = containerRegistry.RegisterSingleton<ILogger>(_ => LogManager.GetCurrentClassLogger());

            // DBライブラリ
            _ = containerRegistry.Register<IMatchedEmployeeNumberRepository, MatchedEmployeeNumberRepository>();
            _ = containerRegistry.Register<IDepartmentCompanyHolidayRepository, DepartmentCompanyHolidayRepository>();
            _ = containerRegistry.Register<IOwnCompanyHolidayRepository, OwnCompanyHolidayRepository>();
            _ = containerRegistry.Register<IEmployeeRepository, EmployeeRepository>();

            // Wada.IO
            _ = containerRegistry.Register<IFileStreamOpener, FileStreamOpener>();
            _ = containerRegistry.Register<IStreamReaderOpener, StreamReaderOpener>();

            // 設定保存
            _ = containerRegistry.Register<IApplicationConfigurationWriter, ApplicationConfigurationWriter>();
            _ = containerRegistry.Register<IStoreSelectedXlsxDirectoriesUseCase, StoreSelectedXlsxDirectoriesUseCase>();

            // DI 勤怠表エクセル
            _ = containerRegistry.Register<IAttendanceTableRepository, AttendanceTableRepository>();
            // DI 勤怠CSV
            _ = containerRegistry.Register<IEmployeeAttendanceCsvReader, EmployeeAttendanceCsvReader>();
            // 自社休日
            _ = containerRegistry.Register<IOwnCompanyHolidayListReader, OwnCompanyHolidayListReader>();
            _ = containerRegistry.Register<IRegisterOwnCompanyHolidayUseCase, RegisterOwnCompanyHolidayUseCase>();
            _ = containerRegistry.Register<IFetchOwnCompanyHolidayMaxDateUseCase, FetchOwnCompanyHolidayMaxDateUseCase>();

            // 勤怠エクセルと給与システムCSVを同異判定するUseCase
            _ = containerRegistry.Register<IDetermineDifferenceUseCase, DetermineDifferenceUseCase>();

            // 社員番号対応表読込
            _ = containerRegistry.Register<IMatchedEmployeeNumberListReader, MatchedEmployeeNumberSpreadSheetReader>();
            _ = containerRegistry.Register<IRegisterEmployeeNumberTableUseCase, RegisterEmployeeNumberTableUseCase>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);

            // Moduleを読み込む
            moduleCatalog.AddModule<VerifyAttendanceCSVModule>(InitializationMode.WhenAvailable);
        }

        // 設定情報ライブラリを作る
        static IConfigurationRoot MyConfigurationBuilder() =>
            // NOTE: https://tech-blog.cloud-config.jp/2019-7-11-how-to-configuration-builder/
            new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(path: "appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
    }
}
