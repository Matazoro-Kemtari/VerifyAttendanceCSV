using System.Runtime.Serialization;

namespace DetermineDifferenceApplication
{
    [Serializable]
    public class EmployeeNumberNotFoundException : Exception
    {
        public EmployeeNumberNotFoundException()
        {
        }

        public EmployeeNumberNotFoundException(string? message) : base(message)
        {
        }

        public EmployeeNumberNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected EmployeeNumberNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}