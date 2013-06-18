using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.Administration;
using erminas.SmartAPI.CMS.Project.ContentClasses;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements;
using erminas.SmartAPI.CMS.Project.Pages;
using erminas.SmartAPI.CMS.Project.Pages.Elements;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Authorizations
{
    //public interface IBaseAuthorizationPackage<T> : IRDList<T>, IProjectObject where T : class, IRedDotObject
    //{
    //    /// <summary>
    //    /// Used after setting the Name property to commit the change on the server
    //    /// </summary>
    //    void Commit();

    //    /// <summary>
    //    /// Rename the authorization package on the server.
    //    /// Same as setting the Name property and calling Commit().
    //    /// </summary>
    //    void Rename(string newName);

    //    void Add(T element);
    //    void Remove(T element);
    //}

    //public interface IDetailedPageAuthorizationPackage : IBaseAuthorizationPackage<IPage>
    //{
    //    IUserPageAuthorizations UserAuthorizations { get; }
    //    IGroupPageAuthorizations GroupAuthorizations { get; }
    //}

    //public interface IDetailedLinkAuthorizationPackage : IBaseAuthorizationPackage<ILinkElement>
    //{
    //    IUserLinkAuthorizations UserAuthorizations { get; }
    //    IGroupLinkAuthorizations GroupAuthorizations { get; }
    //}

    //public interface IContentClassAuthorizationPackage : IBaseAuthorizationPackage<IContentClass>
    //{
    //    IUserContentClassAuthorizations UserAuthorizations { get; }
    //    IGroupContentClassAuthorizations GroupAuthorizations { get; }
    //}

    /// <summary>
    /// indexed by group name
    /// </summary>
    public interface IGroupAuthorizations : IIndexedCachedList<string, IGroupAuthorization>
    {
        IGroupAuthorization CreateFor(IGroup group);
    }

    /// <summary>
    /// indexed by user name
    /// </summary>
    public interface IUserAuthorizations : IIndexedCachedList<string, IUserAuthorization>
    {
        IUserAuthorization CreateFor(IUser user);
    }

    public interface IAuthorizationPackage : IRedDotObject, IProjectObject
    {
        void Commit();
        /// <summary>
        /// same as setting Name and Commit();
        /// </summary>
        void Rename(string newName);
        int Type { get; }
        IGroupAuthorizations GroupAuthorizations { get; }
        IUserAuthorizations UserAuthorizations { get; }
    }

    public interface IAuthorizationPackages : IProjectObject
    {
        void CreateGlobal(string name);
        void CreateDetailForLink(ILinkElement link, string name);
        void CreateDetailForLink(IContentClassElement element, string name);
        void CreateDetailForPage(IPage page, string name);
        void CreateDetailForElement(IPageElement pageElement, string name);
        void CreateDetailForElement(IContentClassElement element, string name);

        IIndexedCachedList<string, IAuthorizationPackage> ForLinks { get; }
        IIndexedCachedList<string, IAuthorizationPackage> ForElements { get; }
        IIndexedCachedList<string, IAuthorizationPackage> ForPages { get; }
        IIndexedCachedList<string, IAuthorizationPackage> Standard { get; }
    }
    public enum AuthorizationType
    {
        Standard = 0,
        DetailedPage = 1,
        DetailedLink = 2,
        DetailedElement = 4,
        DetailedAssetManagerAttribute = 8,
        ContentClass=16,
        ProjectVariant = 32,
        Folder=64,
        LanguageVariant=128

    }
    internal class AuthorizationPackages : IAuthorizationPackages
    {
        public AuthorizationPackages(IProject project) 
        {
            Project = project;
            ForLinks = new NameIndexedRDList<IAuthorizationPackage>(GetForLinks, Caching.Enabled);
            ForElements = new NameIndexedRDList<IAuthorizationPackage>(GetForElements, Caching.Enabled);
            ForPages = new NameIndexedRDList<IAuthorizationPackage>(GetForPages, Caching.Enabled);
            Standard = new NameIndexedRDList<IAuthorizationPackage>(GetStandard, Caching.Enabled);
        }

        private List<IAuthorizationPackage> GetStandard()
        {
            return GetAuthorizationPackages(AuthorizationType.Standard);
        }

        private List<IAuthorizationPackage> GetForPages()
        {
            return GetAuthorizationPackages(AuthorizationType.DetailedPage);
        }

        private List<IAuthorizationPackage> GetForElements()
        {
            return GetAuthorizationPackages(AuthorizationType.DetailedElement);
        }

        private List<IAuthorizationPackage> GetForLinks()
        {
            return GetAuthorizationPackages(AuthorizationType.DetailedLink);
        }

        private List<IAuthorizationPackage> GetAuthorizationPackages(AuthorizationType type)
        {
            const string LIST_PACKAGES = @"<AUTHORIZATION><AUTHORIZATIONS action=""list"" type=""{0}""/></AUTHORIZATION>";
            var xmlDoc = Project.ExecuteRQL(LIST_PACKAGES.RQLFormat((int)type));

            return
                (from XmlElement curelement in xmlDoc.GetElementsByTagName("AUTHORIZATION")
                 select (IAuthorizationPackage) new AuthorizationPackage(Project, curelement)).ToList();
        }

        public ISession Session { get { return Project.Session; } }
        public IProject Project { get; private set; }
        public void CreateGlobal(string name)
        {
            const string CREATE = @"<AUTHORIZATION><AUTHORIZATIONPACKET action=""addnew"" name=""{0}""/></AUTHORIZATION>";
            var answer = Project.ExecuteRQL(CREATE.RQLFormat(name));
            CheckAnswer(name, answer);
        }

        public void CreateDetailForLink(ILinkElement link, string name)
        {
            CreateDetail("LINK", link, name, AuthorizationType.DetailedLink);
            ForLinks.InvalidateCache();
        }

        private void CreateDetail(string elementtype, IRedDotObject obj, string name, AuthorizationType type)
        {
            const string CREATE =
                @"<AUTHORIZATION><{0} guid=""{1}><AUTHORIZATIONPACKET action=""addnew"" name=""{2}"" guid="" type=""{3}""/></{0}></AUTHORIZATION>";

            string query = CREATE.RQLFormat(elementtype, obj, name, (int)type);

            var answer = Project.ExecuteRQL(query);
            CheckAnswer(name, answer);
            Standard.InvalidateCache();
        }

        private void CheckAnswer(string name, XmlDocument answer)
        {
            try
            {
                var element = answer.GetSingleElement("AUTHORIZATIONPACKET");
                if (element.GetName() != name)
                {
                    throw new SmartAPIException(Session.ServerLogin,
                                                string.Format("Could not create authorizationpackage {0}", name));
                }
            } catch (SmartAPIInternalException e)
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Could not create authorizationpackage {0}", name), e);
            }
        }

        public void CreateDetailForLink(IContentClassElement element, string name)
        {
            CreateDetail("LINK", element, name, AuthorizationType.DetailedLink);
            ForLinks.InvalidateCache(); ;
        }

        public void CreateDetailForPage(IPage page, string name)
        {
            CreateDetail("PAGE", page, name, AuthorizationType.DetailedPage);
            ForPages.InvalidateCache();
        }

        public void CreateDetailForElement(IPageElement pageElement, string name)
        {
            CreateDetail("ELEMENT", pageElement, name, AuthorizationType.DetailedElement);
            ForElements.InvalidateCache();
        }

        public void CreateDetailForElement(IContentClassElement element, string name)
        {
            CreateDetail("ELEMENT", element, name, AuthorizationType.DetailedElement);
            ForElements.InvalidateCache();
        }

        public IIndexedCachedList<string, IAuthorizationPackage> ForLinks { get; private set; }
        public IIndexedCachedList<string, IAuthorizationPackage> ForElements { get; private set; }
        public IIndexedCachedList<string, IAuthorizationPackage> ForPages { get; private set; }
        public IIndexedCachedList<string, IAuthorizationPackage> Standard { get; private set; }
    }

    internal class AuthorizationPackage : RedDotProjectObject, IAuthorizationPackage
    {
        public AuthorizationPackage(IProject project, XmlElement xmlElement) : base(project, xmlElement)
        {
            Type = xmlElement.GetIntAttributeValue("type").GetValueOrDefault();
            GroupAuthorizations = new GroupAuthorizations(this, Caching.Enabled);
            UserAuthorizations = new UserAuthorizations(this, Caching.Enabled);
        }

        public void Commit()
        {
            const string SAVE_NAME = @"<AUTHORIZATION><AUTHORIZATIONPACKET action=""save"" guid=""{0}"" name=""{1}""/></AUTHORIZATION>";
            Project.ExecuteRQL(SAVE_NAME.RQLFormat(this, Name));
        }

        public void Rename(string newName)
        {
            Name = newName;
            Commit();
        }

        public int Type { get; private set; }

        public IGroupAuthorizations GroupAuthorizations { get; private set; }
        public IUserAuthorizations UserAuthorizations { get; private set; }
    }

    internal class GroupAuthorizations : IndexedCachedList<string, IGroupAuthorization>, IGroupAuthorizations
    {
        private readonly IAuthorizationPackage _package;

        public GroupAuthorizations(IAuthorizationPackage package, Caching caching) : base(authorization => authorization.Group.Name, caching)
        {
            _package = package;
            RetrieveFunc = GetAuthorizations;
        }

        private List<IGroupAuthorization> GetAuthorizations()
        {
            const string LOAD_AUTH =
                @"<AUTHORIZATION><AUTHORIZATIONPACKET action=""load"" guid=""{0}""/></AUTHORIZATION>";

            var xmlDoc = _package.Project.ExecuteRQL(LOAD_AUTH.RQLFormat(_package));

            return (from XmlElement curElement in xmlDoc.GetElementsByTagName("GROUP")
                    let curGroup =  _package.Project.AssignedGroups.GetByGuid(curElement.GetGuid())
                    select
                        (IGroupAuthorization)
                        new GroupAuthorizationRights(_package, curGroup,
                                                     curElement)).ToList();
        }

        public IGroupAuthorization CreateFor(IGroup @group)
        {
            return new GroupAuthorizationRights(_package, group);
        }
    }

    internal class UserAuthorizations : IndexedCachedList<string, IUserAuthorization>, IUserAuthorizations
    {
        private readonly IAuthorizationPackage _package;

        public UserAuthorizations(IAuthorizationPackage package, Caching caching)
            : base(authorization => authorization.User.Name, caching)
        {
            _package = package;
            RetrieveFunc = GetAuthorizations;
        }

        private List<IUserAuthorization> GetAuthorizations()
        {
            const string LOAD_AUTH =
                @"<AUTHORIZATION><AUTHORIZATIONPACKET action=""load"" guid=""{0}""/></AUTHORIZATION>";

            var xmlDoc = _package.Project.ExecuteRQL(LOAD_AUTH.RQLFormat(_package));

            return (from XmlElement curElement in xmlDoc.GetElementsByTagName("USER")
                    let curUser = _package.Project.Users.GetByUserGuid(curElement.GetGuid())
                    select
                        (IUserAuthorization)
                        new UserAuthorizationRights(_package, curUser.User,
                                                     curElement)).ToList();
        }

        public IUserAuthorization CreateFor(IUser @group)
        {
            return new UserAuthorizationRights(_package, group);
        }
    }

    public interface IAuthorizationRights : IProjectObject
    {
        IAuthorizationPackage AuthorizationPackage { get; }

        void Commit();
        int DeniedAssetManagerFolderRights { get; set; }
        int DeniedAssetManagerAttributeRights { get; set; }
        int DeniedContentClassRights { get; set; }

        int DeniedElementRights { get; set; }
        int DeniedGlobalRights { get; set; }
        int DeniedPageRights { get; set; }
        int DeniedProjectOrLanguageVariantRights { get; set; }

        int DeniedStructuralElementRights { get; set; }
        int GrantedAssetManagerFolderRights { get; set; }

        int GrantedAssetManagerAttributeRights { get; set; }

        int GrantedContentClassRights { get; set; }
        int GrantedElementRights { get; set; }

        int GrantedGlobalRights { get; set; }
        int GrantedPageRights { get; set; }
        int GrantedProjectOrLanguageVariantRights { get; set; }
        int GrantedStructuralElementRights { get; set; }

        //PageRights GrantedPageRights { get; set; }
        //PageRights DeniedPageRights { get; set; }

        //ElementRights GrantedElementRights { get; set; }
        //ElementRights DeniedElementRights { get; set; }

        //ContentClassRights GrantedContentClassRights { get; set; }
        //ContentClassRights DeniedContentClassRights { get; set; }

        //ProjectVariantRights GrantedProjectVariantRights { get; set; }
        //ProjectVariantRights DeniedProjectVariantRights { get; set; }

        //LanguageVariantRights GrantedLanguageVariantRights { get; set; }
        //LanguageVariantRights DeniedLanguageVariantRights { get; set; }
    }

    public interface IUserAuthorization : IAuthorizationRights
    {
        IUser User { get; }
    }

    public interface IGroupAuthorization : IAuthorizationRights
    {
        IGroup Group { get; }
    }

    internal abstract class AuthorizationRights : IAuthorizationRights
    {
        private readonly IAuthorizationPackage _package;

        public AuthorizationRights(IAuthorizationPackage package, XmlElement element)
        {
            _package = package;

            GrantedPageRights = element.GetIntAttributeValue("right1").GetValueOrDefault();
            DeniedPageRights = element.GetIntAttributeValue("deny1").GetValueOrDefault();

            GrantedStructuralElementRights = element.GetIntAttributeValue("right2").GetValueOrDefault();
            DeniedStructuralElementRights = element.GetIntAttributeValue("deny2").GetValueOrDefault();

            GrantedElementRights = element.GetIntAttributeValue("right3").GetValueOrDefault();
            DeniedElementRights = element.GetIntAttributeValue("deny3").GetValueOrDefault();

            GrantedGlobalRights = element.GetIntAttributeValue("right4").GetValueOrDefault();
            DeniedGlobalRights = element.GetIntAttributeValue("deny4").GetValueOrDefault();

            GrantedAssetManagerAttributeRights = element.GetIntAttributeValue("right5").GetValueOrDefault();
            DeniedAssetManagerAttributeRights = element.GetIntAttributeValue("deny5").GetValueOrDefault();

            GrantedContentClassRights = element.GetIntAttributeValue("right6").GetValueOrDefault();
            DeniedContentClassRights = element.GetIntAttributeValue("deny6").GetValueOrDefault();

            GrantedProjectOrLanguageVariantRights = element.GetIntAttributeValue("right7").GetValueOrDefault();
            DeniedProjectOrLanguageVariantRights = element.GetIntAttributeValue("deny7").GetValueOrDefault();

            GrantedAssetManagerFolderRights = element.GetIntAttributeValue("right8").GetValueOrDefault();
            DeniedAssetManagerFolderRights = element.GetIntAttributeValue("deny8").GetValueOrDefault();
        }

        protected AuthorizationRights(IAuthorizationPackage package)
        {
            _package = package;
        }


        public IAuthorizationPackage AuthorizationPackage
        {
            get { return _package; }
        }

        public abstract void Commit();
        public int DeniedAssetManagerFolderRights { get; set; }
        public int DeniedAssetManagerAttributeRights { get; set; }
        public int DeniedContentClassRights { get; set; }
        public int DeniedElementRights { get; set; }
        public int DeniedGlobalRights { get; set; }
        public int DeniedPageRights { get; set; }
        public int DeniedProjectOrLanguageVariantRights { get; set; }
        public int DeniedStructuralElementRights { get; set; }
        public int GrantedAssetManagerFolderRights { get; set; }
        public int GrantedAssetManagerAttributeRights { get; set; }
        public int GrantedContentClassRights { get; set; }
        public int GrantedElementRights { get; set; }
        public int GrantedGlobalRights { get; set; }
        public int GrantedPageRights { get; set; }
        public int GrantedProjectOrLanguageVariantRights { get; set; }
        public int GrantedStructuralElementRights { get; set; }

        public IProject Project
        {
            get { return _package.Project; }
        }

        public ISession Session
        {
            get { return _package.Session; }
        }
    }

    internal class UserAuthorizationRights : AuthorizationRights, IUserAuthorization
    {
        public UserAuthorizationRights(IAuthorizationPackage package, IUser user, XmlElement element)
            : base(package, element)
        {
            User = user;
        }

        public UserAuthorizationRights(IAuthorizationPackage package, IUser user) : base(package)
        {
            User = user;
        }

        public override void Commit()
        {
            const string SAVE_USER_RIGHTS =
                @"<AUTHORIZATION><AUTHORIZATIONPACKET action=""save"" guid=""{0}""><USERS><USER guid=""{1}"" right1=""{2}"" right2=""{3}"" right3=""{4}"" right4=""{5}"" right5=""{6}"" right6=""{7}"" right7=""{8}"" right8=""{9}"" deny1=""{10}"" deny2=""{11}"" deny3=""{12}"" deny4=""{13}"" deny5=""{14}"" deny6=""{15}"" deny7=""{16}"" deny8=""{17}""/></USERS></AUTHORIZATIONPACKET></AUTHORIZATION>";

            string query = SAVE_USER_RIGHTS.RQLFormat(AuthorizationPackage, User, GrantedPageRights,
                                                      GrantedStructuralElementRights, GrantedElementRights,
                                                      GrantedGlobalRights, GrantedAssetManagerAttributeRights,
                                                      GrantedContentClassRights, GrantedProjectOrLanguageVariantRights,
                                                      GrantedAssetManagerFolderRights, DeniedPageRights,
                                                      DeniedStructuralElementRights, DeniedElementRights,
                                                      DeniedGlobalRights, DeniedAssetManagerAttributeRights,
                                                      DeniedContentClassRights, DeniedProjectOrLanguageVariantRights,
                                                      DeniedAssetManagerFolderRights );

            Project.ExecuteRQL(query);
        }

        public IUser User { get; private set; }
    }


    internal class GroupAuthorizationRights : AuthorizationRights, IGroupAuthorization
    {
        public IGroup Group { get; private set; }

        public GroupAuthorizationRights(IAuthorizationPackage package, IGroup group, XmlElement element)
            : base(package, element)
        {
            Group = @group;
        }

        public GroupAuthorizationRights(IAuthorizationPackage package, IGroup group) : base(package)
        {
            Group = group;
        }

        public override void Commit()
        {
            const string SAVE_USER_RIGHTS =
                @"<AUTHORIZATION><AUTHORIZATIONPACKET action=""save"" guid=""{0}""><GROUPS><GROUP guid=""{1}"" right1=""{2}"" right2=""{3}"" right3=""{4}"" right4=""{5}"" right5=""{6}"" right6=""{7}"" right7=""{8}"" right8=""{9}"" deny1=""{10}"" deny2=""{11}"" deny3=""{12}"" deny4=""{13}"" deny5=""{14}"" deny6=""{15}"" deny7=""{16}"" deny8=""{17}""/></GROUPS></AUTHORIZATIONPACKET></AUTHORIZATION>";

            string query = SAVE_USER_RIGHTS.RQLFormat(AuthorizationPackage, Group, GrantedPageRights,
                                                      GrantedStructuralElementRights, GrantedElementRights,
                                                      GrantedGlobalRights, GrantedAssetManagerAttributeRights,
                                                      GrantedContentClassRights, GrantedProjectOrLanguageVariantRights,
                                                      GrantedAssetManagerFolderRights, DeniedPageRights,
                                                      DeniedStructuralElementRights, DeniedElementRights,
                                                      DeniedGlobalRights, DeniedAssetManagerAttributeRights,
                                                      DeniedContentClassRights, DeniedProjectOrLanguageVariantRights,
                                                      DeniedAssetManagerFolderRights);

            Project.ExecuteRQL(query);
        }

    }

    //public interface IAuthorizations<T> : IRDList<T> where T : class, IRedDotObject
    //{
    //    void Add(T element);
    //    void AddAll(IEnumerable<T> elements);
    //    void Remove(T element);
    //    void RemoveAll(IEnumerable<T> elements);
    //}
}