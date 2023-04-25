using Microsoft.Extensions.Configuration;
using Wada.AOP.Logging;
using Wada.Data.DesignDepartmentDataBase.Models;
using Wada.Data.DesignDepartmentDataBase.Models.OwnCompanyCalendarAggregation;

namespace Wada.RegisterOwnCompanyHolidayApplication;

public interface IFetchOwnCompanyHolidayMaxDateUseCase
{
    Task<LastedHoliday> ExecuteAsyc();

    /// <summary>
    /// 環境情報を模倣する
    /// </summary>
    /// <param name="environment"></param>
    void MimicEnvironment(IEnvironment environment);
}

public class FetchOwnCompanyHolidayMaxDateUseCase : IFetchOwnCompanyHolidayMaxDateUseCase
{
    private readonly IConfiguration _configuration;
    private readonly IOwnCompanyHolidayRepository _ownCompanyHolidayRepository;

    private IEnvironment _environment = new DefaultEnvironment();

    public FetchOwnCompanyHolidayMaxDateUseCase(IConfiguration configuration, IOwnCompanyHolidayRepository ownCompanyHolidayRepository)
    {
        _configuration = configuration;
        _ownCompanyHolidayRepository = ownCompanyHolidayRepository;
    }

    [Logging]
    public async Task<LastedHoliday> ExecuteAsyc()
    {
        var headOfficeCalendarGroupId = _configuration["applicationConfiguration:HeadOfficeCalendarGroupId"]
            ?? throw new UseCaseException(
                "設定情報が取得できませんでした システム担当まで連絡してしてください\n" +
                "applicationConfiguration:HeadOfficeCalendarGroupId");
        var matsuzakaOfficeCalendarGroupId = _configuration["applicationConfiguration:MatsuzakaOfficeCalendarGroupId"]
            ?? throw new UseCaseException(
                "設定情報が取得できませんでした システム担当まで連絡してしてください\n" +
                "applicationConfiguration:MatsuzakaOfficeCalendarGroupId");

        var today = _environment.ObtainCurrentDate();
        var firstDayOfToday = today.AddDays(-(today.Day - 1));

        IEnumerable<OwnCompanyHoliday> headHolidays, matsuzakaHolidays;
        try
        {
            headHolidays = await _ownCompanyHolidayRepository.FindByAfterDateAsync(headOfficeCalendarGroupId, firstDayOfToday);
        }
        catch (OwnCompanyCalendarAggregationException)
        {
            headHolidays = Array.Empty<OwnCompanyHoliday>();
        }
        try
        {
            matsuzakaHolidays = await _ownCompanyHolidayRepository.FindByAfterDateAsync(matsuzakaOfficeCalendarGroupId, firstDayOfToday);
        }
        catch (OwnCompanyCalendarAggregationException)
        {
            matsuzakaHolidays = Array.Empty<OwnCompanyHoliday>();
        }

        return new LastedHoliday(
            headHolidays.Max(x => x.HolidayDate),
            matsuzakaHolidays.Max(x => x.HolidayDate));
    }

    public void MimicEnvironment(IEnvironment environment) => _environment = environment;

    private class DefaultEnvironment : IEnvironment
    {
        public DateTime ObtainCurrentDate() => DateTime.Now.Date;
    }
}

public interface IEnvironment
{
    public DateTime ObtainCurrentDate();
}

public record class LastedHoliday(DateTime HeadOffice, DateTime KuwanaOffice)
{
    public DateTime Min() => new[] { HeadOffice, KuwanaOffice }.Min();
}