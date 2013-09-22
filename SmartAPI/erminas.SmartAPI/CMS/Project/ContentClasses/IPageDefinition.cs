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
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.ContentClasses
{
    public interface IPageDefinition : IRedDotObject, IProjectObject, IDeletable
    {
        IContentClass ContentClass { get; }
        string Description { get; }
    }

    internal class PageDefinition : RedDotProjectObject, IPageDefinition
    {
        private readonly IContentClass _contentClass;
        private string _description;

        internal PageDefinition(IContentClass contentClass, XmlElement element) : base(contentClass.Project, element)
        {
            _contentClass = contentClass;
            LoadXml();
        }

        public IContentClass ContentClass
        {
            get { return _contentClass; }
        }

        public void Delete()
        {
            const string DELETE =
                @"<TEMPLATE guid=""{0}""><PAGEDEFINITION action=""delete"" guid=""{1}"" name=""{2}""/></TEMPLATE>";

            Project.ExecuteRQL(DELETE.RQLFormat(_contentClass, this, Name));
        }

        public string Description
        {
            get { return _description; }
        }

        private void LoadXml()
        {
            InitIfPresent(ref _description, "description", x => x);
        }
    }
}