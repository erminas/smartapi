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
using System.Web;
using System.Xml;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.PageElements
{
    public abstract class AbstractValueElement<T> : PageElement, IValueElement
    {
        protected const string SAVE_VALUE =
            @"<ELEMENTS action=""save""><ELT guid=""{0}"" value=""{1}""></ELT></ELEMENTS>";

        protected T _value;

        protected AbstractValueElement(Project project, Guid guid)
            : base(project, guid)
        {
        }

        protected AbstractValueElement(Project project, XmlElement xmlElement) : base(project, xmlElement)
        {
            LoadXml();
        }

        public virtual T Value
        {
            get { return LazyLoad(ref _value); }
            set { _value = value; }
        }

        #region IValueElement Members

        public void SetValueFromString(string value)
        {
            _value = FromString(value);
        }

        public virtual void Commit()
        {
            XmlDocument xmlDoc =
                Project.ExecuteRQL(string.Format(SAVE_VALUE, Guid.ToRQLString(), HttpUtility.HtmlEncode(ToXmlNodeValue(_value))));
            if (xmlDoc.GetElementsByTagName("ELT").Count != 1 && !xmlDoc.InnerXml.Contains(Guid.ToRQLString()))
            {
                throw new Exception(String.Format("Could not save element {0}", Guid.ToRQLString()));
            }
        }

        #endregion
        
        private void LoadXml()
        {
            InitIfPresent(ref _value, "value", FromXmlNodeValue);
        }

        protected sealed override void LoadWholePageElement()
        {
            LoadXml();
            LoadWholeValueElement();
        }

        protected abstract void LoadWholeValueElement();

        protected abstract T FromXmlNodeValue(string arg);

        protected abstract T FromString(string value);

        protected abstract string ToXmlNodeValue(T value);
    }
}