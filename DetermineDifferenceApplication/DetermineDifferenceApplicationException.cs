using System.Runtime.Serialization;

namespace DetermineDifferenceApplication
{
    [Serializable]
    public class DetermineDifferenceApplicationException : Exception
    {
        public DetermineDifferenceApplicationException()
        {
        }

        public DetermineDifferenceApplicationException(string? message) : base(message)
        {
        }

        public DetermineDifferenceApplicationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected DetermineDifferenceApplicationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}