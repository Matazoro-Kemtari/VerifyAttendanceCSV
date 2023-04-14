using Wada.Data.DesignDepartmentDataBase.Models.MatchedEmployeeNumberAggregation;

namespace Wada.AttendanceTableService;

public interface IMatchedEmployeeNumberListReader
{
    /// <summary>
    /// 社員番号対応表を読み込む
    /// </summary>
    /// <param name="streamReader"></param>
    /// <returns></returns>
    Task<IEnumerable<MatchedEmployeeNumber>> ReadAllAsync(Stream stream);
}
