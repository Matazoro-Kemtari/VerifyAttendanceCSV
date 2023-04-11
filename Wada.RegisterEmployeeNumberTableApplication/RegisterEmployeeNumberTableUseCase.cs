using Wada.AOP.Logging;

namespace Wada.RegisterEmployeeNumberTableApplication
{
    public interface IRegisterEmployeeNumberTableUseCase
    {
        void Execute();
    }

    public class RegisterEmployeeNumberTableUseCase : IRegisterEmployeeNumberTableUseCase
    {
        [Logging]
        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}