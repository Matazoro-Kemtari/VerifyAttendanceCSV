﻿using System.Runtime.Serialization;

namespace Wada.DetermineDifferenceApplication
{
    [Serializable]
    public class UseCaseException : Exception
    {
        public UseCaseException()
        {
        }

        public UseCaseException(string? message) : base(message)
        {
        }

        public UseCaseException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected UseCaseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}