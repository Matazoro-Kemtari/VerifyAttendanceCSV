using NLog;
using System.Reflection;
using System.Text;
using Wada.AttendanceTableService;

namespace Wada.AttendanceCSV
{
    public class StreamReaderOpener : IStreamReaderOpener
    {
        private readonly ILogger logger;

        public StreamReaderOpener(ILogger logger)
        {
            this.logger = logger;
        }

        public StreamReader Open(string path)
        {
            logger.Debug($"Start {MethodBase.GetCurrentMethod()?.Name}");

            StreamReader reader = new(path);

            logger.Debug($"Finish {MethodBase.GetCurrentMethod()?.Name}");

            return reader;
        }
    }
}