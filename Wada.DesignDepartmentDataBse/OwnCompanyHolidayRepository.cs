using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NLog;
using Wada.AOP.Logging;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.OwnCompanyCalendarAggregation;
using Wada.AttendanceTableService.ValueObjects;

namespace Wada.DesignDepartmentDataBse
{
    public class OwnCompanyHolidayRepository : IOwnCompanyHolidayRepository
    {
        private readonly ILogger logger;
        private readonly IConfiguration configuration;

        public OwnCompanyHolidayRepository(ILogger logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        [Logging]
        public void AddRange(IEnumerable<OwnCompanyHoliday> ownCompanyHolidays)
        {
            DbConfig dbConfig = new(configuration);
            using var dbContext = new DbContext(dbConfig);

            dbContext.OwnCompanyHolidays!
                .AddRange(
                ownCompanyHolidays.Select(x => new Models.OwnCompanyHoliday(
                    x.HolidayDate,
                    x.HolidayClassification == HolidayClassification.LegalHoliday)));

            int _additionalNumber;
            try
            {
                _additionalNumber = dbContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                string msg = "登録済みの日付の自社カレンダーは追加・上書きできません";
                throw new AttendanceTableServiceException(msg, ex);
            }
            logger.Info($"データベースに{_additionalNumber}件追加しました");
        }

        [Logging]
        public IEnumerable<OwnCompanyHoliday> FindByYearMonth(int year, int month)
        {
            DbConfig dbConfig = new(configuration);
            using var dbContext = new DbContext(dbConfig);

            var ownHoliday = dbContext.OwnCompanyHolidays!
                .Where(x => x.HolidayDate >= new DateTime(year, month, 1))
                .Where(x => x.HolidayDate < new DateTime(year, month, 1).AddMonths(1))
                .Select(x => OwnCompanyHoliday.ReConstruct(
                    x.HolidayDate,
                    x.LegalHoliday ? HolidayClassification.LegalHoliday : HolidayClassification.RegularHoliday));

            if (!ownHoliday.Any())
            {
                string msg = $"自社カレンダーに該当がありませんでした "
                             + $"対象年月: {year}年{month}月";
                throw new AttendanceTableServiceException(msg);
            }

            return ownHoliday.ToList();
        }

        [Logging]
        public DateTime MaxDate()
        {
            DbConfig dbConfig = new(configuration);
            using var dbContext = new DbContext(dbConfig);

            var maxHoliday = dbContext.OwnCompanyHolidays!
                .Max(x => x.HolidayDate);

            return maxHoliday;
        }
    }
}
