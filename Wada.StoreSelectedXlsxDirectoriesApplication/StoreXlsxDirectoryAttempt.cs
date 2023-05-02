namespace Wada.StoreSelectedXlsxDirectoriesApplication;

public record class StoreXlsxDirectoryAttempt : IStoreXlsxDirectoryParam
{
    private StoreXlsxDirectoryAttempt(string directoryPath)
    {
        DirectoryPath = directoryPath;
    }

    public static StoreXlsxDirectoryAttempt Create(string directoryPath)
        => new(directoryPath);

    public string DirectoryPath { get; init; }
}

public class TestStoreXlsxDirectoryAttemptFactory
{
    public static StoreXlsxDirectoryAttempt Create(string directoryPath = @"C:\Temp")
        => StoreXlsxDirectoryAttempt.Create(directoryPath);
}