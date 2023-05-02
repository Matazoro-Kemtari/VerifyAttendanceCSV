using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.StoreSelectedXlsxDirectoriesApplication;

namespace Wada.StoreApplicationConfiguration.Tests
{
    [TestClass()]
    public class ApplicationConfigurationWriterTests
    {
        [TestMethod()]
        public async Task 正常系_xlsxディレクトリが書き込めること()
        {
            // given
            // when
            var writer = new ApplicationConfigurationWriter();
            var directories = new List<IStoreXlsxDirectoryParam>
            {
                TestStoreXlsxDirectoryAttemptFactory.Create(@"C:\Users\username\Documents"),
                TestStoreXlsxDirectoryAttemptFactory.Create(@"C:\Program Files\Google\Chrome"),
                TestStoreXlsxDirectoryAttemptFactory.Create(@"C:\Users\username\Downloads"),
            };
            await writer.StoreSelectedXlsxDirectoriesAsync(directories);

            // then
            IConfiguration configuration = MyConfigurationBuilder();
            CollectionAssert.AreEqual(
                directories.Select(x => x.DirectoryPath).ToList(),
                configuration.GetSection("applicationConfiguration:XLSXPaths").Get<string[]>());
            Assert.IsNotNull(configuration["applicationConfiguration:CSVPath"]);
            Assert.IsNotNull(configuration["applicationConfiguration:HeadOfficeCalendarGroupId"]);
            Assert.IsNotNull(configuration["applicationConfiguration:MatsuzakaOfficeCalendarGroupId"]);
        }

        // 設定情報ライブラリを作る
        static IConfigurationRoot MyConfigurationBuilder() =>
            new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(path: "appsettings.json", optional: true)
                .Build();
    }
}