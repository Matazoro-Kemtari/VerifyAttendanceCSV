﻿using Microsoft.Extensions.Configuration;
using NLog;
using Prism.Ioc;
using Prism.Modularity;
using System.IO;
using System.Windows;
using VerifyAttendanceCSV.Views;
using Wada.AttendanceCSV;
using Wada.AttendanceSpreadSheet;
using Wada.AttendanceTableService;
using Wada.CommonDialogLib;
using Wada.DesignDepartmentDataBse;
using Wada.DetermineDifferenceApplication;
using Wada.OrderDataBase;
using Wada.RegisterOwnCompanyHolidayApplication;
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

            // DI 勤怠表エクセル
            _ = containerRegistry.Register<IMatchedEmployeeNumberRepository, MatchedEmployeeNumberRepository>();
            _ = containerRegistry.Register<IOwnCompanyHolidayRepository, OwnCompanyHolidayRepository>();
            _ = containerRegistry.Register<IStreamOpener, StreamOpener>();
            _ = containerRegistry.Register<IAttendanceTableRepository, AttendanceTableRepository>();
            // DI 勤怠CSV
            _ = containerRegistry.Register<IStreamReaderOpener, StreamReaderOpener>();
            _ = containerRegistry.Register<IEmployeeAttendanceRepository, EmployeeAttendanceRepository>();
            // 自社休日
            _ = containerRegistry.Register<IFetchOwnCompanyHolidayMaxDateUseCase, FetchOwnCompanyHolidayMaxDateUseCase>();

            // 勤怠エクセルと給与システムCSVを同異判定するUseCase
            _ = containerRegistry.Register<IEmployeeRepository, EmployeeRepository>();
            _ = containerRegistry.Register<IDetermineDifferenceUseCase, DetermineDifferenceUseCase>();

            // Presentation
            _ = containerRegistry.Register<ICommonDialogSwitcher, CommonDialogSwitcher>();
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
