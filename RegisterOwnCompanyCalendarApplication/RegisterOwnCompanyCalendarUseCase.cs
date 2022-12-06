using NLog;
using System.Reflection;

namespace RegisterOwnCompanyCalendarApplication
{
    public interface IRegisterOwnCompanyCalendarUseCase
    {
        void Execute();
    }
    public class RegisterOwnCompanyCalendarUseCase : IRegisterOwnCompanyCalendarUseCase
    {
        private readonly ILogger logger;

        public RegisterOwnCompanyCalendarUseCase(ILogger logger)
        {
            this.logger = logger;
        }

        public void Execute()
        {
            logger.Debug($"Start {MethodBase.GetCurrentMethod()?.Name}");
            logger.Debug($"Finish {MethodBase.GetCurrentMethod()?.Name}");
            throw new NotImplementedException();
        }
    }
}