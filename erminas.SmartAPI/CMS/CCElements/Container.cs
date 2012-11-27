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

using System.Xml;
using erminas.SmartAPI.CMS.CCElements.Attributes;

namespace erminas.SmartAPI.CMS.CCElements
{
    public class Container : CCElement
    {
        public Container(ContentClass contentClass, XmlNode xmlNode) : base(contentClass, xmlNode)
        {
            CreateAttributes("eltistargetcontainer", "eltisdynamic");
        }

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Structural; }
        }

        public bool IsDynamic
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltisdynamic")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltisdynamic")).Value = value; }
        }

        public bool IsTargetContainer
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltistargetcontainer")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltistargetcontainer")).Value = value; }
        }
    }
}