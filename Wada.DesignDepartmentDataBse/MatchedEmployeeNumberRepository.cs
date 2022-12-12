using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NLog;
using System.Reflection;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.MatchedEmployeeNumberAggregation;

namespace Wada.DesignDepartmentDataBse
{
    public class MatchedEmployeeNumberRepository : IMatchedEmployeeNumberRepository
    {
        private readonly ILogger logger;
        private readonly IConfiguration configuration;

        public MatchedEmployeeNumberRepository(ILogger logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        public void AddRange(IEnumerable<MatchedEmployeeNumber> matchedEmployeeNumbers)
        {
            logger.Debug($"Start {MethodBase.GetCurrentMethod()?.Name}");

            DbConfig dbConfig = new(configuration);
            using var dbContext = new DbContext(dbConfig);

            dbContext.MatchedEmployeeNumbers!
                .AddRange(
                matchedEmployeeNumbers.Select(x => new Models.MatchedEmployeeNumber(
                    (int)x.EmployeeNumber,
                    (int)x.AttendancePersonalCode)));

            int _additionalNumber;
            try
            {
                _additionalNumber = dbContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                string msg = "登録済みの社員番号対応表は追加・上書きできません";
                logger.Error(ex, msg);
                throw new AttendanceTableServiceException(msg, ex);
            }
            logger.Info($"データベースに{_additionalNumber}件追加しました");

            logger.Debug($"Finish {MethodBase.GetCurrentMethod()?.Name}");
        }

        public IEnumerable<MatchedEmployeeNumber> FindAll()
        {
            logger.Debug($"Start {MethodBase.GetCurrentMethod()?.Name}");

            DbConfig dbConfig = new(configuration);
            using var dbContext = new DbContext(dbConfig);

            var matchedEmployee = dbContext.MatchedEmployeeNumbers!
                .Where(x => x.EmployeeNumbers >= 0)
                .Where(x => x.AttendancePersonalCode >= 0)
                .Select(x => MatchedEmployeeNumber.ReConsttuct((uint)x.EmployeeNumbers, (uint)x.AttendancePersonalCode));

            if (!matchedEmployee.Any())
            {
                string msg = "社員番号対応表がありませんでした ";
                logger.Error(msg);
                throw new AttendanceTableServiceException(msg);
            }

            logger.Debug($"Finish {MethodBase.GetCurrentMethod()?.Name}");

            return matchedEmployee.ToList();
        }

        public void RemoveAll()
        {
            logger.Debug($"Start {MethodBase.GetCurrentMethod()?.Name}");

            DbConfig dbConfig = new(configuration);
            using var dbContext = new DbContext(dbConfig);

            var matchedEmployee = dbContext.MatchedEmployeeNumbers!;
            if (!matchedEmployee.Any())
                return;

            dbContext.MatchedEmployeeNumbers!.RemoveRange(matchedEmployee);
            dbContext.SaveChanges();

            logger.Debug($"Finish {MethodBase.GetCurrentMethod()?.Name}");
        }
    }
}
