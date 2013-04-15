using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project
{
    public interface IProjectGroups : IIndexedRDList<string, IGroup>, IProjectObject
    {
        void Add(IGroup group);
        void Remove(IGroup group);
        void AddRange(IEnumerable<IGroup> group);
        void Clear();
        void Set(IEnumerable<IGroup> group);
        new IProjectGroups Refreshed();
        IGroup CreateAndAdd(string groupName, string groupEmailAdress);
    }

    internal class ProjectGroups : NameIndexedRDList<IGroup>, IProjectGroups
    {
        private readonly IProject _project;
        private const string SINGLE_GROUP = @"<GROUP guid=""{0}"" />";

        internal ProjectGroups(IProject project, Caching caching) : base(caching)
        {
            _project = project;
            RetrieveFunc = GetAssignedGroups;
        }

        private List<IGroup> GetAssignedGroups()
        {
            const string LIST_GROUPS =
                @"<ADMINISTRATION><PROJECT guid=""{0}""><GROUPS action=""list""/></PROJECT></ADMINISTRATION>";
            var xmlDoc = Session.ExecuteRQL(LIST_GROUPS.RQLFormat(_project), RQL.IODataFormat.LogonGuidOnly);
            return
                (from XmlElement curGroup in xmlDoc.GetElementsByTagName("GROUP") select (IGroup)new Group(Session, curGroup))
                    .ToList();
        }

        public ISession Session { get { return _project.Session; } }
        
        public IProject Project { get { return _project; } }

        public void Add(IGroup @group)
        {
            AddRange(new []{@group});
        }

        public void Remove(IGroup @group)
        {
            RemoveRange(new []{@group});
        }

        public void AddRange(IEnumerable<IGroup> groups)
        {
            const string ASSIGN_GROUPS =
              @"<ADMINISTRATION action=""assign""><PROJECT guid=""{0}"">{1}</PROJECT></ADMINISTRATION>";

            var groupsList = groups as IList<IGroup> ?? groups.ToList();
            if (!groupsList.Any())
            {
                return;
            }
            var groupsPart = groupsList.Aggregate("", (s, @group) => s + SINGLE_GROUP.RQLFormat(@group));

            var xmlDoc = Session.ExecuteRQL(ASSIGN_GROUPS.RQLFormat(Project, groupsPart));

            if (!xmlDoc.InnerText.Contains("ok"))
            {
                var errorGroups = groupsList.Aggregate("", (s, @group) => s + @group.ToString() + ";");
                throw new SmartAPIException(Session.ServerLogin, string.Format("Could not add group(s) to {0}: {1}",Project,  errorGroups));
            }
            InvalidateCache();
        }

        private void RemoveRange(IEnumerable<IGroup> groups)
        {
            const string UNASSIGN_GROUPS =
               @"<ADMINISTRATION action=""unlink""><PROJECT guid=""{0}"">{1}</PROJECT></ADMINISTRATION>";

            var groupsList = groups as IList<IGroup> ?? groups.ToList();
            if (!groupsList.Any())
            {
                return;
            }
            var groupsPart = groupsList.Aggregate("", (s, @group) => s + SINGLE_GROUP.RQLFormat(@group));

            var xmlDoc = Session.ExecuteRQL(UNASSIGN_GROUPS.RQLFormat(Project, groupsPart));

            if (!xmlDoc.InnerText.Contains("ok"))
            {
                var errorGroups = groupsList.Aggregate("", (s, @group) => s + @group.ToString() + ";");
                throw new SmartAPIException(Session.ServerLogin, string.Format("Could not remove group(s) from {0}: {1}", Project, errorGroups));
            }
            InvalidateCache();
        }

        public void Clear()
        {
            RemoveRange(this);
        }

        public void Set(IEnumerable<IGroup> newGroups)
        {
            var curGroups = this.ToList();
            var newGroupsList = newGroups as IList<IGroup> ?? newGroups.ToList();

            RemoveRange(curGroups.Except(newGroupsList));
            AddRange(newGroupsList.Except(curGroups));
        }

        IProjectGroups IProjectGroups.Refreshed()
        {
            return this;
        }

        public IGroup CreateAndAdd(string groupName, string groupEmailAdress)
        {
            const string CREATE_GROUP =
                @"<ADMINISTRATION><PROJECT guid=""{0}"" action=""assign""><GROUPS action=""addnew""><GROUP name=""{1}"" email=""{2}""/></GROUPS></PROJECT></ADMINISTRATION>";

            var xmlDoc = Session.ExecuteRQL(CREATE_GROUP.SecureRQLFormat(_project, groupName, groupEmailAdress));

            var groupGuid = xmlDoc.GetSingleElement("GROUP").GetGuid();
            InvalidateCache();
            return GroupFactory.CreateFromGuid(Session, groupGuid);
        }
    }
}
