using NLog;
using System.Reflection;

namespace RegisterEmployeeNumberTableApplication
{
    public interface IRegisterEmployeeNumberTableUseCase
    {
        void Execute();
    }
    public class RegisterEmployeeNumberTableUseCase : IRegisterEmployeeNumberTableUseCase
    {
        private readonly ILogger logger;

        public RegisterEmployeeNumberTableUseCase(ILogger logger)
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