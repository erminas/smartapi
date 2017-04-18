namespace erminas.SmartAPI.CMS.Project
{
    public enum ProjectLockLevel
    {
        None = 0,
        All = -1,
        Admin = 1,
        SiteBuilder = 2,
        Editor = 3,
        Author = 4,
        Visitor = 5,
        Publisher = 16,
        AdminAndPublisher = 17
    }
}