using System.Runtime.Serialization;

namespace Wada.AttendanceTableService.MatchedEmployeeNumberAggregation
{
    [Serializable]
    public class MatchedEmployeeNumberAggregationException : DomainException
    {
        public MatchedEmployeeNumberAggregationException()
        {
        }

        public MatchedEmployeeNumberAggregationException(string? message) : base(message)
        {
        }

        public MatchedEmployeeNumberAggregationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected MatchedEmployeeNumberAggregationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}