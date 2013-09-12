// SmartAPI - .Net programmatic access to RedDot servers
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

using System.Xml;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public interface IHeadline : IContentClassElement
    {
        [RedDot("eltrddescription")]
        string Description { get; set; }

        [RedDot("eltdirectedit")]
        bool IsDirectEditActivated { get; set; }

        [RedDot("eltdragdrop")]
        bool IsDragAndDropActivated { get; set; }

        [VersionIsLessThan(9, 0, 0, 41, VersionName = "Version 9 Hotfix 5")]
        [RedDot("eltlanguageindependent")]
        bool IsLanguageIndependent { get; set; }

        [RedDot("eltdonothtmlencode")]
        bool IsNotConvertingCharactersToHtml { get; set; }

        [VersionIsLessThan(9, 0, 0, 41, VersionName = "Version 9 Hotfix 5")]
        [RedDot("eltignoreworkflow")]
        bool IsNotRelevantForWorklow { get; set; }

        [RedDot("elthideinform")]
        bool IsNotUsedInForm { get; set; }
    }

    internal class Headline : ContentClassElement, IHeadline
    {
        internal Headline(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
        }

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Content; }
        }

        public string Description
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        public bool IsDirectEditActivated
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        public bool IsDragAndDropActivated
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [VersionIsLessThan(9, 0, 0, 41, VersionName = "Version 9 Hotfix 5")]
        public bool IsLanguageIndependent
        {
            get
            {
                VersionVerifier.EnsureVersion(ContentClass.Project.Session);
                return GetAttributeValue<bool>();
            }
            set
            {
                VersionVerifier.EnsureVersion(ContentClass.Project.Session);
                SetAttributeValue(value);
            }
        }

        public bool IsNotConvertingCharactersToHtml
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [VersionIsLessThan(9, 0, 0, 41, VersionName = "Version 9 Hotfix 5")]
        public bool IsNotRelevantForWorklow
        {
            get
            {
                VersionVerifier.EnsureVersion(ContentClass.Project.Session);
                return GetAttributeValue<bool>();
            }
            set
            {
                VersionVerifier.EnsureVersion(ContentClass.Project.Session);
                SetAttributeValue(value);
            }
        }

        public bool IsNotUsedInForm
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }
    }
}