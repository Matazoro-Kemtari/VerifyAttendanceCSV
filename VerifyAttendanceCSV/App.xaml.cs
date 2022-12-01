using Example;
using Microsoft.Extensions.Configuration;
using NLog;
using Prism.Ioc;
using System.IO;
using System.Windows;
using VerifyAttendanceCSV.Views;
using Wada.AttendanceCSV;
using Wada.AttendanceSpreadSheet;
using Wada.AttendanceTableService;

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
            _ = containerRegistry.Register<IAttendanceTableRepository, AttendanceTableRepository>();
            // DI 勤怠CSV
            _ = containerRegistry.Register<IEmployeeAttendanceRepository, EmployeeAttendanceRepository>();
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
