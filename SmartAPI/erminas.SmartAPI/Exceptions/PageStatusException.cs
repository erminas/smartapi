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
using erminas.SmartAPI.CMS.Project.Pages;

namespace erminas.SmartAPI.Exceptions
{
    [Serializable]
    public class PageStatusException : SmartAPIException
    {
        internal PageStatusException(IPage page) : base(page.Project.Session.ServerLogin)
        {
            Page = page;
        }

        internal PageStatusException(IPage page, string message) : base(page.Project.Session.ServerLogin, message)
        {
            Page = page;
        }

        internal PageStatusException(IPage page, string message, Exception innerException)
            : base(page.Project.Session.ServerLogin, message, innerException)
        {
            Page = page;
        }

        internal PageStatusException(IPage page, SerializationInfo info, StreamingContext context)
            : base(page.Project.Session.ServerLogin, info, context)
        {
            Page = page;
        }

        public IPage Page { get; private set; }
    }
}