using System;
using System.Xml;
using erminas.SmartAPI.CMS.Administration;

namespace erminas.SmartAPI.CMS.Project
{
    public abstract class RedDotProjectObject : RedDotObject, IProjectObject
    {
        protected RedDotProjectObject(Project project)
        {
            Project = project;
        }

        protected RedDotProjectObject(Project project, Guid guid) : base(guid)
        {
            Project = project;
        }

        protected RedDotProjectObject(Project project, XmlElement xmlElement) : base(xmlElement)
        {
            Project = project;
        }

        public Session Session { get { return Project.Session; } }
        public Project Project { get; private set; }
    }
}
