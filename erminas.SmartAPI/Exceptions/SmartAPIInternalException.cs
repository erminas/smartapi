using System;
using System.Runtime.Serialization;

namespace erminas.SmartAPI.Exceptions
{
    [Serializable]
    public class SmartAPIInternalException : Exception
    {
         public SmartAPIInternalException()
        {
        }

        public SmartAPIInternalException(string message) : base(message)
        {
        }

        public SmartAPIInternalException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public SmartAPIInternalException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }   
    }
}
