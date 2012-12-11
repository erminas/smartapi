using System;

namespace erminas.SmartAPI.Exceptions
{
    [Serializable]
    public sealed class InvalidServerVersionException : SmartAPIException
    {
        public InvalidServerVersionException(string message) : base(message)
        {
            
        }
    }
}
