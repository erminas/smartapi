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
using System.Xml;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.CCElements
{
    public class InfoAttribute
    {
        public readonly int Id;
        public readonly string Name;
        public readonly InfoType Type;

        public InfoAttribute(XmlElement xmlElement)
        {
            Type = (InfoType) Enum.Parse(typeof (InfoType), xmlElement.Name, true);
            Id = int.Parse(xmlElement.GetAttributeValue("id"));
            Name = xmlElement.GetAttributeValue("name");
        }
    }

    public enum InfoType
    {
        PageInfo,
        ProjectInfo,
        SessionObject
    }
}