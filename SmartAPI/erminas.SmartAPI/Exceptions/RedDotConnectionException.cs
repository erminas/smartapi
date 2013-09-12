// SmartAPI - .Net programmatic access to RedDot servers
//  
// Copyright (C) 2013 erminas GbR
// 
// This program is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with this program.
// If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Runtime.Serialization;

namespace erminas.SmartAPI.Exceptions
{
    [Serializable]
    public class RedDotConnectionException : ApplicationException
    {
        #region FailureTypes enum

        public enum FailureTypes
        {
            Unknown = 0,
            ServerNotFound = 10,
            CouldNotLogin = 20,
            AlreadyLoggedIn = 21,
        }

        #endregion

        public RedDotConnectionException() : this(FailureTypes.Unknown)
        {
        }

        public RedDotConnectionException(string message) : this(FailureTypes.Unknown, message)
        {
        }

        public RedDotConnectionException(string message, Exception inner) : this(FailureTypes.Unknown, message, inner)
        {
        }

        public RedDotConnectionException(FailureTypes ft)
        {
            FailureType = ft;
        }

        public RedDotConnectionException(FailureTypes ft, string message) : base(message)
        {
            FailureType = ft;
        }

        public RedDotConnectionException(FailureTypes ft, string message, Exception inner) : base(message, inner)
        {
            FailureType = ft;
        }

        // This constructor is needed for serialization.
// ReSharper disable UnusedParameter.Local
        protected RedDotConnectionException(SerializationInfo info, StreamingContext context)
            // ReSharper restore UnusedParameter.Local
        {
            // Add implementation.
        }

        public FailureTypes FailureType { get; private set; }
    }
}