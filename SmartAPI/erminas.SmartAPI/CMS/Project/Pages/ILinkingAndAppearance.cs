using System;
using System.Xml;
using erminas.SmartAPI.CMS.Project.Pages.Elements;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.Pages
{
    internal class LinkingAndAppearance : ILinkingAndAppearance
    {
        public IPage Page { get; private set; }
        public ILinkElement Link { get; private set; }
        public DateTime AppearenceStart { get; private set; }
        public DateTime AppearenceEnd { get; private set; }
        public bool HasAppearenceStart { get { return AppearenceStart != DateTime.MinValue; } }
        public bool HasAppearenceEnd { get { return AppearenceEnd != DateTime.MaxValue; } }
        public bool IsActive { get; private set; }

        internal LinkingAndAppearance(IPage page, XmlElement element)
        {
            Page = page;
            LoadXml(element);
        }

        private void LoadXml(XmlElement element)
        {
            Link = (ILinkElement) PageElement.CreateElement(Project, element);

            var start = element.GetOADate("startdate");
            AppearenceStart = !start.HasValue ? DateTime.MinValue : start.Value;

            var end = element.GetOADate("enddate");
            AppearenceEnd = !end.HasValue ? DateTime.MaxValue : end.Value;

            var dateState = element.GetIntAttributeValue("datestate").GetValueOrDefault();
            IsActive = dateState == 1 || dateState == 3;
        }

        public ISession Session { get { return Page.Session; } }
        public IProject Project { get { return Page.Project; } }

        public Guid Guid { get { return Link.Guid; } }
        public string Name { get { return Link.Name; } }
    }

    public interface ILinkingAndAppearance : IProjectObject, IRedDotObject
    {
        IPage Page { get; }

        ILinkElement Link { get; }

        DateTime AppearenceStart { get; }
        DateTime AppearenceEnd { get; }

        bool HasAppearenceStart { get; }
        bool HasAppearenceEnd { get; }
        
        bool IsActive { get; }
    }
}