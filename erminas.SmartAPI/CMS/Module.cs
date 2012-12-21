using System.Xml;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
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