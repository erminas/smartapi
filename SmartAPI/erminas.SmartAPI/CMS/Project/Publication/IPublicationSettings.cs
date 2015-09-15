using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Publication
{
    public interface IPublicationSettings : ICachedList<IPublicationSetting>, IProjectObject
    {
        IPublicationPackage PublicationPackage { get; }

        void Add(IProjectVariant projectVariant, ILanguageVariant languageVariant);

        void Remove(IProjectVariant projectVariant, ILanguageVariant languageVariant);
    }

    internal class PublicationSettings : CachedList<IPublicationSetting>, IPublicationSettings
    {
        internal PublicationSettings(IPublicationPackage publicationPackage, Caching caching = Caching.Enabled) : base(caching)
        {
            PublicationPackage = publicationPackage;
            RetrieveFunc = GetPublicationSettings;
        }

        public IPublicationPackage PublicationPackage { get; private set; }

        public ISession Session
        {
            get { return PublicationPackage.Session; }
        }

        public IProject Project
        {
            get { return PublicationPackage.Project; }
        }

        public void Add(IProjectVariant projectVariant, ILanguageVariant languageVariant)
        {
            const string ADD =
                @"<PROJECT><EXPORTSETTING action=""save"" guid=""{0}"" projectvariantguid=""{1}"" languagevariantguid=""{2}"" copyguid="""" /></PROJECT>";

            var doc = Project.ExecuteRQL(ADD.RQLFormat(PublicationPackage, projectVariant, languageVariant.Guid.ToRQLString()));
            //TODO check answer?

            InvalidateCache();
        }

        public void Remove(IProjectVariant projectVariant, ILanguageVariant languageVariant)
        {
            var entry =
                this.FirstOrDefault(x => x.ProjectVariant.Guid == projectVariant.Guid && x.LanguageVariant.Guid == languageVariant.Guid);
            if (entry != null)
            {
                entry.Delete();
            }
        }

        private List<IPublicationSetting> GetPublicationSettings()
        {
            const string LOAD_PUBLICATION_PACKAGE = @"<PROJECT><EXPORTPACKET action=""loadpacket"" guid=""{0}"" /></PROJECT>";
            XmlDocument xmlDoc = Project.ExecuteRQL(LOAD_PUBLICATION_PACKAGE.RQLFormat(PublicationPackage));

            return (from XmlElement curSetting in xmlDoc.GetElementsByTagName("EXPORTSETTING")
                    select (IPublicationSetting) new PublicationSetting(this, curSetting)).ToList();
        }
    }
}
