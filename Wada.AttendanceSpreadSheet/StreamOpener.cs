using NLog;
using System.Reflection;
using Wada.AttendanceTableService;

namespace Wada.AttendanceSpreadSheet
{
    public class StreamOpener : IStreamOpener
    {
        private readonly ILogger logger;

        public StreamOpener(ILogger logger)
        {
            this.logger = logger;
        }

        public Stream Open(string path)
        {
            logger.Debug($"Start {MethodBase.GetCurrentMethod()?.Name}");

            Stream reader;
            try
            {
                reader = OpenFileStream(path);
            }
            catch (FileNotFoundException e)
            {
                string msg = "ファイルが見つかりません";
                logger.Error(e, msg);
                throw new AttendanceTableServiceException(msg);
            }
            catch (IOException e)
            {
                string msg = "ファイルが使用中です";
                logger.Error(e, msg);
                throw new AttendanceTableServiceException(msg);
            }

            logger.Debug($"Finish {MethodBase.GetCurrentMethod()?.Name}");

            return reader;
        }

        private static FileStream OpenFileStream(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("ファイルが見つかりません", filePath);
            }

            return File.Open(filePath, FileMode.Open);
        }
    }
}
