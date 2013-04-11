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

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public interface IArea : IWorkflowAssignments, IContentClassPreassignable
    {
        string Coords { get; set; }
        bool IsDisplayingConnectedPagesInTargetContainerOfMainLinkIfAvailable { get; set; }
        bool IsOnlyPathAndFilenameInserted { get; set; }
        bool IsSyntaxConformingToXHtml { get; set; }
        Pages.Elements.IContainer PreassignedTargetContainer { get; set; }
        string Shape { get; set; }
        string Supplement { get; set; }
        string Target { get; set; }
    }

    internal class Area : AbstractWorkflowAssignments, IArea
    {
        private readonly TargetContainerPreassignment _targetContainerPreassignment;

        internal Area(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("eltxhtmlcompliant", "eltsupplement", "eltonlyhrefvalue", "eltshape", "elttarget",
                             "eltcoords");
            PreassignedContentClasses = new PreassignedContentClassesAndPageDefinitions(this);
            _targetContainerPreassignment = new TargetContainerPreassignment(this);
        }

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Structural; }
        }

        public string Coords
        {
            get { return GetAttributeValue<string>("eltcoords"); }
            set { SetAttributeValue("eltcoords", value); }
        }

        public bool IsDisplayingConnectedPagesInTargetContainerOfMainLinkIfAvailable
        {
            get { return _targetContainerPreassignment.IsDisplayingConnectedPagesInTargetContainerOfMainLinkIfAvailable; }
            set { _targetContainerPreassignment.IsDisplayingConnectedPagesInTargetContainerOfMainLinkIfAvailable = value; }
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

        public Pages.Elements.IContainer PreassignedTargetContainer
        {
            get { return _targetContainerPreassignment.TargetContainer; }
            set { _targetContainerPreassignment.TargetContainer = value; }
        }

        //TODO use enum instead
        public string Shape
        {
            get { return GetAttributeValue<string>("eltshape"); }
            set { SetAttributeValue("eltshape", value); }
        }

        public string Supplement
        {
            get { return GetAttributeValue<string>("eltsupplement"); }
            set { SetAttributeValue("eltsupplement", value); }
        }

        //TODO use enum instead
        public string Target
        {
            get { return GetAttributeValue<string>("elttarget"); }
            set { SetAttributeValue("elttarget", value); }
        }
    }
}