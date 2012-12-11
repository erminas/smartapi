using System;
using System.Runtime.Serialization;

namespace erminas.SmartAPI.Exceptions
{
    [Serializable]
    public class SmartAPIException : Exception
    {
        public SmartAPIException()
        {
        }

        public SmartAPIException(string message) : base(message)
        {
        }

        public SmartAPIException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public SmartAPIException(SerializationInfo info, StreamingContext context): base(info, context)
        {
        }   
    }
}
