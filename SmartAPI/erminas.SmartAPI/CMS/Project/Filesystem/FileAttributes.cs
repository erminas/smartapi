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
using System.Xml;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.Filesystem
{
    public class FileAttributes
    {
        internal FileAttributes(IFolder folder, XmlElement xmlElement)
        {
            Folder = folder;
            LoadXml(xmlElement);
        }

        public string Album { get; private set; }
        public string Artist { get; private set; }
        public string Colordepth { get; private set; }
        public string Comment { get; private set; }
        public string DocAuthor { get; private set; }
        public string DocCreatedWith { get; private set; }
        public string DocCreationDate { get; private set; }
        public string DocModificationDate { get; private set; }
        public int DocNumberOfPages { get; private set; }
        public string DocOriginalAuthor { get; private set; }
        public string DocTitle { get; private set; }
        public DateTime EntryDate { get; private set; }
        public string Filesize { get; private set; }
        public IFolder Folder { get; private set; }
        public string Genre { get; private set; }
        public string Height { get; private set; }
        public string Keywords { get; private set; }
        public string LastEditor { get; private set; }
        public DateTime ModificationDate { get; private set; }
        public string OriginalAuthor { get; private set; }
        public string Title { get; private set; }
        public string Width { get; private set; }
        public string Year { get; private set; }

        /*
        private static readonly Dictionary<string, string> RQLMapping = new Dictionary<string, string>()
                                                           {
                                                               {"ext01", "Erstellungsautor"}, 
                                                               {"ext02", "Datum der Erstaufnahme"}, 
                                                               {"ext03", "Änderungsautor"}, 
                                                               {"ext04", "Änderungsdatum"}, 
                                                               {"ext05", "Höhe (Pixel)"}, 
                                                               {"ext06", "Breite (Pixel)"}, 
                                                               {"ext07", "Farbtiefe (Bit)"}, 
                                                               {"ext08", "Dateigröße (Byte)"}, 
                                                               {"ext09", "Titel"}, 
                                                               {"ext10", "Künstler"}, 
                                                               {"ext11", "Album"}, 
                                                               {"ext12", "Jahr"}, 
                                                               {"ext13", "Kommentar"}, 
                                                               {"ext14", "Genre"}
                                                           };*/

        private void LoadXml(XmlElement xmlElement)
        {
            if (xmlElement == null)
            {
                return;
            }
            try
            {
                if (xmlElement.GetAttributeValue("ext01") != null)
                {
                    OriginalAuthor = xmlElement.GetAttributeValue("ext01");
                }

                if (xmlElement.GetAttributeValue("ext02") != null)
                {
                    EntryDate = xmlElement.GetOADate("ext02").GetValueOrDefault();
                }

                if (xmlElement.GetAttributeValue("ext03") != null)
                {
                    LastEditor = xmlElement.GetAttributeValue("ext03");
                }

                if (xmlElement.GetAttributeValue("ext04") != null)
                {
                    ModificationDate = xmlElement.GetOADate("ext04").GetValueOrDefault();
                }

                if (xmlElement.GetAttributeValue("ext05") != null)
                {
                    Height = xmlElement.GetAttributeValue("ext05") + " Pixel";
                }

                if (xmlElement.GetAttributeValue("ext06") != null)
                {
                    Width = xmlElement.GetAttributeValue("ext06") + " Pixel";
                }

                if (xmlElement.GetAttributeValue("ext07") != null)
                {
                    Colordepth = xmlElement.GetAttributeValue("ext07") + " Bit";
                }

                if (xmlElement.GetAttributeValue("ext08") != null)
                {
                    Filesize = xmlElement.GetAttributeValue("ext08") + " Byte";
                }

                if (xmlElement.GetAttributeValue("ext09") != null)
                {
                    Title = xmlElement.GetAttributeValue("ext09");
                }

                if (xmlElement.GetAttributeValue("ext10") != null)
                {
                    Artist = xmlElement.GetAttributeValue("ext10");
                }

                if (xmlElement.GetAttributeValue("ext11") != null)
                {
                    Album = xmlElement.GetAttributeValue("ext11");
                }

                if (xmlElement.GetAttributeValue("ext12") != null)
                {
                    Year = xmlElement.GetAttributeValue("ext12");
                }

                if (xmlElement.GetAttributeValue("ext13") != null)
                {
                    Comment = xmlElement.GetAttributeValue("ext13");
                }

                if (xmlElement.GetAttributeValue("ext14") != null)
                {
                    Genre = xmlElement.GetAttributeValue("ext14");
                }

                if (xmlElement.GetAttributeValue("ext4124") != null)
                {
                    DocTitle = xmlElement.GetAttributeValue("ext4124");
                }

                if (xmlElement.GetAttributeValue("ext4125") != null)
                {
                    Keywords = xmlElement.GetAttributeValue("ext4125");
                }

                if (xmlElement.GetAttributeValue("ext4126") != null)
                {
                    DocAuthor = xmlElement.GetAttributeValue("ext4126");
                }

                if (xmlElement.GetAttributeValue("ext4127") != null)
                {
                    DocOriginalAuthor = xmlElement.GetAttributeValue("ext4127");
                }

                if (xmlElement.GetAttributeValue("ext4128") != null)
                {
                    DocCreatedWith = xmlElement.GetAttributeValue("ext4128");
                }

                if (xmlElement.GetAttributeValue("ext4129") != null)
                {
                    DocCreationDate = xmlElement.GetAttributeValue("ext4129");
                }

                if (xmlElement.GetAttributeValue("ext4130") != null)
                {
                    DocModificationDate = xmlElement.GetAttributeValue("ext4130");
                }

                if (xmlElement.GetAttributeValue("ext4131") != null)
                {
                    DocNumberOfPages = int.Parse(xmlElement.GetAttributeValue("ext4131"));
                }
            } catch (Exception e)
            {
                // couldn't read data
                throw new FileDataException(Folder.Project.Session.ServerLogin, "Couldn't read file data..", e);
            }
        }
    }
}