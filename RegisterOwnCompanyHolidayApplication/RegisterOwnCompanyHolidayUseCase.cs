using NLog;
using System.Reflection;
using Wada.AttendanceTableService;

namespace RegisterOwnCompanyHolidayApplication
{
    public interface IRegisterOwnCompanyHolidayUseCase
    {
        void Execute();
    }
    public class RegisterOwnCompanyHolidayUseCase : IRegisterOwnCompanyHolidayUseCase
    {
        private readonly ILogger logger;
        private readonly IOwnCompanyHolidayRepository ownCompanyHolidayRepository;

        public RegisterOwnCompanyHolidayUseCase(ILogger logger, IOwnCompanyHolidayRepository ownCompanyHolidayRepository)
        {
            this.logger = logger;
            this.ownCompanyHolidayRepository = ownCompanyHolidayRepository;
        }

        public void Execute()
        {
            logger.Debug($"Start {MethodBase.GetCurrentMethod()?.Name}");
            logger.Debug($"Finish {MethodBase.GetCurrentMethod()?.Name}");
            throw new NotImplementedException();
        }
    }
}