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

using System;
using System.Xml;
using erminas.SmartAPI.CMS.Project.Pages.Elements;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.Pages
{
    internal class LinkingAndAppearance : ILinkingAndAppearance
    {
        internal LinkingAndAppearance(IPage page, XmlElement element)
        {
            Page = page;
            LoadXml(element);
        }

        public DateTime AppearenceEnd { get; private set; }
        public DateTime AppearenceStart { get; private set; }

        public Guid Guid
        {
            get { return Link.Guid; }
        }

        public bool HasAppearenceEnd
        {
            get { return AppearenceEnd != DateTime.MaxValue; }
        }

        public bool HasAppearenceStart
        {
            get { return AppearenceStart != DateTime.MinValue; }
        }

        public bool IsActive { get; private set; }
        public ILinkElement Link { get; private set; }

        public string Name
        {
            get { return Link.Name; }
        }

        public IPage Page { get; private set; }

        public IProject Project
        {
            get { return Page.Project; }
        }

        public ISession Session
        {
            get { return Page.Session; }
        }

        private void LoadXml(XmlElement element)
        {
            Link = (ILinkElement) PageElement.CreateElement(Project, element);

            var start = element.GetOADate("startdate");
            AppearenceStart = !start.HasValue ? DateTime.MinValue : start.Value;

            var end = element.GetOADate("enddate");
            AppearenceEnd = !end.HasValue ? DateTime.MaxValue : end.Value;

            var dateState = element.GetIntAttributeValue("datestate").GetValueOrDefault();
            IsActive = dateState == 1 || dateState == 3;
        }
    }

    public interface ILinkingAndAppearance : IProjectObject, IRedDotObject
    {
        DateTime AppearenceEnd { get; }
        DateTime AppearenceStart { get; }

        bool HasAppearenceEnd { get; }
        bool HasAppearenceStart { get; }

        bool IsActive { get; }
        ILinkElement Link { get; }
        IPage Page { get; }
    }
}