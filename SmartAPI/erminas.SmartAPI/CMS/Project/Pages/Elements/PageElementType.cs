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
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements;
using Attribute = System.Attribute;

namespace erminas.SmartAPI.CMS.Project.Pages.Elements
{
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class PageElementType : Attribute
    {
        public PageElementType(ElementType type)
        {
            Type = type;
        }

        public ElementType Type { get; set; }
    }
}