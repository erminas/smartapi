// Smart API - .Net programatical access to RedDot servers
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
using erminas.SmartAPI.CMS;

namespace erminas.SmartAPI.Utils
{
    /// <summary>
    ///     Utility class to represent a RedDotObject through its name and guid. E.g. used for client side representation or tests.
    /// </summary>
    public class RedDotObjectHandle : IRedDotObject
    {
        private readonly Guid _guid;
        private readonly string _name;

        /// <summary>
        ///     Create a new handle with a specific Guid and name
        /// </summary>
        public RedDotObjectHandle(Guid guid, string name)
        {
            _guid = guid;
            _name = name;
        }

        /// <summary>
        ///     Create a handle and initialize its Guid/Name from an exisiting IRedDotObject.
        /// </summary>
        /// <param name="rdobject"> Object from which Guid/Name get retrieved from </param>
        public RedDotObjectHandle(IRedDotObject rdobject)
        {
            _guid = rdobject.Guid;
            _name = rdobject.Name;
        }

        #region IRedDotObject Members

        public string Name
        {
            get { return _name; }
        }

        public Guid Guid
        {
            get { return _guid; }
            set { throw new InvalidOperationException(); }
        }

        #endregion
    }
}