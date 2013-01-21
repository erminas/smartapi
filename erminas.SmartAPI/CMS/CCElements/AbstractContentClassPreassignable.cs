using System.Xml;

namespace erminas.SmartAPI.CMS.CCElements
{
    public abstract class AbstractContentClassPreassignable : CCElement, IContentClassPreassignable
    {
        protected AbstractContentClassPreassignable(ContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            PreassignedContentClasses = new PreassignedContentClassesAndPageDefinitions(this);
        }

        public PreassignedContentClassesAndPageDefinitions PreassignedContentClasses { get; private set; }
    }
}
