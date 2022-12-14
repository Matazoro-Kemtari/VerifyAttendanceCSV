using ClosedXML.Excel;
using Wada.AOP.Logging;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.AttendanceTableAggregation;
using Wada.AttendanceTableService.ValueObjects;

[module: Logging] // https://stackoverflow.com/questions/49648179/how-to-use-methoddecorator-fody-decorator-in-another-project
namespace Wada.AttendanceSpreadSheet
{
    public class AttendanceTableRepository : IAttendanceTableRepository
    {
        //private readonly ILogger logger;
        private readonly IOwnCompanyHolidayRepository ownCompanyHolidayRepository;

        public AttendanceTableRepository(IOwnCompanyHolidayRepository ownCompanyHolidayRepository)
        {
            this.ownCompanyHolidayRepository = ownCompanyHolidayRepository;
        }

        [Logging]
        public AttendanceTable ReadByMonth(Stream stream, int month)
        {
            using var xlBook = new XLWorkbook(stream);
            IXLWorksheet targetSheet = SearchMonthSheet(xlBook, month);

            // 勤怠表の基本情報を取得
            (uint employeeNumber, AttendanceYear attendanceYear, AttendanceMonth attendanceMonth) =
                GetAttendanceTableBaseInfo(targetSheet);
            AttendanceTable attendanceTable = new(employeeNumber, attendanceYear, attendanceMonth);

            // 自社カレンダーを取得
            var calendar = ownCompanyHolidayRepository.FindByYearMonth(attendanceYear.Value, attendanceMonth.Value);
            HolidayClassification FindByDay(DateTime day)
            {
                var result = calendar?.SingleOrDefault(x => x.HolidayDate == day);
                return result == null ? HolidayClassification.None : result.HolidayClassification;
            }

            // ヘッダ部分をスキップして走査
            IEnumerable<IXLRow> rows = targetSheet.Rows().Skip(4);
            foreach (IXLRow row in rows)
            {
                if (row.RowNumber() == 36)
                    break;

                const string StartedTimeColumnLetter = "E";
                const string EndedTimeColumnLetter = "F";
                const string DayOffColumnLetter = "D";
                const string DayColumnLetter = "B";
                const string RestTimeColumnLetter = "O";
                const string OrderedLunchBoxColumnLetter = "K";

                if (row.Cell(DayOffColumnLetter).IsEmpty()
                    && row.Cell(StartedTimeColumnLetter).IsEmpty()
                    && row.Cell(EndedTimeColumnLetter).IsEmpty())
                    continue;

                // 日付列
                if (!row.Cell(DayColumnLetter).TryGetValue(out int attendanceDay))
                {
                    string msg = $"日付が取得できません シート:{targetSheet.Name}, セル:{row.Cell(DayColumnLetter).Address}";
                    throw new AttendanceTableServiceException(msg);
                }

                // 日付の有効性判定
                DateTime date = new(attendanceYear.Value, attendanceMonth.Value, attendanceDay);
                if (date.Year != attendanceYear.Value || date.Month != attendanceMonth.Value)
                {
                    string msg = $"日付の値が範囲を超えています シート:{targetSheet.Name}, セル:{row.Cell(DayColumnLetter).Address}";
                    throw new AttendanceTableServiceException(msg);
                }

                AttendanceTime? startTime = null;
                AttendanceTime? endTime = null;
                TimeSpan? restTime = null;
                if (!row.Cell(StartedTimeColumnLetter).IsEmpty()
                    && !row.Cell(EndedTimeColumnLetter).IsEmpty())
                {
                    // 始業時間列
                    if (!row.Cell(StartedTimeColumnLetter).TryGetValue(out DateTime _startTime))
                    {
                        string msg = $"始業時間が取得できません シート:{targetSheet.Name}, セル:{row.Cell(StartedTimeColumnLetter).Address}";
                        throw new AttendanceTableServiceException(msg);
                    }
                    startTime = new AttendanceTime(date + _startTime.TimeOfDay);

                    // 終業時間列
                    if (!row.Cell(EndedTimeColumnLetter).TryGetValue(out DateTime _endTime))
                    {
                        string msg = $"終業時間が取得できません シート:{targetSheet.Name}, セル:{row.Cell(EndedTimeColumnLetter).Address}";
                        throw new AttendanceTableServiceException(msg);
                    }
                    if (_startTime.TimeOfDay > _endTime.TimeOfDay)
                        _endTime.Add(new TimeSpan(1, 0, 0, 0));
                    endTime = new AttendanceTime(date + _endTime.TimeOfDay);

                    // 休憩時間列
                    if (!row.Cell(RestTimeColumnLetter).TryGetValue(out DateTime _restTime))
                    {
                        string msg = $"休憩時間が取得できません シート:{targetSheet.Name}, セル:{row.Cell(RestTimeColumnLetter).Address}";
                        throw new AttendanceTableServiceException(msg);
                    }
                    restTime = _restTime.TimeOfDay;
                }

                // 勤務列
                if (!row.Cell(DayOffColumnLetter).TryGetValue(out string _dayOffValue))
                {
                    string msg = $"勤務が取得できません シート:{targetSheet.Name}, セル:{row.Cell(DayOffColumnLetter).Address}";
                    throw new AttendanceTableServiceException(msg);
                }
                DayOffClassification dayOff = ConvertDayOffClassification(_dayOffValue);
                if (dayOff == DayOffClassification.None && startTime != null && endTime != null)
                    // 遅刻早退の判定
                    dayOff = DetermineLateEarly(startTime.Value, endTime.Value);

                // 弁当列
                if (!row.Cell(OrderedLunchBoxColumnLetter).TryGetValue(out string _orderedLunchBox))
                {
                    string msg = $"弁当が取得できません シート:{targetSheet.Name}, セル:{row.Cell(OrderedLunchBoxColumnLetter).Address}";
                    throw new AttendanceTableServiceException(msg);
                }
                OrderedLunchBox orderedLunchBox = ConvertOrderedLunchBox(_orderedLunchBox);


                AttendanceRecord attendanceRecord = new(
                    new AttendanceDay(attendanceYear, attendanceMonth, attendanceDay),
                    FindByDay(date),
                    dayOff,
                    startTime,
                    endTime,
                    restTime,
                    orderedLunchBox
                    );
                attendanceTable.AttendanceRecords.Add(attendanceRecord);
            }

            return attendanceTable;
        }

        private OrderedLunchBox ConvertOrderedLunchBox(string orderedLunchBox) => orderedLunchBox switch
        {
            "注" => OrderedLunchBox.Orderd,
            _ => OrderedLunchBox.None,
        };

        private static DayOffClassification DetermineLateEarly(DateTime startTime, DateTime endTime)
        {
            // 一般勤務 8:00-17:00
            // 交代勤務1 15:00-24:00
            // 交代勤務2 20:00-5:00
            // 遅刻しても残業すれば帳消しになる制度を考慮する
            DateTime startGeneralShift = startTime.Date.AddHours(8);
            DateTime startLateShift = startTime.Date.AddHours(15);
            DateTime startNightShift = startTime.Date.AddHours(20);

            TimeSpan timeSpan = endTime - startTime;
            return timeSpan.TotalHours >= 9
                ? DayOffClassification.None
                : startTime == startGeneralShift || startTime == startLateShift || startTime == startNightShift
                    ? DayOffClassification.EarlyLeave
                    : DayOffClassification.Lateness;
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
                throw new AttendanceTableServiceException(msg);
            }
            AttendanceYear year = new(yearMonth.Year);
            AttendanceMonth month = new(yearMonth.Month);

            if (!targetSheet.Cell("G2").TryGetValue(out uint employeeNumber))
            {
                string msg = $"社員番号が取得できません シート:{targetSheet.Name}, セル:G2";
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
                throw new AttendanceTableServiceException(msg);
            }

            return targetSheet;
        }
    }
}