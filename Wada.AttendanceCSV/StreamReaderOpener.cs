using Wada.AOP.Logging;
using Wada.AttendanceTableService;

namespace Wada.AttendanceCSV
{
    public class StreamReaderOpener : IStreamReaderOpener
    {
        [Logging]
        public StreamReader Open(string path)
        {
            StreamReader reader = new(path);

            return reader;
        }
    }
}