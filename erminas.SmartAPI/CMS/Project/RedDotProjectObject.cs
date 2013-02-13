using System;
using System.Xml;

namespace erminas.SmartAPI.CMS.Project
{
    public abstract class RedDotProjectObject : RedDotObject, IProjectObject
    {
        protected RedDotProjectObject(Project project) :base (project.Session)
        {
            Project = project;
        }

        protected RedDotProjectObject(Project project, Guid guid) : base(project.Session, guid)
        {
            Project = project;
        }

        protected RedDotProjectObject(Project project, XmlElement xmlElement) : base(project.Session, xmlElement)
        {
            Project = project;
        }

        public Project Project { get; private set; }
    }
}
