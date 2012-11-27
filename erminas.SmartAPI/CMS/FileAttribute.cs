/*
 * Smart API - .Net programatical access to RedDot servers
 * Copyright (C) 2012  erminas GbR 
 *
 * This program is free software: you can redistribute it and/or modify it 
 * under the terms of the GNU General Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details. 
 *
 * You should have received a copy of the GNU General Public License along with this program.
 * If not, see <http://www.gnu.org/licenses/>. 
 */

using System;
using System.Globalization;
using System.Xml;
using erminas.SmartAPI.Exceptions;
using erminas.Utilities;

namespace erminas.SmartAPI.CMS
{
    public class FileAttribute
    {
        public FileAttribute(XmlNode node)
        {
            LoadXml(node);
        }

        public string OriginalAuthor { get; set; }
        public string EntryDate { get; set; }
        public string LastEditor { get; set; }
        public string ModificationDate { get; set; }
        public string Height { get; set; }
        public string Width { get; set; }
        public string Colordepth { get; set; }
        public string Filesize { get; set; }

        // Audio maybe
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Year { get; set; }
        public string Comment { get; set; }
        public string Genre { get; set; }

        // maybe dirreferent Text Documents (pdf verified)
        public string DocTitle { get; set; }
        public string Keywords { get; set; }
        public string DocAuthor { get; set; }
        public string DocOriginalAuthor { get; set; }
        public string DocCreatedWith { get; set; }
        public string DocCreationDate { get; set; }
        public string DocModificationDate { get; set; }
        public int DocNumberOfPages { get; set; }


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

        protected void LoadXml(XmlNode node)
        {
            if (node != null && node.Attributes != null)
            {
                try
                {
                    if (node.GetAttributeValue("ext01") != null)
                    {
                        OriginalAuthor = node.GetAttributeValue("ext01");
                    }

                    if (node.GetAttributeValue("ext02") != null)
                    {
                        EntryDate =
                            DateTime.FromOADate(double.Parse(node.GetAttributeValue("ext02"),
                                                             CultureInfo.InvariantCulture)).ToString(
                                                                 CultureInfo.CurrentCulture);
                    }

                    if (node.GetAttributeValue("ext03") != null)
                    {
                        LastEditor = node.GetAttributeValue("ext03");
                    }

                    if (node.GetAttributeValue("ext04") != null)
                    {
                        ModificationDate =
                            DateTime.FromOADate(double.Parse(node.GetAttributeValue("ext04"),
                                                             CultureInfo.InvariantCulture)).ToString(
                                                                 CultureInfo.CurrentCulture);
                    }

                    if (node.GetAttributeValue("ext05") != null)
                    {
                        Height = node.GetAttributeValue("ext05") + " Pixel";
                    }

                    if (node.GetAttributeValue("ext06") != null)
                    {
                        Width = node.GetAttributeValue("ext06") + " Pixel";
                    }

                    if (node.GetAttributeValue("ext07") != null)
                    {
                        Colordepth = node.GetAttributeValue("ext07") + " Bit";
                    }

                    if (node.GetAttributeValue("ext08") != null)
                    {
                        Filesize = node.GetAttributeValue("ext08") + " Byte";
                    }

                    if (node.GetAttributeValue("ext09") != null)
                    {
                        Title = node.GetAttributeValue("ext09");
                    }

                    if (node.GetAttributeValue("ext10") != null)
                    {
                        Artist = node.GetAttributeValue("ext10");
                    }

                    if (node.GetAttributeValue("ext11") != null)
                    {
                        Album = node.GetAttributeValue("ext11");
                    }

                    if (node.GetAttributeValue("ext12") != null)
                    {
                        Year = node.GetAttributeValue("ext12");
                    }

                    if (node.GetAttributeValue("ext13") != null)
                    {
                        Comment = node.GetAttributeValue("ext13");
                    }

                    if (node.GetAttributeValue("ext14") != null)
                    {
                        Genre = node.GetAttributeValue("ext14");
                    }

                    if (node.GetAttributeValue("ext4124") != null)
                    {
                        DocTitle = node.GetAttributeValue("ext4124");
                    }

                    if (node.GetAttributeValue("ext4125") != null)
                    {
                        Keywords = node.GetAttributeValue("ext4125");
                    }

                    if (node.GetAttributeValue("ext4126") != null)
                    {
                        DocAuthor = node.GetAttributeValue("ext4126");
                    }

                    if (node.GetAttributeValue("ext4127") != null)
                    {
                        DocOriginalAuthor = node.GetAttributeValue("ext4127");
                    }

                    if (node.GetAttributeValue("ext4128") != null)
                    {
                        DocCreatedWith = node.GetAttributeValue("ext4128");
                    }

                    if (node.GetAttributeValue("ext4129") != null)
                    {
                        DocCreationDate = node.GetAttributeValue("ext4129");
                    }

                    if (node.GetAttributeValue("ext4130") != null)
                    {
                        DocModificationDate = node.GetAttributeValue("ext4130");
                    }

                    if (node.GetAttributeValue("ext4131") != null)
                    {
                        DocNumberOfPages = int.Parse(node.GetAttributeValue("ext4131"));
                    }
                }
                catch (Exception e)
                {
                    // couldn't read data
                    throw new RedDotDataException("Couldn't read file data..", e);
                }
            }
        }
    }
}