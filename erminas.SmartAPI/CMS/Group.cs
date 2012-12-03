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

using System.Collections.Generic;
using System.Xml;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    /// <summary>
    ///   Group data: <pre>
    ///                 <GROUP action="load" guid="[!guid_group!]" name="group_name" email="name@company.com" />
    ///               </pre>
    /// </summary>
    public class Group : RedDotObject
    {
        public Group()
        {
            Users = null;
        }

        /// <summary>
        ///   Reads group data from XML-Element "GROUP" like: <pre>
        ///                                                     <GROUP action="load" guid="[!guid_group!]" name="group_name"
        ///                                                       email="name@company.com" />
        ///                                                   </pre>
        /// </summary>
        /// <exception cref="RedDotDataException">Thrown if element doesn't contain valid data.</exception>
        /// <param name="xmlElement"> </param>
        public Group(XmlElement xmlElement)
            : base(xmlElement)
        {
            LoadXml();
        }

        /// <summary>
        ///   All users within a group. Set to "null" if not loaded yet. If there is no user in this group, Users is set to an empty list.
        /// </summary>
        public List<User> Users { get; set; }

        public string Email { get; set; }


        private void LoadXml()
        {
            Email = XmlNode.GetAttributeValue("email");
        }
    }
}