using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NLog;
using Wada.AOP.Logging;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.MatchedEmployeeNumberAggregation;

[module: Logging] // https://stackoverflow.com/questions/49648179/how-to-use-methoddecorator-fody-decorator-in-another-project
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

        [Logging]
        public void AddRange(IEnumerable<MatchedEmployeeNumber> matchedEmployeeNumbers)
        {
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
                throw new AttendanceTableServiceException(msg, ex);
            }
            logger.Info($"データベースに{_additionalNumber}件追加しました");
        }

        [Logging]
        public IEnumerable<MatchedEmployeeNumber> FindAll()
        {
            DbConfig dbConfig = new(configuration);
            using var dbContext = new DbContext(dbConfig);

            var matchedEmployee = dbContext.MatchedEmployeeNumbers!
                .Where(x => x.EmployeeNumbers >= 0)
                .Where(x => x.AttendancePersonalCode >= 0)
                .Select(x => MatchedEmployeeNumber.ReConsttuct((uint)x.EmployeeNumbers, (uint)x.AttendancePersonalCode));

            if (!matchedEmployee.Any())
            {
                string msg = "社員番号対応表がありませんでした ";
                throw new AttendanceTableServiceException(msg);
            }

            return matchedEmployee.ToList();
        }

        [Logging]
        public void RemoveAll()
        {
            DbConfig dbConfig = new(configuration);
            using var dbContext = new DbContext(dbConfig);

            var matchedEmployee = dbContext.MatchedEmployeeNumbers!;
            if (!matchedEmployee.Any())
                return;

            dbContext.MatchedEmployeeNumbers!.RemoveRange(matchedEmployee);
            dbContext.SaveChanges();
        }
    }
}
