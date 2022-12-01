using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wada.AttendanceTableService.OwnCompanyCalendarAggregation
{
    [Equals(DoNotAddEqualityOperators = true), ToString]
    public class OwnCompanyCalendar
    {
        public DateTime Date { get; init; }

    }
}
