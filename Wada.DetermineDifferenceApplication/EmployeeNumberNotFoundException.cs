using System.Runtime.Serialization;

namespace Wada.DetermineDifferenceApplication
{
    [Serializable]
    public class EmployeeNumberNotFoundException : UseCaseException
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