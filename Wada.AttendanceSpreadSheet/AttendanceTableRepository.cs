using ClosedXML.Excel;
using Wada.AOP.Logging;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.AttendanceTableAggregation;
using Wada.AttendanceTableService.OwnCompanyCalendarAggregation;
using Wada.AttendanceTableService.ValueObjects;

namespace Wada.AttendanceSpreadSheet;

public class AttendanceTableRepository : IAttendanceTableRepository
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IDepartmentCompanyHolidayRepository _departmentCompanyHolidayRepository;
    private readonly IOwnCompanyHolidayRepository _ownCompanyHolidayRepository;

    public AttendanceTableRepository(IEmployeeRepository employeeRepository, IDepartmentCompanyHolidayRepository departmentCompanyHolidayRepository, IOwnCompanyHolidayRepository ownCompanyHolidayRepository)
    {
        _employeeRepository = employeeRepository;
        _departmentCompanyHolidayRepository = departmentCompanyHolidayRepository;
        _ownCompanyHolidayRepository = ownCompanyHolidayRepository;
    }

    [Logging]
    public async Task<AttendanceTable> ReadByMonthAsync(Stream stream, int month)
    {
        // Excelファイルを読み込む
        using var xlBook = new XLWorkbook(stream);

        // 指定された月のシートを取得
        var targetSheet = SearchMonthSheet(xlBook, month);

        // 勤怠表の基本情報を取得
        var attendanceTable = await GetAttendanceTableBaseInfoAsync(targetSheet);

        // 休日カレンダーを取得
        IEnumerable<OwnCompanyHoliday> calendar = await FetchOwnCompanyHolidaysAsync(attendanceTable);

        // 勤怠表の詳細を取得
        GetAttendanceTableDetail(targetSheet, calendar, attendanceTable)
            .ToList()
            .ForEach(x => attendanceTable.AttendanceRecords.Add(x));

        return attendanceTable;
    }

    /// <summary>
    /// 勤怠表の詳細を取得
    /// </summary>
    /// <param name="targetSheet"></param>
    /// <param name="calendar"></param>
    /// <param name="attendanceTable"></param>
    /// <exception cref="DomainException"></exception>
    [Logging]
    private static IEnumerable<AttendanceRecord> GetAttendanceTableDetail(IXLWorksheet targetSheet, IEnumerable<OwnCompanyHoliday> calendar, AttendanceTable attendanceTable)
    {
        const int HeaderRowCount = 4;
        const string StartedTimeColumnLetter = "E";
        const string EndedTimeColumnLetter = "F";
        const string DayOffColumnLetter = "D";
        const string DayColumnLetter = "B";
        const string RestTimeColumnLetter = "O";

        return targetSheet.Rows()
            // ヘッダ部分をスキップ
            .Skip(HeaderRowCount)
            // 36行以降はデータがないため終了する
            .TakeWhile(row => row.RowNumber() < 36)
            .Select(row =>
            {
                var dayOffCell = row.Cell(DayOffColumnLetter);
                var startedTimeCell = row.Cell(StartedTimeColumnLetter);
                var endedTimeCell = row.Cell(EndedTimeColumnLetter);
                var dayCell = row.Cell(DayColumnLetter);
                var restTimeCell = row.Cell(RestTimeColumnLetter);

                if (dayOffCell.IsEmpty() && startedTimeCell.IsEmpty() && endedTimeCell.IsEmpty())
                {
                    return null;
                }

                return CreateAttendanceRecordFromWorksheet(targetSheet,
                                                           attendanceTable,
                                                           calendar,
                                                           dayOffCell,
                                                           startedTimeCell,
                                                           endedTimeCell,
                                                           dayCell,
                                                           restTimeCell);
            })
            .Where(record => record != null)
            .Cast<AttendanceRecord>();
    }

    private static AttendanceRecord CreateAttendanceRecordFromWorksheet(
        IXLWorksheet targetSheet,
        AttendanceTable attendanceTable,
        IEnumerable<OwnCompanyHoliday> calendar,
        IXLCell dayOffCell,
        IXLCell startedTimeCell,
        IXLCell endedTimeCell,
        IXLCell dayCell,
        IXLCell restTimeCell)
    {
        // 勤務日を取得
        (int attendanceDay, DateTime attendanceDate) = GetAttendanceDayFromCell(targetSheet, attendanceTable, dayCell);

        // 始業時間、終業時間を取得
        (AttendanceTime? startTime, AttendanceTime? endTime, TimeSpan? restTime) =
            GetAttendanceTimeFromCells(targetSheet, startedTimeCell, endedTimeCell, restTimeCell, attendanceDate);

        // 勤務区分を取得
        DayOffClassification dayOff = GetDayOffClassificationFromCell(targetSheet, dayOffCell, startTime, endTime);

        return new AttendanceRecord(
                new AttendanceDay(attendanceTable.Year, attendanceTable.Month, attendanceDay),
                FindHoliday(attendanceDate, calendar),
                dayOff,
                startTime,
                endTime,
                restTime
                );
    }

    /// <summary>
    /// 勤務区分を取得
    /// </summary>
    /// <param name="targetSheet"></param>
    /// <param name="dayOffCell"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
    /// <exception cref="DomainException"></exception>
    [Logging]
    private static DayOffClassification GetDayOffClassificationFromCell(IXLWorksheet targetSheet, IXLCell dayOffCell, AttendanceTime? startTime, AttendanceTime? endTime)
    {
        // 勤務列
        if (!dayOffCell.TryGetValue(out string _dayOffValue))
        {
            string msg = $"勤務が取得できません シート:{targetSheet.Name}, セル:{dayOffCell.Address}";
            throw new DomainException(msg);
        }
        var dayOff = ConvertDayOffClassification(_dayOffValue);
        if (dayOff == DayOffClassification.None && startTime != null && endTime != null)
            // 遅刻早退の判定
            dayOff = DetermineLateEarly(startTime.Value, endTime.Value);
        return dayOff;
    }

    /// <summary>
    /// 始業時間列、終業時間列からAttendanceTimeを取得
    /// </summary>
    /// <param name="targetSheet"></param>
    /// <param name="startedTimeCell"></param>
    /// <param name="endedTimeCell"></param>
    /// <param name="restTimeCell"></param>
    /// <param name="attendanceDate"></param>
    /// <returns></returns>
    /// <exception cref="DomainException"></exception>
    [Logging]
    private static (AttendanceTime? startTime, AttendanceTime? endTime, TimeSpan? restTime) GetAttendanceTimeFromCells(
        IXLWorksheet targetSheet,
        IXLCell startedTimeCell,
        IXLCell endedTimeCell,
        IXLCell restTimeCell,
        DateTime attendanceDate)
    {
        AttendanceTime? startTime, endTime;
        startTime = null;
        endTime = null;
        TimeSpan? restTime;
        restTime = null;

        if (!startedTimeCell.IsEmpty()
            && !endedTimeCell.IsEmpty())
        {
            // 始業時間列
            if (!startedTimeCell.TryGetValue(out DateTime _startTime))
            {
                string msg = $"始業時間が取得できません シート:{targetSheet.Name}, セル:{startedTimeCell.Address}";
                throw new DomainException(msg);
            }
            startTime = new AttendanceTime(attendanceDate + _startTime.TimeOfDay);

            // 終業時間列
            if (!endedTimeCell.TryGetValue(out DateTime _endTime))
            {
                string msg = $"終業時間が取得できません シート:{targetSheet.Name}, セル:{endedTimeCell.Address}";
                throw new DomainException(msg);
            }
            if (_startTime.TimeOfDay > _endTime.TimeOfDay)
                _endTime.Add(new TimeSpan(1, 0, 0, 0));
            endTime = new AttendanceTime(attendanceDate + _endTime.TimeOfDay);

            // 休憩時間列
            if (!restTimeCell.TryGetValue(out DateTime _restTime))
            {
                string msg = $"休憩時間が取得できません シート:{targetSheet.Name}, セル:{restTimeCell.Address}";
                throw new DomainException(msg);
            }
            restTime = _restTime.TimeOfDay;
        }

        return (startTime, endTime, restTime);
    }

    /// <summary>
    /// 勤務日を取得
    /// </summary>
    /// <param name="targetSheet"></param>
    /// <param name="attendanceTable"></param>
    /// <param name="dayCell"></param>
    /// <returns></returns>
    /// <exception cref="DomainException"></exception>
    [Logging]
    private static (int attendanceDay, DateTime attendanceDate) GetAttendanceDayFromCell(IXLWorksheet targetSheet, AttendanceTable attendanceTable, IXLCell dayCell)
    {
        // 日付列
        if (!dayCell.TryGetValue(out int attendanceDay))
        {
            string msg = $"日付が取得できません シート:{targetSheet.Name}, セル:{dayCell.Address}";
            throw new DomainException(msg);
        }

        // 日付の有効性判定
        var attendanceDate = new DateTime(attendanceTable.Year.Value, attendanceTable.Month.Value, attendanceDay);
        if (attendanceDate.Year != attendanceTable.Year.Value || attendanceDate.Month != attendanceTable.Month.Value)
        {
            string msg = $"日付の値が範囲を超えています シート:{targetSheet.Name}, セル:{dayCell.Address}";
            throw new DomainException(msg);
        }
        return (attendanceDay, attendanceDate);
    }

    /// <summary>
    /// 社員番号→部署ID→カレンダーグループID順に自社カレンダーを取得する
    /// </summary>
    /// <param name="attendanceTable"></param>
    /// <returns></returns>
    /// <exception cref="DomainException"></exception>
    [Logging]
    private async Task<IEnumerable<OwnCompanyHoliday>> FetchOwnCompanyHolidaysAsync(AttendanceTable attendanceTable)
    {
        // 社員情報取得
        var employee = await _employeeRepository.FindByEmployeeNumberAsync(attendanceTable.EmployeeNumber);
        if (employee.DepartmentId == null)
            throw new DomainException(
                "受注管理上で所属部署が確認できません\n" +
                $"受注管理を確認してください 社員番号: {attendanceTable.EmployeeNumber}");

        // 部署カレンダーID取得
        var departmentHoliday = await _departmentCompanyHolidayRepository.FindByDepartmentIdAsync(employee.DepartmentId.Value);

        // 自社カレンダーを取得
        var calendar = await _ownCompanyHolidayRepository.FindByYearMonthAsync(departmentHoliday.CalendarGroupId, attendanceTable.Year.Value, attendanceTable.Month.Value);
        return calendar;
    }

    [Logging]
    private static HolidayClassification FindHoliday(DateTime day, IEnumerable<OwnCompanyHoliday>? calendar)
    {
        var result = calendar?.SingleOrDefault(x => x.HolidayDate == day);
        return result == null ? HolidayClassification.None : result.HolidayClassification;
    }

    [Logging]
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
        "特休有給" => DayOffClassification.PaidSpecialLeave,
        "特休無給" => DayOffClassification.UnpaidSpecialLeave,
        _ => DayOffClassification.None,
    };

    /// <summary>
    /// 勤怠表の基本情報を取得する
    /// </summary>
    /// <param name="targetSheet"></param>
    /// <returns></returns>
    /// <exception cref="DomainException"></exception>
    [Logging]
    private static Task<AttendanceTable> GetAttendanceTableBaseInfoAsync(IXLWorksheet targetSheet)
        => Task.Run(() =>
        {
            if (!targetSheet.Cell("A1").TryGetValue(out DateTime yearMonth))
            {
                string msg = $"年月が取得できません シート:{targetSheet.Name}, セル:A1";
                throw new DomainException(msg);
            }
            AttendanceYear year = new(yearMonth.Year);
            AttendanceMonth month = new(yearMonth.Month);

            if (!targetSheet.Cell("G2").TryGetValue(out uint employeeNumber))
            {
                string msg = $"社員番号が取得できません シート:{targetSheet.Name}, セル:G2";
                throw new DomainException(msg);
            }

            return new AttendanceTable(employeeNumber, year, month);
        });

    /// <summary>
    /// 指定付きのシートを取得する
    /// </summary>
    /// <param name="xlBook"></param>
    /// <param name="month"></param>
    /// <returns></returns>
    /// <exception cref="DomainException"></exception>
    [Logging]
    private static IXLWorksheet SearchMonthSheet(XLWorkbook xlBook, int month)
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
            throw new DomainException(msg);
        }

        return targetSheet;
    }
}