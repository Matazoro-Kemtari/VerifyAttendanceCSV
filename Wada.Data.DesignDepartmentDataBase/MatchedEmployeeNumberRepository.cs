using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.MatchedEmployeeNumberAggregation;
using Wada.DataBase.EFCore.DesignDepartment;
using ILogger = NLog.ILogger;

namespace Wada.Data.DesignDepartmentDataBase;

public class MatchedEmployeeNumberRepository : IMatchedEmployeeNumberRepository
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    public MatchedEmployeeNumberRepository(IConfiguration configuration, ILogger logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task AddRangeAsync(IEnumerable<MatchedEmployeeNumber> matchedEmployeeNumbers)
    {
        using var dbContext = new DesignDepartmentContext(_configuration);

        int _additionalCount;
        try
        {
            await dbContext.MatchedEmployeeNumbers
                .AddRangeAsync(
                matchedEmployeeNumbers.Select(
                    x => new DataBase.EFCore.DesignDepartment.Entities.MatchedEmployeeNumber(
                        (int)x.EmployeeNumber,
                        (int)x.AttendancePersonalCode)));

            _additionalCount = await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            string msg = "社員番号対応表に追加できませんでした";
            throw new MatchedEmployeeNumberAggregationException(msg, ex);
        }
        _logger.Trace($"社員番号対応表に{_additionalCount}件追加しました");
    }

    public async Task<IEnumerable<MatchedEmployeeNumber>> FindAllAsync()
    {
        using var dbContext = new DesignDepartmentContext(_configuration);

        return await dbContext.MatchedEmployeeNumbers
            .Where(x => x.EmployeeNumbers >= 0)
            .Where(x => x.AttendancePersonalCode >= 0)
            .Select(x => MatchedEmployeeNumber.Reconstruct(
                (uint)x.EmployeeNumbers, (uint)x.AttendancePersonalCode))
            .ToListAsync();
    }

    public async Task RemoveRangeAsync(IEnumerable<MatchedEmployeeNumber> matchedEmployeeNumbers)
    {
        using var dbContext = new DesignDepartmentContext(_configuration);

        int remoedCount;
        try
        {
            dbContext.MatchedEmployeeNumbers.RemoveRange(
                matchedEmployeeNumbers.Select(
                    x => new DataBase.EFCore.DesignDepartment.Entities.MatchedEmployeeNumber(
                        (int)x.EmployeeNumber, (int)x.AttendancePersonalCode)));

            remoedCount = await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            string msg = "社員番号対応表から削除できませんでした";
            throw new MatchedEmployeeNumberAggregationException(msg, ex);
        }

        _logger.Trace($"社員番号対応表から{remoedCount}件削除しました");
    }
}
