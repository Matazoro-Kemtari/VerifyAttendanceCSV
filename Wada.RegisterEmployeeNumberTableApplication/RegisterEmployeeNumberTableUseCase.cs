using System.Transactions;
using Wada.AOP.Logging;
using Wada.AttendanceTableService;
using Wada.Data.DesignDepartmentDataBase.Models;
using Wada.Data.DesignDepartmentDataBase.Models.MatchedEmployeeNumberAggregation;

namespace Wada.RegisterEmployeeNumberTableApplication;

public interface IRegisterEmployeeNumberTableUseCase
{
    /// <summary>
    /// 社員番号対応表を登録する
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    Task ExecuteAsync(string filePath);
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
    public async Task ExecuteAsync(string filePath)
    {
        try
        {
            // データファイルを読み込む
            using var stream = await _fileStreamOpener.OpenAsync(filePath);
            var additionalEmployeeNumbers = await _matchedEmployeeNumberListReader.ReadAllAsync(stream);

            // 削除するか判断するため全て取得
            var allMatchedEmployeeNumbers = await _matchedEmployeeNumberRepository.FindAllAsync();

            // 消すべきレコードを求める
            var deletable = allMatchedEmployeeNumbers.Join(
                additionalEmployeeNumbers,
                all => all.EmployeeNumber,
                add => add.EmployeeNumber,
                (all, add) => MatchedEmployeeNumber.Create(all.EmployeeNumber, all.AttendancePersonalCode));

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            if (deletable.Any())
                // 一旦消す
                await _matchedEmployeeNumberRepository.RemoveRangeAsync(deletable);

            // データベースに登録する
            await _matchedEmployeeNumberRepository.AddRangeAsync(
                additionalEmployeeNumbers);
            scope.Complete();
        }
        catch (Exception ex) when (ex is DomainException or MatchedEmployeeNumberAggregationException)
        {
            throw new UseCaseException(ex.Message, ex);
        }
    }
}