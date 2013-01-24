using System;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.Exceptions
{
    [Serializable]
    public sealed class InvalidServerVersionException : SmartAPIException
    {
        internal InvalidServerVersionException(ServerLogin server, string message) : base(server, message)
        {
        }
    }
}