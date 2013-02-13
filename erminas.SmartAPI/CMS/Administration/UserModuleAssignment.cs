// Smart API - .Net programmatic access to RedDot servers
//  
// Copyright (C) 2013 erminas GbR
// 
// This program is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with this program.
// If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Administration
{
    [Flags]
    public enum ServerManagerRights
    {
        None = 0,
        CreateUsers = 262144,
        EditUsers = 4194304,
        DeleteUsers = 131072,
        CreateGroups = 1048576,
        DeleteGroups = 524288,
        AssignUsersToGroups = 2097152,
        AdministerUserDefinedJobs = 16777216,
        AdministerPlugins = 8388608,
        AdministerDatabaseServers = 33554432,
        AdministerApplicationServers = 67108864,
        AdministerDirectoryServices = 134217728,
        AdministerProjects = 268435456,
        AdministerXCMSProjects = 536870912,
        AdministerDeliveryServer = 1073741824
    }

    /// <summary>
    ///     Contains the module assignment for a single user. You iterate over the assigned modules through the IEnumerable interface and test for a specific assignment through the
    ///     <see
    ///         cref="IsModuleAssigned" />
    ///     method. New modules can be assigned to the user with the <see cref="SetIsModuleAssigned" /> method. The assigned modules are cached and the cache can be invalidated/refreshed through the ICaching interface methods. The cache only needs to get invalidated/refreshed for possible external changes. All changes to the module assignment through an instance of UserModuleAssignment get reflected in the assigned modules without a need for a cache update.
    /// </summary>
    public class UserModuleAssignment : IEnumerable<Module>, ICaching, ISessionObject
    {
        private readonly IndexedRDList<ModuleType, Module> _assignedModules;

        private readonly User _user;

        internal UserModuleAssignment(User user)
        {
            _user = user;
            Session = _user.Session;
            _assignedModules = new IndexedRDList<ModuleType, Module>(GetAssignedModules, module => module.Type,
                                                                     Caching.Enabled);
        }

        public void AddServerManagerRights(ServerManagerRights right)
        {
            ServerManagerRights |= right;
        }

        public bool HasAccessToAssetManager
        {
            get { return IsModuleAssigned(ModuleType.Translation); }
            set { SetIsModuleAssigned(ModuleType.Translation, value); }
        }

        public bool HasAccessToSmartEdit
        {
            get { return IsModuleAssigned(ModuleType.SmartEdit); }
            set { SetIsModuleAssigned(ModuleType.SmartEdit, value); }
        }

        public bool HasAccessToSmartTree
        {
            get { return IsModuleAssigned(ModuleType.SmartTree); }
            set { SetIsModuleAssigned(ModuleType.SmartTree, value); }
        }

        public bool IsModuleAssigned(ModuleType moduleType)
        {
            return _assignedModules.ContainsKey(moduleType);
        }

        public bool IsServerManager
        {
            get { return IsModuleAssigned(ModuleType.ServerManager); }
            set { SetIsModuleAssigned(ModuleType.ServerManager, value); }
        }

        public bool IsTemplateEditor
        {
            get { return IsModuleAssigned(ModuleType.TemplateEditor); }
            set { SetIsModuleAssigned(ModuleType.TemplateEditor, value); }
        }

        public bool IsTranslationEditor
        {
            get { return IsModuleAssigned(ModuleType.Translation); }
            set { SetIsModuleAssigned(ModuleType.Translation, value); }
        }

        public void RemoveServerManagerRights(ServerManagerRights right)
        {
            ServerManagerRights ^= right;
        }

        public ServerManagerRights ServerManagerRights
        {
            get
            {
                Module serverManagerModule;
                if (!_assignedModules.TryGet(ModuleType.ServerManager, out serverManagerModule))
                {
                    return ServerManagerRights.None;
                }

                return GetServerManagerRights(serverManagerModule);
            }
            set
            {
                var serverManagerModule = _user.Session.Modules[ModuleType.ServerManager];

                ServerManagerRights rights = RightsWithResolvedDepenecies(value);

                const string SAVE_SERVER_MANAGER_RIGHTS =
                    @"<MODULE guid=""{0}"" checked=""{1}"" servermanagerflag=""{2}""/>";

                ExecuteSaveModules(SAVE_SERVER_MANAGER_RIGHTS.RQLFormat(serverManagerModule,
                                                                        rights != ServerManagerRights.None, (int) rights));
                _assignedModules.InvalidateCache();
            }
        }

        public void SetIsModuleAssigned(ModuleType moduleType, bool assign)
        {
            var module = _user.Session.Modules[moduleType];

            var modulesToAssign = new List<Module> {module};
            if (IsModuleDependentOnSmartEdit(moduleType))
            {
                var smartEditModule = _user.Session.Modules[ModuleType.SmartEdit];
                modulesToAssign.Add(smartEditModule);
            }
            string moduleAssignmentSubString = CreateModuleAssignmentSubString(modulesToAssign, assign);
            ExecuteSaveModules(moduleAssignmentSubString);
        }

        public void SetModuleAssignment(UserModuleAssignment otherAssignment)
        {
            var modulesToUnassign = this.Except(otherAssignment, new NameEqualityComparer<Module>());

            string unassign = CreateModuleAssignmentSubString(modulesToUnassign, false);
            string assign = CreateModuleAssignmentSubString(otherAssignment, true);

            ExecuteSaveModules(unassign + assign);

            ServerManagerRights = otherAssignment.ServerManagerRights;
        }

        private static string CreateModuleAssignmentSubString(IEnumerable<Module> modulesToUnassign, bool value)
        {
            return modulesToUnassign.Aggregate("",
                                               (s, module) =>
                                               s + @"<MODULE guid=""{0}"" checked=""{1}"" />".RQLFormat(module, value));
        }

        private void ExecuteSaveModules(string modules)
        {
            const string SAVE_MODULES =
                @"<ADMINISTRATION><USER action=""save"" guid=""{0}""><MODULES>{1}</MODULES></USER></ADMINISTRATION>";
            _user.Session.ExecuteRQL(SAVE_MODULES.RQLFormat(_user, modules));
            _assignedModules.InvalidateCache();
        }

        private List<Module> GetAssignedModules()
        {
            const string LIST_ASSIGNED_MODULES =
                @"<ADMINISTRATION><MODULES action=""list"" userguid=""{0}"" countlicense=""1""/></ADMINISTRATION>";
            var xmlDoc = _user.Session.ExecuteRQL(LIST_ASSIGNED_MODULES.RQLFormat(_user));
            //create a copy of the elements because the XmlNodeList returned by GetElementsByTagName would get modified during iteration in the linq/ToList() expression
            //due to the cloning of XmlElement in the Module->AbstractAttributeContainer c'tor. This would lead to an InvalidOperationException.
            var modules = xmlDoc.GetElementsByTagName("MODULE").Cast<XmlElement>().ToList();

            return (from curModule in modules where IsAssignedModule(curModule) select new Module(curModule)).ToList();
        }

        private static ServerManagerRights GetServerManagerRights(Module serverManagerModule)
        {
            return
                (ServerManagerRights)
                serverManagerModule.XmlElement.GetIntAttributeValue("servermanagerflag").GetValueOrDefault();
        }

        private static bool IsAssignedModule(XmlElement curModule)
        {
            return curModule.GetBoolAttributeValue("checked").GetValueOrDefault();
        }

        private static bool IsModuleDependentOnSmartEdit(ModuleType moduleType)
        {
            return moduleType == ModuleType.SmartTree || moduleType == ModuleType.TemplateEditor ||
                   moduleType == ModuleType.Translation || moduleType == ModuleType.Assets;
        }

        private static ServerManagerRights ResolveAssignUsersToGroupDependencies(ServerManagerRights rights)
        {
            const ServerManagerRights DEPENDENT_ON_ASSIGN_USER_TO_GROUP =
                ServerManagerRights.EditUsers | ServerManagerRights.CreateGroups | ServerManagerRights.CreateUsers;
            if (!rights.HasFlag(ServerManagerRights.AssignUsersToGroups) &&
                (rights & DEPENDENT_ON_ASSIGN_USER_TO_GROUP) != 0)
            {
                rights |= ServerManagerRights.AssignUsersToGroups;
            }
            return rights;
        }

        private static ServerManagerRights ResolveDepenciesOfAdministerDirectoryServices(ServerManagerRights value)
        {
            if (value.HasFlag(ServerManagerRights.AdministerDirectoryServices))
            {
                value |= ServerManagerRights.CreateUsers | ServerManagerRights.EditUsers |
                         ServerManagerRights.DeleteUsers | ServerManagerRights.AssignUsersToGroups |
                         ServerManagerRights.DeleteGroups | ServerManagerRights.CreateGroups;
            }
            return value;
        }

        private static ServerManagerRights ResolveEditUsersDependencies(ServerManagerRights value)
        {
            if (value.HasFlag(ServerManagerRights.CreateUsers))
            {
                value |= ServerManagerRights.EditUsers;
            }
            return value;
        }

        private ServerManagerRights RightsWithResolvedDepenecies(ServerManagerRights value)
        {
            value = ResolveAssignUsersToGroupDependencies(value);
            value = ResolveEditUsersDependencies(value);
            value = ResolveDepenciesOfAdministerDirectoryServices(value);

            return value;
        }

        #region ICaching Members

        public void InvalidateCache()
        {
            _assignedModules.InvalidateCache();
        }

        public void Refresh()
        {
            _assignedModules.Refresh();
        }

        #endregion

        #region IEnumerable<Module> Members

        public IEnumerator<Module> GetEnumerator()
        {
            return _assignedModules.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public Session Session { get; private set; }
    }
}