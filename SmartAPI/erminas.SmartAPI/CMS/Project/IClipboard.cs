//// Smart API - .Net programmatic access to RedDot servers
////  
//// Copyright (C) 2013 erminas GbR
//// 
//// This program is free software: you can redistribute it and/or modify it 
//// under the terms of the GNU General Public License as published by the Free Software Foundation,
//// either version 3 of the License, or (at your option) any later version.
//// This program is distributed in the hope that it will be useful,
//// but WITHOUT ANY WARRANTY; without even the implied warranty of
//// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
//// See the GNU General Public License for more details.
//// 
//// You should have received a copy of the GNU General Public License along with this program.
//// If not, see <http://www.gnu.org/licenses/>.

//using System;
//using System.Web;
//using erminas.SmartAPI.CMS.Project.Pages;
//using erminas.SmartAPI.Exceptions;
//using erminas.SmartAPI.Utils;

//namespace erminas.SmartAPI.CMS.Project
//{
//    public interface IClipboard : IProjectObject
//    {
//        void Add(IPage page);
//    }

//    internal class Clipboard : IClipboard
//    {
//        private readonly IProject _project;

//        private const string ADD_CLIPBOARD_ENTRY =
//            @"<ADMINISTRATION><USER guid=""{0}""><CLIPBOARDDATA action=""save"" value=""{1}"" projectguid=""{2}"" /></USER></ADMINISTRATION>";
//        const string TREESEGMENT_ENTRY=@"<TREESEGMENT guid=""{0}"" type=""{1}"" />";

//        public Clipboard(IProject project)
//        {
//            _project = project;
//        }


//        public void Add(IPage page)
//        {
//            var pageEntry = HttpUtility.UrlEncode(TREESEGMENT_ENTRY.RQLFormat(page, "page"));
//            var xmlDoc = Project.ExecuteRQL(ADD_CLIPBOARD_ENTRY.RQLFormat(Session.CurrentUser, pageEntry, Project));
//            if (Session.Version >= new Version(10, 0))
//            {
//                if (!xmlDoc.IsContainingOk())
//                {
//                    throw new SmartAPIException(Session.ServerLogin,
//                                                string.Format("Could not add page {0} to clipboard", page));
//                }
//            }
//            else
//            {
//                //TODO check reply for older server versions?
//            }
//        }
    

//        public ISession Session { get { return _project.Session; } }
//        public IProject Project { get { return _project; } }
//    }
//}