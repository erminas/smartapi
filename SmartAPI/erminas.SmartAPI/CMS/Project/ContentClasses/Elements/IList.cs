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
    public interface IList : IWorkflowAssignments, IContentClassPreassignable
    {
        [RedDot("eltfontclass")]
        string FontClass { get; set; }

        [RedDot("eltfontcolor")]
        string FontColor { get; set; }

        [RedDot("eltfontface")]
        string FontFace { get; set; }

        [RedDot("eltfontsize")]
        string FontSize { get; set; }

        bool IsDisplayingConnectedPagesInTargetContainerOfMainLinkIfAvailable { get; set; }

        [RedDot("eltfontbold")]
        bool IsFontBold { get; set; }

        [RedDot("eltonlyhrefvalue")]
        bool IsOnlyPathAndFilenameInserted { get; set; }

        [RedDot("eltxhtmlcompliant")]
        bool IsSyntaxConformingToXHtml { get; set; }

        [RedDot("eltextendedlist")]
        bool IsTransferingElementContentOfFollowingPages { get; set; }

        Pages.Elements.IContainer PreassignedTargetContainer { get; set; }
    }

    internal class List : AbstractWorkflowAssignments, IList
    {
        private readonly TargetContainerPreassignment _targetContainerPreassignment;

        internal List(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            _targetContainerPreassignment = new TargetContainerPreassignment(this);
            PreassignedContentClasses = new PreassignedContentClassesAndPageDefinitions(this);
        }

        public override sealed ContentClassCategory Category
        {
            get { return ContentClassCategory.Structural; }
        }

        public string FontClass
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        public string FontColor
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        public string FontFace
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        public string FontSize
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        public bool IsDisplayingConnectedPagesInTargetContainerOfMainLinkIfAvailable
        {
            get { return _targetContainerPreassignment.IsDisplayingConnectedPagesInTargetContainerOfMainLinkIfAvailable; }
            set { _targetContainerPreassignment.IsDisplayingConnectedPagesInTargetContainerOfMainLinkIfAvailable = value; }
        }

        public bool IsFontBold
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        public bool IsOnlyPathAndFilenameInserted
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        public bool IsSyntaxConformingToXHtml
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        public bool IsTransferingElementContentOfFollowingPages
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        public PreassignedContentClassesAndPageDefinitions PreassignedContentClasses { get; private set; }

        public Pages.Elements.IContainer PreassignedTargetContainer
        {
            get { return _targetContainerPreassignment.TargetContainer; }
            set { _targetContainerPreassignment.TargetContainer = value; }
        }
    }
}