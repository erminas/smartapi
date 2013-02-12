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

using System.Xml;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Administration
{
    public enum ModuleType
    {
        NoModule = 0,
        Cms,
        Search,
        TemplateEditor,
        Tasks,
        Translation,
        SmartEdit,
        SmartTree,
        ServerManager,
        Assets
    }

    public static class ModuleTypeUtils
    {
        public static ModuleType ToModuleType(this string value)
        {
            switch (value)
            {
                case "cms":
                    return ModuleType.Cms;
                case "search":
                    return ModuleType.Search;
                case "templateeditor":
                    return ModuleType.TemplateEditor;
                case "translation":
                    return ModuleType.Translation;
                case "tasks":
                    return ModuleType.Tasks;
                case "servermanager":
                    return ModuleType.ServerManager;
                case "assets":
                    return ModuleType.Assets;
                case "smartedit":
                    return ModuleType.SmartEdit;
                case "smarttree":
                    return ModuleType.SmartTree;
                default:
                    throw new SmartAPIInternalException(string.Format("Invalid string value for {0} conversion: {1}",
                                                                      typeof (ModuleType).Name, value));
            }
        }

        public static string ToRQLString(this ModuleType type)
        {
            switch (type)
            {
                case ModuleType.Cms:
                    return "cms";
                case ModuleType.Search:
                    return "search";
                case ModuleType.TemplateEditor:
                    return "templateeditor";
                case ModuleType.Translation:
                    return "translation";
                case ModuleType.Tasks:
                    return "tasks";
                case ModuleType.ServerManager:
                    return "servermanager";
                case ModuleType.Assets:
                    return "assets";
                case ModuleType.SmartEdit:
                    return "smartedit";
                case ModuleType.SmartTree:
                    return "smarttree";
                default:
                    throw new SmartAPIInternalException(string.Format("Invalid {0} for RQL string conversion: {1}",
                                                                      typeof (ModuleType).Name, type));
            }
        }
    }

    public class Module : RedDotObject
    {
        public Module(XmlElement xmlElement) : base(xmlElement)
        {
            LoadXml(xmlElement);
        }

        public ModuleType Type { get; private set; }

        private void LoadXml(XmlElement xmlElement)
        {
            Type = xmlElement.GetAttributeValue("id").ToModuleType();
        }
    }
}