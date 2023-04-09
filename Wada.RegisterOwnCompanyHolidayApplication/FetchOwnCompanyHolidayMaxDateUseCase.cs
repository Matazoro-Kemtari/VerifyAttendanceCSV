using Wada.AOP.Logging;
using Wada.AttendanceTableService;

[module: Logging] // https://stackoverflow.com/questions/49648179/how-to-use-methoddecorator-fody-decorator-in-another-project
namespace Wada.RegisterOwnCompanyHolidayApplication
{
    public interface IFetchOwnCompanyHolidayMaxDateUseCase
    {
        Task<DateTime> ExecuteAsyc();
    }

    public class FetchOwnCompanyHolidayMaxDateUseCase : IFetchOwnCompanyHolidayMaxDateUseCase
    {
        private readonly IOwnCompanyHolidayRepository ownCompanyHolidayRepository;

        public FetchOwnCompanyHolidayMaxDateUseCase(IOwnCompanyHolidayRepository ownCompanyHolidayRepository)
        {
            this.ownCompanyHolidayRepository = ownCompanyHolidayRepository;
        }

        [Logging]
        public async Task<DateTime> ExecuteAsyc()
        {
            DateTime maxDate = await Task.Run(() => ownCompanyHolidayRepository.MaxDate());
            return maxDate;
        }
    }
}
