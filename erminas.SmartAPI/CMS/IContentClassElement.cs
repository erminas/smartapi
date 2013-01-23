using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Xml;
using erminas.SmartAPI.CMS.CCElements.Attributes;

namespace erminas.SmartAPI.CMS
{
    public interface IContentClassElement : IRedDotObject
    {
        /// <summary>
        ///   Element category of the lement
        /// </summary>
        ContentClassCategory Category { get; }

        /// <summary>
        ///   TypeId of the element.
        /// </summary>
        ElementType Type { get; }

        /// <summary>
        ///   Language variant of the element.
        /// </summary>
        LanguageVariant LanguageVariant { get; }

        ContentClass ContentClass { get; set; }

        [ScriptIgnore]
        List<IRDAttribute> Attributes { get; }

        [ScriptIgnore]
        XmlElement XmlNode { get; set; }

        /// <summary>
        ///   Save element on the server. Saves only the attributes!
        /// </summary>
        void Commit();

        void RegisterAttribute(IRDAttribute attribute);
        IRDAttribute GetAttribute(string name);
        void RefreshAttributeValues();
        void AssignAttributes(List<IRDAttribute> attributes);
    }
}