using Wada.Data.DesignDepartmentDataBase.Models;

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
    private readonly IMatchedEmployeeNumberRepository _matchedEmployeeNumberRepository;

    public RegisterEmployeeNumberTableUseCase(IMatchedEmployeeNumberRepository matchedEmployeeNumberRepository)
    {
        _matchedEmployeeNumberRepository = matchedEmployeeNumberRepository;
    }

    public async Task ExecuteAsync(string csvPath)
    {
        throw new NotImplementedException();
        //return await _matchedEmployeeNumberRepository.AddRangeAsync();
    }
}