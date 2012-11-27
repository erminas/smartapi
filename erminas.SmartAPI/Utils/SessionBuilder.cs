/*
 * Smart API - .Net programatical access to RedDot servers
 * Copyright (C) 2012  erminas GbR 
 *
 * This program is free software: you can redistribute it and/or modify it 
 * under the terms of the GNU General Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details. 
 *
 * You should have received a copy of the GNU General Public License along with this program.
 * If not, see <http://www.gnu.org/licenses/>. 
 */

using System;
using erminas.SmartAPI.CMS;

namespace erminas.SmartAPI.Utils
{
    /// <summary>
    ///   Utility class to create <see cref="Session" /> objects with given login data/login guid/session key and project guid. Useful for RedDot plugins.
    /// </summary>
    public class SessionBuilder
    {
        private readonly ServerLogin _login;
        private readonly Guid _loginGuid;
        private readonly Guid _projectGuid;
        private readonly Guid _sessionKey;

        public SessionBuilder(ServerLogin login, Guid loginGuid, Guid sessionKey, Guid projectGuid)
        {
            _login = login;
            _loginGuid = loginGuid;
            _sessionKey = sessionKey;
            _projectGuid = projectGuid;
        }

        /// <summary>
        ///   Create a new session initialized with the login guid, session key and project guid of this SessionBuilder.
        /// </summary>
        /// <returns> </returns>
        public Session CreateSession()
        {
            return new Session(_login, _loginGuid, _sessionKey, _projectGuid);
        }
    }
}