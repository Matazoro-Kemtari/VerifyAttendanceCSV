using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using NLog;
using System.IO;
using System.Reflection;
using Wada.AttendanceTableService;

namespace Wada.AttendanceSpreadSheet
{
    public class AttendanceTableRepository : IAttendanceTableRepository
    {
        private readonly ILogger logger;

        public AttendanceTableRepository(ILogger logger)
        {
            this.logger = logger;
        }

        public AttendanceTable LoadMonth(Stream stream, int month)
        {
            logger.Debug($"Start {MethodBase.GetCurrentMethod()?.Name}");

            using var xlBook = new XLWorkbook(stream);
            IXLWorksheet targetSheet = SearchMonthSheet(xlBook, month);

            // 勤怠表の基本情報を取得
            (uint employeeNumber, AttendanceYear attendanceYear, AttendanceMonth attendanceMonth) =
                GetAttendanceTableBaseInfo(targetSheet);
            AttendanceTable attendanceTable = new(employeeNumber, attendanceYear, attendanceMonth);

            // ヘッダ部分をスキップして走査
            IEnumerable<IXLRow> rows = targetSheet.Rows().Skip(4);
            foreach (IXLRow row in rows)
            {

            }


            logger.Debug($"Finish {MethodBase.GetCurrentMethod()?.Name}");

            return attendanceTable;
        }

        /// <summary>
        /// 勤怠表の基本情報を取得する
        /// </summary>
        /// <param name="targetSheet"></param>
        /// <returns></returns>
        /// <exception cref="AttendanceTableServiceException"></exception>
        private (uint employeeNumber, AttendanceYear year, AttendanceMonth month) GetAttendanceTableBaseInfo(IXLWorksheet targetSheet)
        {
            if (!targetSheet.Cell("A1").TryGetValue(out DateTime yearMonth))
            {
                string msg = $"年月が取得できません シート:{targetSheet.Name}, セル:A1";
                logger.Error(msg);
                throw new AttendanceTableServiceException(msg);
            }
            AttendanceYear year = new(yearMonth.Year);
            AttendanceMonth month = new(yearMonth.Month);

            if (!targetSheet.Cell("G2").TryGetValue(out uint employeeNumber))
            {
                string msg = $"社員番号が取得できません シート:{targetSheet.Name}, セル:G2";
                logger.Error(msg);
                throw new AttendanceTableServiceException(msg);
            }

            return (employeeNumber, year, month);
        }

        private IXLWorksheet SearchMonthSheet(XLWorkbook xlBook, int month)
        {
            IXLWorksheet? targetSheet = null;
            string searchingSheetName = $"{month}月";
            foreach (var sheet in xlBook.Worksheets)
            {
                if (sheet.Name == searchingSheetName)
                {
                    targetSheet = sheet;
                    break;
                }
            }
            if (targetSheet == null)
            {
                string msg = $"{month}月のシートが見つかりません";
                logger.Error(msg);
                throw new AttendanceTableServiceException(msg);
            }

            return targetSheet;
        }
    }
}