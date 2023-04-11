using Microsoft.Extensions.Configuration;
using System;
using Wada.AOP.Logging;
using Wada.Data.DesignDepartmentDataBase.Models;

namespace Wada.RegisterOwnCompanyHolidayApplication
{
    public interface IFetchOwnCompanyHolidayMaxDateUseCase
    {
        Task<DateTime> ExecuteAsyc();

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
        public async Task<DateTime> ExecuteAsyc()
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

            var headHolidays = await _ownCompanyHolidayRepository.FindByAfterYearAsync(headOfficeCalendarGroupId, today.Year);
            var matsuzakaHolidays = await _ownCompanyHolidayRepository.FindByAfterYearAsync(matsuzakaOfficeCalendarGroupId, today.Year);

            var maxDate = headHolidays.Union(matsuzakaHolidays).Max(x => x.HolidayDate);
            return maxDate;
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
}
