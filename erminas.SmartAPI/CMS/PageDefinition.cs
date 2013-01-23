using System;
using System.Xml;

namespace erminas.SmartAPI.CMS
{
    public class PageDefinition : RedDotObject, IPageDefinition
    {
        private readonly ContentClass _contentClass;
        private string _description;
        
        internal PageDefinition(ContentClass contentClass, XmlElement element) : base(element)
        {
            _contentClass = contentClass;
            LoadXml();
        }

        private void LoadXml()
        {
            InitIfPresent(ref _description, "description", x=>x);
        }

        public string Description { get { return _description; } }
        
        public ContentClass ContentClass { get { return _contentClass; } }
    }
}
