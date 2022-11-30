using System.Runtime.Serialization;

namespace Wada.AttendanceTableService
{
    [Serializable]
    public class AttendanceTableServiceException : Exception
    {
        public AttendanceTableServiceException()
        {
        }

        public AttendanceTableServiceException(string? message) : base(message)
        {
        }

        public AttendanceTableServiceException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected AttendanceTableServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
