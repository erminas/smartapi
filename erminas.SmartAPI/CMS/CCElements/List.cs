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

using System.Xml;
using erminas.SmartAPI.CMS.CCElements.Attributes;

namespace erminas.SmartAPI.CMS.CCElements
{
    public class List : AbstractWorkflowPreassignable, IContentClassPreassignable
    {
        private readonly TargetContainerPreassignment _targetContainerPreassignment;

        internal List(ContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("eltextendedlist", "eltfontclass", "eltfontsize", "eltfontbold", "eltonlyhrefvalue",
                             "eltxhtmlcompliant", "eltfontface", "eltfontcolor");
            _targetContainerPreassignment = new TargetContainerPreassignment(this);
            PreassignedContentClasses = new PreassignedContentClassesAndPageDefinitions(this);
        }

        public override sealed ContentClassCategory Category
        {
            get { return ContentClassCategory.Structural; }
        }

        public string FontClass
        {
            get { return GetAttributeValue<string>("eltfontclass"); }
            set
            {
                SetAttributeValue("eltfontclass", value);
            }
        }

        public string FontColor
        {
            get { return GetAttributeValue<string>("eltfontcolor"); }
            set
            {
                SetAttributeValue("eltfontcolor", value);
            }
        }

        public string FontFace
        {
            get { return GetAttributeValue<string>("eltfontface"); }
            set
            {
                SetAttributeValue("eltfontface", value);
            }
        }

        public string FontSize
        {
            get { return GetAttributeValue<string>("eltfontsize"); }
            set
            {
                SetAttributeValue("eltfontsize", value);
            }
        }

        public bool IsDisplayingConnectedPagesInTargetContainerOfMainLinkIfAvailable
        {
            get { return _targetContainerPreassignment.IsDisplayingConnectedPagesInTargetContainerOfMainLinkIfAvailable; }
            set { _targetContainerPreassignment.IsDisplayingConnectedPagesInTargetContainerOfMainLinkIfAvailable = value; }
        }

        public bool IsFontBold
        {
            get { return GetAttributeValue<bool>("eltfontbold"); }
            set { SetAttributeValue("eltfontbold", value); }
        }

        public bool IsOnlyPathAndFilenameInserted
        {
            get { return GetAttributeValue<bool>("eltonlyhrefvalue"); }
            set { SetAttributeValue("eltonlyhrefvalue", value); }
        }

        public bool IsSyntaxConformingToXHtml
        {
            get { return GetAttributeValue<bool>("eltxhtmlcompliant"); }
            set { SetAttributeValue("eltxhtmlcompliant", value); }
        }

        public PreassignedContentClassesAndPageDefinitions PreassignedContentClasses { get; private set; }

        public PageElements.Container PreassignedTargetContainer
        {
            get { return _targetContainerPreassignment.TargetContainer; }
            set { _targetContainerPreassignment.TargetContainer = value; }
        }
    }
}