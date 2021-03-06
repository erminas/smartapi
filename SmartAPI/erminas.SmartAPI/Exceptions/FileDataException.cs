﻿// SmartAPI - .Net programmatic access to RedDot servers
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
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.Exceptions
{
    [Serializable]
    public class FileDataException : SmartAPIException
    {
        internal FileDataException(ServerLogin login) : base(login)
        {
            // Add implementation.
        }

        internal FileDataException(ServerLogin login, string message) : base(login, message)
        {
            // Add implementation.
        }

        internal FileDataException(ServerLogin login, string message, Exception inner) : base(login, message, inner)
        {
            // Add implementation.
        }

        // This constructor is needed for serialization.
        protected FileDataException(ServerLogin login, SerializationInfo info, StreamingContext context)
            : base(login, info, context)
        {
            // Add implementation.
        }
    }
}