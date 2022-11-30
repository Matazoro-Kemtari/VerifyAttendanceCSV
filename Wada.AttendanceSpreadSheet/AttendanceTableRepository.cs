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
        private readonly IWadaHolidayRepository wadaHolidayRepository;

        public AttendanceTableRepository(ILogger logger, IWadaHolidayRepository wadaHolidayRepository)
        {
            this.logger = logger;
            this.wadaHolidayRepository = wadaHolidayRepository;
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
                if (row.RowNumber() == 36)
                    break;

                if (row.Cell("D").IsEmpty() && row.Cell("O").IsEmpty() && row.Cell("P").IsEmpty())
                    continue;

                // 日付列
                if (!row.Cell("B").TryGetValue(out int attendanceDay))
                {
                    string msg = $"日付が取得できません シート:{targetSheet.Name}, セル:{row.Cell("B").Address}";
                    logger.Error(msg);
                    throw new AttendanceTableServiceException(msg);
                }

                // 日付の有効性判定
                DateTime date = new(attendanceYear.Value, attendanceMonth.Value, attendanceDay);
                if (date.Year != attendanceYear.Value || date.Month != attendanceMonth.Value)
                {
                    string msg = $"日付の値が範囲を超えています シート:{targetSheet.Name}, セル:{row.Cell("B").Address}";
                    logger.Error(msg);
                    throw new AttendanceTableServiceException(msg);
                }

                AttendanceTime? startTime = null;
                AttendanceTime? endTime = null;
                TimeSpan? restTime = null;
                if (!row.Cell("O").IsEmpty() && !row.Cell("P").IsEmpty() && !row.Cell("Q").IsEmpty())
                {
                    // 始業時間列
                    if (!row.Cell("O").TryGetValue(out DateTime _startTime))
                    {
                        string msg = $"始業時間が取得できません シート:{targetSheet.Name}, セル:{row.Cell("O").Address}";
                        logger.Error(msg);
                        throw new AttendanceTableServiceException(msg);
                    }
                    startTime = new AttendanceTime(date + _startTime.TimeOfDay);

                    // 終業時間列
                    if (!row.Cell("P").TryGetValue(out DateTime _endTime))
                    {
                        string msg = $"終業時間が取得できません シート:{targetSheet.Name}, セル:{row.Cell("P").Address}";
                        logger.Error(msg);
                        throw new AttendanceTableServiceException(msg);
                    }
                    if (_startTime.TimeOfDay > _endTime.TimeOfDay)
                        _endTime.Add(new TimeSpan(1, 0, 0, 0));
                    endTime = new AttendanceTime(date + _endTime.TimeOfDay);

                    // 休憩時間列
                    if (!row.Cell("Q").TryGetValue(out DateTime _restTime))
                    {
                        string msg = $"休憩時間が取得できません シート:{targetSheet.Name}, セル:{row.Cell("Q").Address}";
                        logger.Error(msg);
                        throw new AttendanceTableServiceException(msg);
                    }
                    restTime = _restTime.TimeOfDay;
                }

                // 勤務列
                if (!row.Cell("D").TryGetValue(out string _dayOffValue))
                {
                    string msg = $"勤務が取得できません シート:{targetSheet.Name}, セル:{row.Cell("D").Address}";
                    logger.Error(msg);
                    throw new AttendanceTableServiceException(msg);
                }
                DayOffClassification dayOff = ConvertDayOffClassification(_dayOffValue);
                if (dayOff == DayOffClassification.None && startTime != null && endTime != null)
                    // 遅刻早退の判定
                    dayOff = DetermineLateEarly(startTime.Value, endTime.Value);

                AttendanceRecord attendanceRecord = new(
                    new AttendanceDay(attendanceYear, attendanceMonth, attendanceDay),
                    wadaHolidayRepository.FindByDay(date),
                    dayOff,
                    startTime,
                    endTime,
                    restTime,
                    OrderedLunchBox.None
                    );
                attendanceTable.AttendanceRecords.Add(attendanceRecord);
            }

            logger.Debug($"Finish {MethodBase.GetCurrentMethod()?.Name}");

            return attendanceTable;
        }

        private DayOffClassification DetermineLateEarly(DateTime startTime, DateTime endTime)
        {
            // 一般勤務 8:00-17:00
            // 交代勤務1 15:00-24:00
            // 交代勤務2 20:00-5:00
            // 遅刻しても残業すれば帳消しになる制度を考慮する
            DateTime startGeneralShift = startTime.Date.AddHours(8);
            DateTime startLateShift = startTime.Date.AddHours(15);
            DateTime startNightShift = startTime.Date.AddHours(20);

            TimeSpan timeSpan = endTime - startTime;
            return timeSpan.TotalHours > 9
                ? DayOffClassification.None
                : startTime == startGeneralShift || startTime == startLateShift || startTime == startNightShift
                    ? DayOffClassification.EarlyLeave
                    : DayOffClassification.BeLate;
        }

        private static DayOffClassification ConvertDayOffClassification(string dayOffValue) => dayOffValue switch
        {
            "有休" => DayOffClassification.PaidLeave,
            "AM有" => DayOffClassification.AMPaidLeave,
            "PM有" => DayOffClassification.PMPaidLeave,
            "振休" => DayOffClassification.SubstitutedHoliday,
            "振出" => DayOffClassification.TransferedAttendance,
            "休出" => DayOffClassification.HolidayWorked,
            "欠勤" => DayOffClassification.Absence,
            "休業" => DayOffClassification.BusinessSuspension,
            "AM休業" => DayOffClassification.AMBusinessSuspension,
            "PM休業" => DayOffClassification.PMBusinessSuspension,
            "特休有給" => DayOffClassification.PaidSpecialLeave,
            "特休無給" => DayOffClassification.UnpaidSpecialLeave,
            _ => DayOffClassification.None,
        };

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