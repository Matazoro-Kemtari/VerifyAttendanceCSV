namespace Wada.StoreSelectedXlsxDirectoriesApplication;

public interface IStoreSelectedXlsxDirectoriesUseCase
{
    Task ExecuteAsync(IEnumerable<IStoreXlsxDirectoryParam> storeXlsxDirectories);
}

public class StoreSelectedXlsxDirectoriesUseCase : IStoreSelectedXlsxDirectoriesUseCase
{
    private readonly IApplicationConfigurationWriter _applicationConfigurationWriter;

    public StoreSelectedXlsxDirectoriesUseCase(IApplicationConfigurationWriter applicationConfigurationWriter)
    {
        _applicationConfigurationWriter = applicationConfigurationWriter;
    }

    public Task ExecuteAsync(IEnumerable<IStoreXlsxDirectoryParam> storeXlsxDirectories)
        => _applicationConfigurationWriter.StoreSelectedXlsxDirectoriesAsync(storeXlsxDirectories);
}