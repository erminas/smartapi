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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using erminas.SmartAPI.CMS.Administration;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Folder
{
    public interface IAssetManagerFiles : IFiles
    {
        new IAssetManagerFolder Folder { get; }

        /// <summary>
        ///     Returns List of files that match a predicate on an attribute
        /// </summary>
        /// <param name="attribute"> Attribute which values get checked in the predicate </param>
        /// <param name="operator"> Opreator e.g. "le" (less equal), "ge" (greater equal), "lt"(less than), "gt" (greater than) or "eq" (equal) </param>
        /// <param name="value"> Value e.g. 50 pixel/ 24 bit, etc. </param>
        /// <returns> </returns>
        [VersionIsGreaterThanOrEqual(10, VersionName = "Version 10")]
        ReadOnlyCollection<IFile> GetByAttribute(FileComparisonAttribute attribute, FileComparisonOperator @operator,
                                                 int value);

        ReadOnlyCollection<IFile> GetByAuthor(IUser user);
        ReadOnlyCollection<IFile> GetByLastModifier(IUser user);
        void UpdateThumbnailAndFileInformation(string filename);
        void UpdateThumbnailAndFileInformationForAll();
        void UpdateThumbnailAndFileInformationRange(IEnumerable<string> filenames);
    }

    internal class AssetManagerFiles : Files, IAssetManagerFiles
    {
        internal AssetManagerFiles(IAssetManagerFolder folder, Caching caching) : base(folder, caching)
        {
            RetrieveFunc = GetFiles;
        }

        public new IAssetManagerFolder Folder
        {
            get { return (IAssetManagerFolder) base.Folder; }
        }

        private List<IFile> GetFiles()
        {
            const string LIST_FILES =
                @"<MEDIA><FOLDER  guid=""{0}"" subdirguid=""{0}""><FILES action=""list"" view=""thumbnail"" maxfilesize=""0"" attributeguid="""" searchtext=""*"" pattern="""" startcount=""1"" orderby=""name""/></FOLDER></MEDIA>";

            return RetrieveFiles(LIST_FILES.RQLFormat(Folder));
        }

        [VersionIsGreaterThanOrEqual(10, VersionName = "Version 10")]
        public ReadOnlyCollection<IFile> GetByAttribute(FileComparisonAttribute attribute,
                                                        FileComparisonOperator @operator, int value)
        {
            VersionVerifier.EnsureVersion(Session);

            const string FILTER_FILES_BY_COMMAND =
                @"<MEDIA><FOLDER  guid=""{0}"" subdirguid=""{0}""><FILES action=""list"" view=""thumbnail"" sectioncount=""30"" maxfilesize=""0""  command=""{1}"" op=""{2}"" value=""{3}""  startcount=""1"" orderby=""name""/></FOLDER></MEDIA>";

            var rqlString = FILTER_FILES_BY_COMMAND.RQLFormat(Folder, ComparisonAttributeToString(attribute),
                                                              ComparisonOperatorToString(@operator), value);
            return RetrieveFiles(rqlString).AsReadOnly();
        }

        public ReadOnlyCollection<IFile> GetByAuthor(IUser author)
        {
            const string FILTER_FILES_BY_CREATOR =
                @"<MEDIA><FOLDER  guid=""{0}"" subdirguid=""{0}""><FILES action=""list"" view=""thumbnail"" maxfilesize=""0"" createguid=""{1}"" pattern="""" startcount=""1"" orderby=""name""/></FOLDER></MEDIA>";

            var query = FILTER_FILES_BY_CREATOR.RQLFormat(Folder, author);
            return RetrieveFiles(query).AsReadOnly();
        }

        public ReadOnlyCollection<IFile> GetByLastModifier(IUser lastModifier)
        {
            const string FILTER_FILES_BY_CHANGEAUTHOR =
                @"<MEDIA><FOLDER  guid=""{0}"" subdirguid=""{0}""><FILES action=""list"" view=""thumbnail"" maxfilesize=""0"" changeguid=""{1}"" pattern="""" startcount=""1"" orderby=""name""/></FOLDER></MEDIA>";

            var query = FILTER_FILES_BY_CHANGEAUTHOR.RQLFormat(Folder, lastModifier);
            return RetrieveFiles(query).AsReadOnly();
        }

        public void UpdateThumbnailAndFileInformation(string filename)
        {
            UpdateThumbnailAndFileInformationRange(new[] {filename});
        }

        public void UpdateThumbnailAndFileInformationForAll()
        {
            UpdateThumbnailAndFileInformationRange(this.Select(file => file.Name));
        }

        public void UpdateThumbnailAndFileInformationRange(IEnumerable<string> filenames)
        {
            const string FILE_TO_UPDATE = @"<FILE action=""update"" sourcename=""{0}""/>";

            var enumerable = filenames as IList<string> ?? filenames.ToList();
            var rqlFiles = enumerable.Select(s => FILE_TO_UPDATE.SecureRQLFormat(s));
            var files = string.Join(string.Empty, rqlFiles);

            const string UPDATE_FILES_IN_FOLDER = @"<MEDIA><FOLDER guid=""{0}"">{1}</FOLDER></MEDIA>";
            var xmlDoc = Project.ExecuteRQL(UPDATE_FILES_IN_FOLDER.RQLFormat(Folder, files));
            if (xmlDoc.GetElementsByTagName("THUMB").Count == 0)
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format(
                                                "Could not update thumbnails/file information in folder {0} on: {1}",
                                                Folder, string.Join(", ", enumerable)));
            }
        }

        private static string ComparisonAttributeToString(FileComparisonAttribute attribute)
        {
            switch (attribute)
            {
                case FileComparisonAttribute.Width:
                    return "width";
                case FileComparisonAttribute.Heigth:
                    return "height";
                case FileComparisonAttribute.Size:
                    return "size";
                case FileComparisonAttribute.Depth:
                    return "depth";
                default:
                    throw new ArgumentException(string.Format("Unknown file attribute: {0}", attribute));
            }
        }

        private static string ComparisonOperatorToString(FileComparisonOperator @operator)
        {
            switch (@operator)
            {
                case FileComparisonOperator.Greater:
                    return "gt";
                case FileComparisonOperator.Less:
                    return "lt";
                case FileComparisonOperator.LessEqual:
                    return "le";
                case FileComparisonOperator.GreaterEqual:
                    return "ge";
                case FileComparisonOperator.Equal:
                    return "eq";
                default:
                    throw new ArgumentException(string.Format("Unknown comparison operator: {0}", @operator));
            }
        }
    }
}