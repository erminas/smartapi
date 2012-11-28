using System.Xml;
using erminas.SmartAPI.CMS.CCElements.Attributes;

namespace erminas.SmartAPI.CMS.CCElements
{
    public class ProjectContent : CCElement
    {
        private readonly ElementReferenceAttribute _elementReference;

        public ProjectContent(ContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("eltislistentry", "eltinvisibleinpage", "eltisreffield");
            _elementReference = new ElementReferenceAttribute(this);
        }

        public override sealed ContentClassCategory Category
        {
            get { return ContentClassCategory.Content; }
        }

        public CCElement ReferencedElement
        {
            get { return _elementReference.Value; }
            set { _elementReference.Value = value; }
        }

        public bool IsHitList
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltislistentry")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltislistentry")).Value = value; }
        }

        public bool IsNotVisibleOnPublishedPage
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltinvisibleinpage")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltinvisibleinpage")).Value = value; }
        }

        public bool IsReferenceField
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltisreffield")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltisreffield")).Value = value; }
        }
    }
}