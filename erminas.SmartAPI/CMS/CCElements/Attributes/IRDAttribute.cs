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

namespace erminas.SmartAPI.CMS.CCElements.Attributes
{
    /// <summary>
    ///   Interface for attribute objects.
    /// </summary>
    public interface IRDAttribute
    {
        /// <summary>
        ///   Name of the attribute, usually the name of an XML attribute
        /// </summary>
        string Name { get; }

        object DisplayObject { get; }
        string Description { get; }
        bool IsAssignableFrom(IRDAttribute o, out string reason);
        void Assign(IRDAttribute o);
        bool Equals(object o);
        void Refresh();
    }
}