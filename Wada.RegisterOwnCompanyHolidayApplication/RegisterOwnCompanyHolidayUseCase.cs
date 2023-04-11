using Wada.AOP.Logging;
using Wada.Data.DesignDepartmentDataBase.Models;

namespace Wada.RegisterOwnCompanyHolidayApplication
{
    public interface IRegisterOwnCompanyHolidayUseCase
    {
        Task ExecuteAsync();
    }

    public class RegisterOwnCompanyHolidayUseCase : IRegisterOwnCompanyHolidayUseCase
    {
        private readonly IOwnCompanyHolidayRepository ownCompanyHolidayRepository;

        public RegisterOwnCompanyHolidayUseCase(IOwnCompanyHolidayRepository ownCompanyHolidayRepository)
        {
            this.ownCompanyHolidayRepository = ownCompanyHolidayRepository;
        }

        [Logging]
        public Task ExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }
}