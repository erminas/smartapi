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
    public class ReadOnlyAttribute : IRDAttribute
    {
        private readonly IRDAttribute _attribute;

        public ReadOnlyAttribute(IRDAttribute attr)
        {
            _attribute = attr;
        }

        #region IRDAttribute Members

        public string Name
        {
            get { return _attribute.Name; }
        }

        public object DisplayObject
        {
            get { return _attribute.DisplayObject; }
        }

        public bool IsAssignableFrom(IRDAttribute o, out string reason)
        {
            reason = "attribute is read only";
            return false;
        }

        public void Assign(IRDAttribute o)
        {
            return;
        }

        public string Description
        {
            get { return _attribute.Description; }
        }

        #endregion
    }
}