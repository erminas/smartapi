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

using System.Collections.Generic;
using System.Xml;

namespace erminas.SmartAPI.CMS
{
    public class WebCompliance : RedDotObject
    {
        private List<string> _errors;
        private bool _ignorePermissions;
        private Page _page;
        private List<string> _warnings;

        internal WebCompliance(Page page, bool ignorePermissions)
        {
            _page = page;
            _ignorePermissions = ignorePermissions;
        }

        internal WebCompliance(Page page)
        {
            _page = page;
            _ignorePermissions = false;
        }

        public bool IsValid
        {
            get { return int.Parse(XmlNode.Attributes.GetNamedItem("approved").Value) == 1; }
        }

        public List<string> Errors
        {
            get
            {
                if (_errors == null)
                {
                    _errors = ExtractDetails(XmlNode.SelectNodes("descendant::ERROR"));
                    _errors.AddRange(ExtractDetails(XmlNode.SelectNodes("descendant::FATALERROR")));
                }
                return _errors;
            }
        }

        public List<string> Warnings
        {
            get { return _warnings ?? (_warnings = ExtractDetails(XmlNode.SelectNodes("descendant::WARNING"))); }
        }

        private static List<string> ExtractDetails(XmlNodeList xmlNodeList)
        {
            var details = new List<string>();
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                string line = xmlNode.Attributes.GetNamedItem("row").Value;
                string column = xmlNode.Attributes.GetNamedItem("col").Value;
                string text = xmlNode.InnerText;
                details.Add(text + " at line " + line + ", column " + column + ".");
            }
            return details;
        }


        protected override void LoadXml(XmlElement node)
        {
        }
    }
}