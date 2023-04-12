using Wada.AOP.Logging;
using Wada.AttendanceTableService;

namespace Wada.IO;

public class StreamReaderOpener : IStreamReaderOpener
{
    [Logging]
    public StreamReader Open(string path)
    {
        FileStreamOptions fileStreamOptions = new()
        {
            Access = FileAccess.Read,
            Mode = FileMode.Open,
            Share = FileShare.ReadWrite,
        };
        StreamReader reader = new(path, fileStreamOptions);

        return reader;
    }
}