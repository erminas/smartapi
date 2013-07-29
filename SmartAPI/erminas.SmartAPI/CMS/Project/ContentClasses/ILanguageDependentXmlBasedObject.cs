using System.Xml;

namespace erminas.SmartAPI.CMS.Project.ContentClasses
{
    public interface ILanguageDependentXmlBasedObject : IXmlBasedObject
    {
        XmlElement GetXmlElementForLanguage(string languageAbbreviation);
    }
}