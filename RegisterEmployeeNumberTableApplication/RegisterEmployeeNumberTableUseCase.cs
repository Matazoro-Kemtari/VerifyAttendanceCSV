namespace RegisterEmployeeNumberTableApplication;

public interface IRegisterEmployeeNumberTableUseCase
{
    /// <summary>
    /// 社員番号対応表を登録する
    /// </summary>
    /// <param name="csvPath"></param>
    /// <returns></returns>
    Task ExecuteAsync(string csvPath);
}

public class RegisterEmployeeNumberTableUseCase : IRegisterEmployeeNumberTableUseCase
{
    public async Task ExecuteAsync(string csvPath)
    {

    }
}