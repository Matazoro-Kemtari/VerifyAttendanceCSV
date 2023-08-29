using Wada.AttendanceTableService.MatchedEmployeeNumberAggregation;

namespace Wada.AttendanceTableService;

public interface IMatchedEmployeeNumberRepository
{
    Task<IEnumerable<MatchedEmployeeNumber>> FindAllAsync();

    Task AddRangeAsync(IEnumerable<MatchedEmployeeNumber> matchedEmployeeNumbers);

    Task RemoveRangeAsync(IEnumerable<MatchedEmployeeNumber> matchedEmployeeNumbers);
}
