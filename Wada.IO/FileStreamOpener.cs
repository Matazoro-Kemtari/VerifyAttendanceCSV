using Wada.AOP.Logging;
using Wada.AttendanceTableService;

namespace Wada.IO;

public class FileStreamOpener : IFileStreamOpener
{
    [Logging]
    public async Task<Stream> OpenAsync(string path)
    {
        if (!File.Exists(path))
        {
            string msg = $"ファイルが見つかりません ファイルパス: {path}";
            throw new DomainException(msg);
        }
        try
        {
            return await OpenFileStreamAsync(path);
        }
        catch (IOException ex)
        {
            string msg = $"ファイルが使用中です ファイルパス: {path}";
            throw new DomainException(msg, ex);
        }
    }

    [Logging]
    private static Task<FileStream> OpenFileStreamAsync(string filePath)
        => Task.Run(
            () => File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read));
}