using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Wada.StoreSelectedXlsxDirectoriesApplication;

namespace Wada.StoreApplicationConfiguration;

public class ApplicationConfigurationWriter : IApplicationConfigurationWriter
{
    private readonly string jsonFileName = "appsettings.json";

    public async Task StoreSelectedXlsxDirectoriesAsync(IEnumerable<IStoreXlsxDirectoryParam> storeXlsxDirectories)
    {
        if (storeXlsxDirectories is null)
            throw new ArgumentNullException(nameof(storeXlsxDirectories));

        // データクラスを詰め替える
        var _directories = storeXlsxDirectories.Select(x => x.DirectoryPath);

        var settingJson = await LoadAppConfigAsync();
        var _appConfig = settingJson.ApplicationConfiguration with { XLSXPaths = _directories };
        var _settingJson = settingJson with { ApplicationConfiguration = _appConfig };

        await StoreAppConfigAsync(_settingJson);
    }

    /// <summary>
    /// Newtonsoft.Jsonを使って書き込む
    /// </summary>
    /// <param name="appSettings"></param>
    /// <returns></returns>
    private async Task StoreAppConfigAsync(AppSettingsRecord appSettings)
    {
        var jsonString = JsonConvert.SerializeObject(
                appSettings,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });

        await File.WriteAllTextAsync(jsonFileName, jsonString);
    }

    /// <summary>
    /// Newtonsoft.Jsonを使って読み込む
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<AppSettingsRecord> LoadAppConfigAsync()
    {
        var jsonString = await File.ReadAllTextAsync(jsonFileName);

        return JsonConvert.DeserializeObject<AppSettingsRecord>(jsonString)
            ?? throw new InvalidOperationException(
                "設定ファイルが破損しています\n" +
                "内容を確認してください" +
                jsonFileName);
    }

    private record class AppSettingsRecord(
        ApplicationConfigurationRecord ApplicationConfiguration);
    private record class ApplicationConfigurationRecord(
        string CSVPath,
        IEnumerable<string> XLSXPaths,
        string HeadOfficeCalendarGroupId,
        string MatsuzakaOfficeCalendarGroupId);
}