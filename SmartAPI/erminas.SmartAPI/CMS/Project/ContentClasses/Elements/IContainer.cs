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
    public interface IContainer : IWorkflowAssignments, IContentClassPreassignable, IReferencePreassignable, IReferencePreassignTarget
    {
        bool IsDynamic { get; set; }

        bool IsTargetContainer { get; set; }

        bool IsTransferingContentOfFollowingPages { get; set; }
    }

    internal class Container : AbstractWorkflowAssignments, IContainer
    {
        private readonly ReferencePreassignment _referencePreassignment;

        internal Container(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            PreassignedContentClasses = new PreassignedContentClassesAndPageDefinitions(this);
            _referencePreassignment = new ReferencePreassignment(this);
        }

        public override void Refresh()
        {
            _referencePreassignment.InvalidateCache();
            base.Refresh();
        }

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Structural; }
        }

        public IReferencePreassignTarget PreassignedReference
        {
            get { return _referencePreassignment.Target; }
            set { _referencePreassignment.Target = value; }
        }

        [RedDot("eltisdynamic")]
        public bool IsDynamic
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltistargetcontainer")]
        public bool IsTargetContainer
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltextendedlist")]
        public bool IsTransferingContentOfFollowingPages
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        public PreassignedContentClassesAndPageDefinitions PreassignedContentClasses { get; }
    }
}
