using System.Runtime.Serialization;

namespace Wada.AttendanceTableService.MatchedEmployeeNumberAggregation
{
    [Serializable]

    public class MatchedEmployeeNumberException : DomainException
    {
        public MatchedEmployeeNumberException()
        {
        }

        public MatchedEmployeeNumberException(string? message) : base(message)
        {
        }

        public MatchedEmployeeNumberException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected MatchedEmployeeNumberException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
