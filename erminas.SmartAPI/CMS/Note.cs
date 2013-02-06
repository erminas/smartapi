﻿// Smart API - .Net programatical access to RedDot servers
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

namespace erminas.SmartAPI.CMS
{
    public class Note : RedDotObject
    {
        #region NoteType enum

        public enum NoteType
        {
            Line = 1,
            TextField = 2
        }

        #endregion

        public readonly Workflow Workflow;

        public Note(Workflow workflow, XmlElement xmlElement) : base(xmlElement)
        {
            Workflow = workflow;
            LoadXml();
        }

        public string Value { get; private set; }
        public NoteType Type { get; private set; }

        private void LoadXml()
        {
            Value = XmlElement.GetAttributeValue("value");
            Type = (NoteType) XmlElement.GetIntAttributeValue("type").GetValueOrDefault();
        }
    }
}