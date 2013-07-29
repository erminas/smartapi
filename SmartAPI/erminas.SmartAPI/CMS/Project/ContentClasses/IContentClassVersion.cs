using System;
using erminas.SmartAPI.CMS.Administration;
using erminas.SmartAPI.CMS.Project.Folder;

namespace erminas.SmartAPI.CMS.Project.ContentClasses
{
    public interface IContentClassVersion : IRedDotObject, IProjectObject
    {
        IContentClass ContentClass { get; }
        ContentClassVersionType CreationType { get; }

        /// <summary>
        ///     Time the version was created
        /// </summary>
        DateTime Date { get; }

        /// <summary>
        ///     Description text
        /// </summary>
        string Description { get; }

        IContentClassFolder Folder { get; }
        IUser User { get; }
        string Username { get; }
    }
}