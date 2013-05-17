// Smart API - .Net programmatic access to RedDot servers
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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using erminas.SmartAPI.CMS;

namespace erminas.SmartAPI.Utils
{
    /// <summary>
    ///     Utility class to create <see cref="Session" /> objects with given login data/login guid/session key and project guid. Useful for RedDot plugins.
    /// </summary>
    public class SessionBuilder
    {
        public SessionBuilder(ServerLogin login, Guid loginGuid, string sessionKey, Guid projectGuid)
        {
            Login = login;
            LoginGuid = loginGuid;
            SessionKey = sessionKey;
            ProjectGuid = projectGuid;
        }

        public SessionBuilder()
        {
        }

        /// <summary>
        ///     Create a new session initialized with the login guid, session key and project guid of this SessionBuilder.
        /// </summary>
        /// <returns> </returns>
        public ISession CreateSession()
        {
            return new Session(Login, LoginGuid, SessionKey, ProjectGuid);
        }

        public static ISession CreateSession(ServerLogin login)
        {
            return new Session(login);
        }

        public static ISession CreateOrReplaceSession(Func<IEnumerable<RunningSessionInfo>, RunningSessionInfo>  sessionReplacementSelector)
        {
            return null;
        }

        public static ISession CreateOrReplaceOldestSession()
        {
            Func<IEnumerable<RunningSessionInfo>, RunningSessionInfo> sessionReplacementSelector =
                infos => infos.OrderBy(info => info.LoginDate).First();
            return CreateOrReplaceSession(sessionReplacementSelector);
        }

        public ServerLogin Login { get; set; }
        public Guid LoginGuid { get; set; }
        public Guid ProjectGuid { get; set; }
        public string SessionKey { get; set; }
    }
}