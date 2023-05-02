namespace Wada.StoreSelectedXlsxDirectoriesApplication;

public interface IApplicationConfigurationWriter
{
    Task StoreSelectedXlsxDirectoriesAsync(IEnumerable<IStoreXlsxDirectoryParam> storeXlsxDirectories);
}

public interface IStoreXlsxDirectoryParam
{
    string DirectoryPath { get; }
}
