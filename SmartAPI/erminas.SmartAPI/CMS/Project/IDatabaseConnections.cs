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
        private readonly IProject _project;

        internal DatabaseConnections(IProject project, Caching caching) : base(caching)
        {
            _project = project;
            RetrieveFunc = GetDatabaseConnections;
        }

        private List<IDatabaseConnection> GetDatabaseConnections()
        {
            const string LIST_DATABASE_CONNECTION = @"<DATABASES action=""list""/>";
            XmlDocument xmlDoc = _project.ExecuteRQL(LIST_DATABASE_CONNECTION, RqlType.SessionKeyInProject);
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("DATABASE");

            return (from XmlElement curNode in xmlNodes select (IDatabaseConnection) new DatabaseConnection(_project, curNode)).ToList();
        }

        public ISession Session { get { return _project.Session; } }
        public IProject Project { get { return _project; } }
    }
}
