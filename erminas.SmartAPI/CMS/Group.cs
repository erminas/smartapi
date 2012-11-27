﻿/*
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
using System.Collections.Generic;
using System.Xml;
using erminas.SmartAPI.Exceptions;

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

        /* Element specific data */
        /* END: Element specific data */


        /// <summary>
        ///   Reads group data from XML-Element "GROUP" like: <pre>
        ///                                                     <GROUP action="load" guid="[!guid_group!]" name="group_name"
        ///                                                       email="name@company.com" />
        ///                                                   </pre>
        /// </summary>
        /// <exception cref="RedDotDataException">Thrown if element doesn't contain valid data.</exception>
        /// <param name="xmlNode"> GROUP XML-Element to get data from </param>
        public Group(XmlNode xmlNode) : base(xmlNode)
        {
            LoadXml(xmlNode);
        }

        /// <summary>
        ///   All users within a group. Set to "null" if not loaded yet. If there is no user in this group, Users is set to an empty list.
        /// </summary>
        public List<User> Users { get; set; }

        public string Email { get; set; }


        protected override void LoadXml(XmlNode node)
        {
            XmlAttributeCollection attr = node.Attributes;
            if (attr != null)
            {
                try
                {
                    Guid = Guid.Parse(attr["guid"].Value);
                    Name = attr["name"].Value;
                    Email = attr["email"].Value;
                }
                catch (Exception e)
                {
                    // couldn't read data
                    throw new RedDotDataException("Couldn't read group data.", e);
                }
            }
        }
    }
}