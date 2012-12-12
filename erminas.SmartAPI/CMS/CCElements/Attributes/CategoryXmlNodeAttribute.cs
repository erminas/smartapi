using System;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.CCElements.Attributes
{
    public class CategoryXmlNodeAttribute : AbstractGuidXmlNodeAttribute<Category>
    {
        public CategoryXmlNodeAttribute(ContentClass parent, string name) : base(parent, name)
        {
        }

        protected override Category RetrieveByGuid(Guid guid)
        {
            return ((ContentClass) Parent).Project.Categories.GetByGuid(guid);
        }

        protected override Category RetrieveByName(string name)
        {
            return ((ContentClass)Parent).Project.Categories.GetByName(name);
        }
        
        private bool IsArbitraryCategory
        {
            get
            {
                var value = ((ContentClass) Parent).XmlNode.GetAttributeValue(Name);
                return value == "-1" || String.IsNullOrEmpty(value);
            }
        }

        public void SetUseArbitraryCategory()
        {
            SetXmlNodeValue("-1");
        }

        protected override void UpdateValue(string value)
        {
            var updateValue = IsArbitraryCategoryValue(value) ? null : value;

            base.UpdateValue(updateValue);
        }

        private static bool IsArbitraryCategoryValue(string value)
        {
            return String.IsNullOrEmpty(value) || value == "-1";
        }

        public override void Assign(IRDAttribute o)
        {
            var categoryXmlNodeAttribute = ((CategoryXmlNodeAttribute) o);
            var value = categoryXmlNodeAttribute.Value;
            
            if (categoryXmlNodeAttribute.IsArbitraryCategory)
            {
                SetUseArbitraryCategory();
            }
            else
            {
                SetValue(value.Name);
            }
        }

        protected override void SetXmlNodeValue(string value)
        {
            Parent.XmlNode.SetAttributeValue(Name, string.IsNullOrEmpty(value) ? "-1" : value);
        }

        protected override string GetTypeDescription()
        {
            return "category";
        }
    }
}
