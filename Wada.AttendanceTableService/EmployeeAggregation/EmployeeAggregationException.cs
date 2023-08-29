using System.Runtime.Serialization;

namespace Wada.AttendanceTableService.EmployeeAggregation
{
    [Serializable]
    public class EmployeeAggregationException : DomainException
    {
        public EmployeeAggregationException()
        {
        }

        public EmployeeAggregationException(string? message) : base(message)
        {
        }

        public EmployeeAggregationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected EmployeeAggregationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
