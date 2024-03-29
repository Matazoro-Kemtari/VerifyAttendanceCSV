﻿using System.Transactions;
using Wada.AOP.Logging;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.OwnCompanyCalendarAggregation;
using Wada.AttendanceTableService.ValueObjects;
using Wada.Extensions;

namespace Wada.RegisterOwnCompanyHolidayApplication;

public interface IRegisterOwnCompanyHolidayUseCase
{
    /// <summary>
    /// 自社カレンダーを登録する
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="calendarGroupClass"></param>
    /// <returns></returns>
    Task ExecuteAsync(string filePath, CalendarGroupAttempt calendarGroupClass);
}

public class RegisterOwnCompanyHolidayUseCase : IRegisterOwnCompanyHolidayUseCase
{
    private readonly IFileStreamOpener _fileStreamOpener;
    private readonly IOwnCompanyHolidayListReader _ownCompanyHolidayListReader;
    private readonly IOwnCompanyHolidayRepository _ownCompanyHolidayRepository;

    public RegisterOwnCompanyHolidayUseCase(
        IFileStreamOpener fileStreamOpener,
        IOwnCompanyHolidayListReader ownCompanyHolidayListReader,
        IOwnCompanyHolidayRepository ownCompanyHolidayRepository)
    {
        _fileStreamOpener = fileStreamOpener;
        _ownCompanyHolidayListReader = ownCompanyHolidayListReader;
        _ownCompanyHolidayRepository = ownCompanyHolidayRepository;
    }

    [Logging]
    public async Task ExecuteAsync(string filePath, CalendarGroupAttempt calendarGroupClass)
    {
        try
        {
            // データファイルを読み込む
            using var stream = await _fileStreamOpener.OpenAsync(filePath);
            var additionalEmployeeNumbers = await _ownCompanyHolidayListReader.ReadAllAsync(stream, OwnCompanyHoliday.GetCalendarGroupId((CalendarGroupClassification)calendarGroupClass));

            // 読み込んだ最小日の月初と最大日の月末の範囲で削除する
            var _minDate = additionalEmployeeNumbers.Min(x => x.HolidayDate);
            var deletableMinDate = new DateTime(_minDate.Year, _minDate.Month, 1);
            var _maxDate = additionalEmployeeNumbers.Max(x => x.HolidayDate);
            var deletableMaxDate = new DateTime(_maxDate.Year, _maxDate.Month, DateTime.DaysInMonth(_maxDate.Year, _maxDate.Month));

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            IEnumerable<OwnCompanyHoliday> deletableHolidays = Enumerable.Empty<OwnCompanyHoliday>();
            try
            {
                // 消すべきレコードを求める
                var _deletableHolidays = await _ownCompanyHolidayRepository.FindByAfterDateAsync(OwnCompanyHoliday.GetCalendarGroupId((CalendarGroupClassification)calendarGroupClass), deletableMinDate);
                deletableHolidays = _deletableHolidays.Where(x => x.HolidayDate <= deletableMaxDate);
            }
            catch (OwnCompanyCalendarAggregationException)
            {
                // 消すべきレコードがFindByAfterDateAsyncで見つからな場合発生するので無視
            }

            if (deletableHolidays.Any())
                // 一旦消す
                await _ownCompanyHolidayRepository.RemoveRangeAsync(deletableHolidays);

            // データベースに登録する
            await _ownCompanyHolidayRepository.AddRangeAsync(
            additionalEmployeeNumbers);
            scope.Complete();
        }
        catch (Exception ex) when (ex is DomainException or OwnCompanyCalendarAggregationException)
        {
            throw new UseCaseException(ex.Message, ex);
        }
    }
}

/// <summary>
/// 会社カレンダー グループ
/// </summary>
public enum CalendarGroupAttempt
{
    [EnumDisplayName("本社")]
    HeadOffice,

    [EnumDisplayName("松阪")]
    MatsuzakaOffice,
}