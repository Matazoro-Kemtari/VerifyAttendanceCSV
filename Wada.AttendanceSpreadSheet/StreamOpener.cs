﻿using Wada.AOP.Logging;
using Wada.AttendanceTableService;

namespace Wada.AttendanceSpreadSheet
{
    public class StreamOpener : IStreamOpener
    {
        [Logging]
        public Stream Open(string path)
        {
            Stream reader;
            try
            {
                reader = OpenFileStream(path);
            }
            catch (FileNotFoundException ex)
            {
                string msg = "ファイルが見つかりません";
                throw new AttendanceTableServiceException(msg, ex);
            }
            catch (IOException ex)
            {
                string msg = "ファイルが使用中です";
                throw new AttendanceTableServiceException(msg, ex);
            }

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
