using Wada.AOP.Logging;
using Wada.AttendanceTableService;

namespace Wada.RegisterOwnCompanyHolidayApplication
{
    public interface IRegisterOwnCompanyHolidayUseCase
    {
        void Execute();
    }

    public class RegisterOwnCompanyHolidayUseCase : IRegisterOwnCompanyHolidayUseCase
    {
        private readonly IOwnCompanyHolidayRepository ownCompanyHolidayRepository;

        public RegisterOwnCompanyHolidayUseCase(IOwnCompanyHolidayRepository ownCompanyHolidayRepository)
        {
            this.ownCompanyHolidayRepository = ownCompanyHolidayRepository;
        }

        [Logging]
        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}