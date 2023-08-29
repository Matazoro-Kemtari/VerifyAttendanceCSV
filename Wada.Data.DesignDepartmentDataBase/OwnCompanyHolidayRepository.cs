using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NLog;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.OwnCompanyCalendarAggregation;
using Wada.AttendanceTableService.ValueObjects;
using Wada.DataBase.EFCore.DesignDepartment;

namespace Wada.Data.DesignDepartmentDataBase;

public class OwnCompanyHolidayRepository : IOwnCompanyHolidayRepository
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    public OwnCompanyHolidayRepository(IConfiguration configuration, ILogger logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task AddRangeAsync(IEnumerable<OwnCompanyHoliday> ownCompanyHolidays)
    {
        using var dbContext = new DesignDepartmentContext(_configuration);

        int _additionalCount;
        try
        {
            await dbContext.OwnCompanyHolidays
                .AddRangeAsync(
                ownCompanyHolidays.Select(
                    x => new DataBase.EFCore.DesignDepartment.Entities.OwnCompanyHoliday(
                        x.CalendarGroupId,
                        x.HolidayDate,
                        x.HolidayClassification == HolidayClassification.LegalHoliday)));

            _additionalCount = await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            string msg = "自社カレンダーに追加できませんでした";
            throw new OwnCompanyCalendarAggregationException(msg, ex);
        }
        _logger.Trace($"自社カレンダーに{_additionalCount}件追加しました");
    }

    public async Task<IEnumerable<OwnCompanyHoliday>> FindByAfterDateAsync(string calendarGroupId, DateTime date)
    {
        using var dbContext = new DesignDepartmentContext(_configuration);

        var ownHoliday = dbContext.OwnCompanyHolidays
            .Where(x => x.CalendarGroupId == calendarGroupId)
            .Where(x => x.HolidayDate >= date)
            .Select(x => OwnCompanyHoliday.Reconstruct(
                calendarGroupId,
                x.HolidayDate,
                x.LegalHoliday ? HolidayClassification.LegalHoliday : HolidayClassification.RegularHoliday));

        if (!ownHoliday.Any())
        {
            string msg = $"自社カレンダーに該当がありませんでした "
                         + $"カレンダーグループ: {calendarGroupId}, 対象日: {date:yyyy年MM月dd日}";
            throw new OwnCompanyCalendarAggregationException(msg);
        }

        return await ownHoliday.ToListAsync();
    }

    public async Task<IEnumerable<OwnCompanyHoliday>> FindByYearMonthAsync(string calendarGroupId, int year, int month)
    {
        using var dbContext = new DesignDepartmentContext(_configuration);

        var ownHoliday = dbContext.OwnCompanyHolidays
            .Where(x => x.CalendarGroupId == calendarGroupId)
            .Where(x => x.HolidayDate >= new DateTime(year, month, 1))
            .Where(x => x.HolidayDate < new DateTime(year, month, 1).AddMonths(1))
            .Select(x => OwnCompanyHoliday.Reconstruct(
                calendarGroupId,
                x.HolidayDate,
                x.LegalHoliday ? HolidayClassification.LegalHoliday : HolidayClassification.RegularHoliday));

        if (!ownHoliday.Any())
        {
            string msg = $"自社カレンダーに該当がありませんでした "
                         + $"カレンダーグループ: {calendarGroupId}, 対象年月: {year}年{month}月";
            throw new OwnCompanyCalendarAggregationException(msg);
        }

        return await ownHoliday.ToListAsync();
    }

    public async Task RemoveRangeAsync(IEnumerable<OwnCompanyHoliday> ownCompanyHolidays)
    {
        using var dbContext = new DesignDepartmentContext(_configuration);

        int remoedCount;
        try
        {
            dbContext.OwnCompanyHolidays.RemoveRange(
                ownCompanyHolidays.Select(
                    x => new DataBase.EFCore.DesignDepartment.Entities.OwnCompanyHoliday(
                        x.CalendarGroupId,
                        x.HolidayDate,
                        x.HolidayClassification == HolidayClassification.LegalHoliday)));

            remoedCount = await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            string msg = "自社カレンダーから削除できませんでした";
            throw new OwnCompanyCalendarAggregationException(msg, ex);
        }

        _logger.Trace($"自社カレンダーから{remoedCount}件削除しました");
    }
}
