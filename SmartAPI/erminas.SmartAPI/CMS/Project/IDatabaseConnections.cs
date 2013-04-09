using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project
{
    public interface IDatabaseConnections : IIndexedRDList<string, IDatabaseConnection>, IProjectObject
    {
    }

    internal class DatabaseConnections : NameIndexedRDList<IDatabaseConnection>, IDatabaseConnections
    {
        private readonly Project _project;

        internal DatabaseConnections(Project project, Caching caching) : base(caching)
        {
            _project = project;
            RetrieveFunc = GetDatabaseConnections;
        }

        private List<IDatabaseConnection> GetDatabaseConnections()
        {
            const string LIST_DATABASE_CONNECTION = @"<DATABASES action=""list""/>";
            XmlDocument xmlDoc = _project.ExecuteRQL(LIST_DATABASE_CONNECTION, Project.RqlType.SessionKeyInProject);
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("DATABASE");

            return (from XmlElement curNode in xmlNodes select (IDatabaseConnection) new DatabaseConnection(_project, curNode)).ToList();
        }

        public Session Session { get { return _project.Session; } }
        public Project Project { get { return _project; } }
    }
}
