using System;
using System.Xml;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.CCElements.Attributes
{
    public class ElementReferenceAttribute : IRDAttribute
    {
        private readonly CCElement _parent;

        public ElementReferenceAttribute(CCElement parent)
        {
            _parent = parent;
            parent.RegisterAttribute(this);
        }

        public CCElement Value
        {
            get
            {
                Guid projectGuid, ccGuid, elementGuid;
                XmlNode xmlNode = _parent.XmlNode;
                if (!xmlNode.TryGetGuid("eltprojectguid", out projectGuid) ||
                    !xmlNode.TryGetGuid("elttemplateguid", out ccGuid) ||
                    !xmlNode.TryGetGuid("eltelementguid", out elementGuid))
                {
                    return null;
                }

                string langId = xmlNode.GetAttributeValue("eltlanguagevariantid");

                Project project = _parent.ContentClass.Project.Session.Projects.GetByGuid(projectGuid);
                ContentClass contentClass = project.ContentClasses.GetByGuid(ccGuid);

                return contentClass.
                    Elements[langId].GetByGuid(elementGuid);
            }
            set
            {
                XmlNode xmlNode = _parent.XmlNode;
                xmlNode.SetAttributeValue("eltlanguagevariantid", value.LanguageVariant.Language);
                xmlNode.SetAttributeValue("eltelementguid", value.Guid.ToRQLString());
                xmlNode.SetAttributeValue("elttemplateguid", value.ContentClass.Guid.ToRQLString());
                xmlNode.SetAttributeValue("eltprojectguid", value.ContentClass.Project.Guid.ToRQLString());
            }
        }

        #region IRDAttribute Members

        public string Name
        {
            get { return "__elementreferenceattribute"; }
        }

        public object DisplayObject
        {
            get
            {
                CCElement ccElement = Value;
                if (ccElement == null)
                {
                    return "not set";
                }
                return string.Format("Element {0} of content class {1} in project {2}", ccElement.Name,
                                     ccElement.ContentClass.Name, ccElement.ContentClass.Project.Name);
            }
        }

        public string Description
        {
            get { return "Element"; }
        }

        public bool IsAssignableFrom(IRDAttribute o, out string reason)
        {
            reason = string.Empty;
            return o is ElementReferenceAttribute;
        }

        public void Assign(IRDAttribute o)
        {
            var other = (ElementReferenceAttribute) o;

            CCElement otherCCElement = other.Value;
            try
            {
                var project = _parent.ContentClass.Project.Session.Projects[otherCCElement.ContentClass.Project.Name];
                var cc = project.ContentClasses[otherCCElement.ContentClass.Name];
                Value = cc.Elements[otherCCElement.LanguageVariant.Language].GetByName(otherCCElement.Name);
            }
            catch (Exception e)
            {
                throw new Exception(
                    string.Format("Can't find project/content class/element {0}/{1}/{2} on server",
                                  otherCCElement.ContentClass.Project.Name, otherCCElement.ContentClass.Name,
                                  otherCCElement.Name), e);
            }
        }

        #endregion
    }
}