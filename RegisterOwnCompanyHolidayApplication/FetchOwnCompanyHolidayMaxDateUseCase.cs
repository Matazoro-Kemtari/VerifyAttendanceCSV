using NLog;
using System.Reflection;
using Wada.AttendanceTableService;

namespace RegisterOwnCompanyHolidayApplication
{
    public interface IFetchOwnCompanyHolidayMaxDateUseCase
    {
        Task<DateTime> ExecuteAsyc();
    }

    public class FetchOwnCompanyHolidayMaxDateUseCase : IFetchOwnCompanyHolidayMaxDateUseCase
    {
        private readonly ILogger logger;
        private readonly IOwnCompanyHolidayRepository ownCompanyHolidayRepository;

        public FetchOwnCompanyHolidayMaxDateUseCase(ILogger logger, IOwnCompanyHolidayRepository ownCompanyHolidayRepository)
        {
            this.logger = logger;
            this.ownCompanyHolidayRepository = ownCompanyHolidayRepository;
        }

        public async Task<DateTime> ExecuteAsyc()
        {
            logger.Debug($"Start {MethodBase.GetCurrentMethod()?.Name}");

            DateTime maxDate = await Task.Run(() => ownCompanyHolidayRepository.MaxDate());

            logger.Debug($"Finish {MethodBase.GetCurrentMethod()?.Name}");

            return maxDate;
        }
    }
}
