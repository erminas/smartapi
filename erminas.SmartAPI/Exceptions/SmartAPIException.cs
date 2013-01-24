using System;
using System.Runtime.Serialization;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.Exceptions
{
    [Serializable]
    public class SmartAPIException : Exception
    {
        internal SmartAPIException(ServerLogin login)
        {
            if (login != null)
            {
                Server = login.Name;
            }
        }

        internal SmartAPIException(ServerLogin login, string message)
            : base(message)
        {
            if (login != null)
            {
                Server = login.Name;
            }
        }

        internal SmartAPIException(string serverName, string message)
            : base(message)
        {
            Server = serverName;
        }

        internal SmartAPIException(ServerLogin login, string message, Exception innerException)
            : base(message, innerException)
        {
            if (login != null)
            {
                Server = login.Name;
            }
        }

        internal SmartAPIException(string serverName, string message, Exception innerException)
            : base(message, innerException)
        {
            Server = serverName;
        }

        internal SmartAPIException(ServerLogin login, SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (login != null)
            {
                Server = login.Name;
            }
        }

        public string Server { get; protected set; }
    }
}