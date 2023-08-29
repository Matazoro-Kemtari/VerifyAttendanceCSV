using System.Runtime.Serialization;

namespace Wada.AttendanceTableService.DepartmentCompanyHolidayAggregation
{
    [Serializable]
    public class DepartmentCompanyHolidayException : DomainException
    {
        public DepartmentCompanyHolidayException()
        {
        }

        public DepartmentCompanyHolidayException(string message) : base(message)
        {
        }

        public DepartmentCompanyHolidayException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DepartmentCompanyHolidayException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
