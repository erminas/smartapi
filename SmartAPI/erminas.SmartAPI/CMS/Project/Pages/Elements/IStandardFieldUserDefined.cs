﻿// SmartAPI - .Net programmatic access to RedDot servers
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
using System.Text.RegularExpressions;
using System.Xml;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements;

namespace erminas.SmartAPI.CMS.Project.Pages.Elements
{
    public interface IStandardFieldUserDefined : IStandardField<string>
    {
        Regex Regex { get; }
    }

    [PageElementType(ElementType.StandardFieldUserDefined)]
    internal sealed class StandardFieldUserDefined : StandardField<string>, IStandardFieldUserDefined
    {
        public static readonly Regex NUMERIC_CHECK_REGEX =
            new Regex("^\\s*(-)?((\\d+((\\.|\\,)\\d*)?)|((\\.|\\,)\\d+))([eE](-)?\\d+)?\\s*$");

        private int _maxSize;
        private Regex _regex;

        internal StandardFieldUserDefined(IProject project, XmlElement xmlElement, Regex regex)
            : base(project, xmlElement)
        {
            LoadXml();
            _regex = regex;
        }

        internal StandardFieldUserDefined(IProject project, XmlElement xmlElement) : base(project, xmlElement)
        {
            LoadXml();
        }

        public Regex Regex
        {
            get { return _regex; }
        }

        public override string Value
        {
            set
            {
                if (!string.IsNullOrEmpty(value) && !_regex.IsMatch(value))
                {
                    throw new ArgumentException("Value doesn't match regular expression");
                }
                base.Value = value;
            }
        }

        protected override string FromString(string value)
        {
            return value;
        }

        protected override void LoadWholeStandardField()
        {
            LoadXml();
        }

        private void LoadXml()
        {
            InitIfPresent(ref _maxSize, "eltmaxsize", int.Parse);
            InitIfPresent(ref _regex, "eltverifytermregexp", x => new Regex(x));
        }
    }
}