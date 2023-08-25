using Wada.AttendanceTableService;
using Wada.AttendanceTableService.MatchedEmployeeNumberAggregation;

namespace Wada.Data.DesignDepartmentDataBase;

public class MatchedEmployeeNumberRepository : IMatchedEmployeeNumberRepository
{
    public Task AddRangeAsync(IEnumerable<MatchedEmployeeNumber> matchedEmployeeNumbers)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<MatchedEmployeeNumber>> FindAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task RemoveAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task RemoveRangeAsync(IEnumerable<MatchedEmployeeNumber> matchedEmployeeNumbers)
    {
        throw new NotImplementedException();
    }
}
