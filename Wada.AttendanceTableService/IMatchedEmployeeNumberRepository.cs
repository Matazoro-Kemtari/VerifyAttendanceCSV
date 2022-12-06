using Wada.AttendanceTableService.MatchedEmployeeNumberAggregation;

namespace Wada.AttendanceTableService
{
    public interface IMatchedEmployeeNumberRepository
    {
        /// <summary>
        /// 個人コード検索
        /// </summary>
        /// <param name="employeeNumber"></param>
        /// <returns></returns>
        IEnumerable<MatchedEmployeeNumber> FindAll();

        /// <summary>
        /// 社員番号対応表追加
        /// </summary>
        /// <param name="matchedEmployeeNumbers"></param>
        void AddRange(IEnumerable<MatchedEmployeeNumber> matchedEmployeeNumbers);

        /// <summary>
        /// 削除
        /// </summary>
        void RemoveAll();
    }
}
