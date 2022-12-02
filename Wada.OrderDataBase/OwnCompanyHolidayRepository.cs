using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wada.AttendanceTableService;
using Wada.AttendanceTableService.OwnCompanyCalendarAggregation;
using Wada.AttendanceTableService.ValueObjects;

namespace Wada.OrderDataBase
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

        public IEnumerable<OwnCompanyHoliday> FindByYearMonth(int year, int month)
        {
            logger.Debug($"Start {MethodBase.GetCurrentMethod()?.Name}");

            OrderDbConfig dbConfig = new(configuration);
            using var dbContext = new OrderDbContext(dbConfig);

            var ownHoliday = dbContext.OwnCompanyHolidays!
                .Where(x => x.HolidayDate >= new DateTime(year, month, 1))
                .Where(x => x.HolidayDate < new DateTime(year, month, 1).AddMonths(1))
                .Select(x => new OwnCompanyHoliday(
                    x.HolidayDate,
                    x.LegalHoliday ? HolidayClassification.LegalHoliday : HolidayClassification.RegularHoliday));

            if (!ownHoliday.Any())
            {
                string msg = $"自社カレンダーに該当がありませんでした "
                             + $"対象年月: {year}年{month}月";
                logger.Error(msg);
                throw new AttendanceTableServiceException(msg);
            }

            logger.Debug($"Finish {MethodBase.GetCurrentMethod()?.Name}");

            return ownHoliday.ToList();
        }

        public DateTime MaxDate()
        {
            logger.Debug($"Start {MethodBase.GetCurrentMethod()?.Name}");

            OrderDbConfig dbConfig = new(configuration);
            using var dbContext = new OrderDbContext(dbConfig);

            var maxHoliday = dbContext.OwnCompanyHolidays!
                .Max(x => x.HolidayDate);

            logger.Debug($"Finish {MethodBase.GetCurrentMethod()?.Name}");

            return maxHoliday;
        }
    }
}
