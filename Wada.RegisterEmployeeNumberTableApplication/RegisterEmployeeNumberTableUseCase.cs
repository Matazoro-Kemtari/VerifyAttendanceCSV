using System.Transactions;
using Wada.AOP.Logging;
using Wada.AttendanceTableService;
using Wada.Data.DesignDepartmentDataBase.Models;

namespace Wada.RegisterEmployeeNumberTableApplication;

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
    private readonly IFileStreamOpener _fileStreamOpener;
    private readonly IMatchedEmployeeNumberListReader _matchedEmployeeNumberListReader;
    private readonly IMatchedEmployeeNumberRepository _matchedEmployeeNumberRepository;

    public RegisterEmployeeNumberTableUseCase(
        IFileStreamOpener fileStreamOpener,
        IMatchedEmployeeNumberListReader matchedEmployeeNumberListReader,
        IMatchedEmployeeNumberRepository matchedEmployeeNumberRepository)
    {
        _fileStreamOpener = fileStreamOpener;
        _matchedEmployeeNumberListReader = matchedEmployeeNumberListReader;
        _matchedEmployeeNumberRepository = matchedEmployeeNumberRepository;
    }

    [Logging]
    public async Task ExecuteAsync(string csvPath)
    {
        try
        {
            // データファイルを読み込む
            var stream = await _fileStreamOpener.OpenAsync(csvPath);
            var matchedEmployeeNumbers = await _matchedEmployeeNumberListReader.ReadAllAsync(stream);

            // データベースに登録する
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            await _matchedEmployeeNumberRepository.AddRangeAsync(
                matchedEmployeeNumbers.Select(x => x.Convert()));
            scope.Complete();
        }
        catch (DomainException ex)
        {
            throw new UseCaseException(ex.Message, ex);
        }
    }
}