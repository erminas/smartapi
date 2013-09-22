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
using erminas.SmartAPI.CMS.Converter;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public interface IArea : IWorkflowAssignments, IContentClassPreassignable
    {
        string Coords { get; set; }

        bool IsDisplayingConnectedPagesInTargetContainerOfMainLinkIfAvailable { get; set; }

        bool IsOnlyPathAndFilenameInserted { get; set; }

        bool IsSyntaxConformingToXHtml { get; set; }

        Pages.Elements.IContainer PreassignedTargetContainer { get; set; }

        Shape Shape { get; set; }

        string Supplement { get; set; }

        HtmlTarget Target { get; set; }
    }

    internal class Area : AbstractWorkflowAssignments, IArea
    {
        private readonly TargetContainerPreassignment _targetContainerPreassignment;

        internal Area(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            PreassignedContentClasses = new PreassignedContentClassesAndPageDefinitions(this);
            _targetContainerPreassignment = new TargetContainerPreassignment(this);
        }

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Structural; }
        }

        [RedDot("eltcoords")]
        public string Coords
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        public bool IsDisplayingConnectedPagesInTargetContainerOfMainLinkIfAvailable
        {
            get { return _targetContainerPreassignment.IsDisplayingConnectedPagesInTargetContainerOfMainLinkIfAvailable; }
            set { _targetContainerPreassignment.IsDisplayingConnectedPagesInTargetContainerOfMainLinkIfAvailable = value; }
        }

        [RedDot("eltonlyhrefvalue")]
        public bool IsOnlyPathAndFilenameInserted
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltxhtmlcompliant")]
        public bool IsSyntaxConformingToXHtml
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

        [RedDot("eltshape", ConverterType = typeof (StringEnumConverter<Shape>))]
        public Shape Shape
        {
            get { return GetAttributeValue<Shape>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltsupplement")]
        public string Supplement
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("elttarget", ConverterType = typeof (StringEnumConverter<HtmlTarget>))]
        public HtmlTarget Target
        {
            get { return GetAttributeValue<HtmlTarget>(); }
            set { SetAttributeValue(value); }
        }
    }
}