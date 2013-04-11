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
using System.Globalization;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes
{
    internal class TextContentAttribute : RDXmlNodeAttribute
    {
        public enum TextType
        {
            Default = 3,
            Sample = 10
        }

        private readonly TextType _type;

        private Guid _guid;
        private bool _hasChanged;
        private string _text;

        public TextContentAttribute(IContentClassElement parent, TextType type, string name) : base(parent, name)
        {
            _type = type;
        }

        public override void Assign(IRDAttribute o)
        {
            var attr = (TextContentAttribute) o;
            Text = attr.Text;
        }

        public void Commit()
        {
            if (!_hasChanged)
            {
                return;
            }
            var parent = ((ContentClassElement) Parent);
            ILanguageVariant lang = parent.LanguageVariant;

            if (string.IsNullOrEmpty(_text))
            {
                SetValue(null);
                _text = null;
                return;
            }
            try
            {
                SetValue(
                    parent.ContentClass.Project.SetTextContent(_guid, lang,
                                                               ((int) _type).ToString(CultureInfo.InvariantCulture),
                                                               _text).ToRQLString());
            } catch (Exception e)
            {
                throw new SmartAPIException(parent.Session.ServerLogin,
                                            string.Format("Could not set {0} text for {1}", _type.ToString().ToLower(),
                                                          parent), e);
            }
            _hasChanged = false;
        }

        public override object DisplayObject
        {
            get { return Text; }
        }

        public override bool Equals(object o)
        {
            var attr = o as TextContentAttribute;
            return attr != null && attr._type == _type && attr.Text == Text;
        }

        public override int GetHashCode()
        {
            return _type.GetHashCode() + 23*base.GetHashCode();
        }

        public override bool IsAssignableFrom(IRDAttribute o, out string reason)
        {
            var attr = o as TextContentAttribute;
            reason = "";
            return attr != null && attr._type == _type;
        }

        public string Text
        {
            get
            {
                if (_text == null)
                {
                    var parent = ((ContentClassElement) Parent);
                    ILanguageVariant lang = parent.LanguageVariant;
                    _text = _guid == Guid.Empty
                                ? string.Empty
                                : parent.ContentClass.Project.GetTextContent(_guid, lang,
                                                                             ((int) _type).ToString(
                                                                                 CultureInfo.InvariantCulture));
                }
                return _text;
            }
            set
            {
                _hasChanged = value != Text;
                _text = value;
            }
        }

        protected override void UpdateValue(string value)
        {
            _guid = string.IsNullOrEmpty(value) || value == "#" + Parent.Session.SessionKey
                        ? new Guid()
                        : new Guid(value);
        }
    }
}