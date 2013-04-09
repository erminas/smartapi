using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project
{
    public interface ISyllables : IIndexedRDList<string, ISyllable>, IProjectObject
    {
    }

    internal class Syllables : NameIndexedRDList<ISyllable> , ISyllables
    {
        private readonly Project _project;

        internal Syllables(Project project, Caching caching) : base(caching)
        {
            RetrieveFunc = GetSyllables;
            _project = project;
        }
        private List<ISyllable> GetSyllables()
        {
            var xmlDoc = Project.ExecuteRQL(@"<SYLLABLES action=""list""/>", Project.RqlType.SessionKeyInProject);
            var syllablelist = xmlDoc.GetElementsByTagName("SYLLABLE");
            return (from XmlElement curNode in syllablelist select (ISyllable)new Syllable(Project, curNode)).ToList();
        }
        public Session Session { get { return _project.Session; } }
        public Project Project { get { return _project; } }
    }
}
