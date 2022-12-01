using Wada.AttendanceTableService.ValueObjects;

namespace Wada.AttendanceTableService
{
    public interface IWadaHolidayRepository
    {
        HolidayClassification FindByDay(DateTime day);
    }
}
