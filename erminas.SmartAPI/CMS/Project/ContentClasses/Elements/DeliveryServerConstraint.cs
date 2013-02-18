using System.Xml;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public class DeliveryServerConstraint : ContentClassElement
    {
        internal DeliveryServerConstraint(ContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("eltignoreworkflow", "eltlanguageindependent", "eltrequired", 
                "eltinvisibleinclient", "elthideinform");
        }

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Content; }
        }

        public bool IsEditingMandatory
        {
            get { return GetAttributeValue<bool>("eltrequired"); }
            set { SetAttributeValue("eltrequired", value); }
        }

        public bool IsHiddenInProjectStructure
        {
            get { return GetAttributeValue<bool>("eltinvisibleinclient"); }
            set { SetAttributeValue("eltinvisibleinclient", value); }
        }

        public bool IsLanguageIndependent
        {
            get { return GetAttributeValue<bool>("eltlanguageindependent"); }
            set { SetAttributeValue("eltlanguageindependent", value); }
        }

        public bool IsNotRelevantForWorklow
        {
            get { return GetAttributeValue<bool>("eltignoreworkflow"); }
            set { SetAttributeValue("eltignoreworkflow", value); }
        }

        public bool IsNotUsedInForm
        {
            get { return GetAttributeValue<bool>("elthideinform"); }
            set { SetAttributeValue("elthideinform", value); }
        }
    }
}
