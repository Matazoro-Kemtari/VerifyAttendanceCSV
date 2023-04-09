using Wada.AOP.Logging;

[module: Logging] // https://stackoverflow.com/questions/49648179/how-to-use-methoddecorator-fody-decorator-in-another-project
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