using System;
using System.Linq;
using erminas.SmartAPI.CMS.Administration;

namespace erminas.SmartAPI.CMS.Project
{
    public interface IProjectJob
    {

        /// <summary>
        /// Run this job and wait until the IT is finished. Jobs can take some time, so make sure you have a large enough maxWait for your server to finish.
        /// </summary>
        /// <exception cref="TimeoutException">Thrown, if the copy job wasn't finished in time</exception>
        void RunSync(TimeSpan maxWait);

        /// <summary>
        /// Run this job asynchronously.
        /// If you need to wait for the job to finish, use <see cref="RunSync"/> instead.
        /// </summary>
        void RunAsync();
        Project Project { get; }
        ApplicationServer Server { get; set; }
        string EmailMessage { get; set; }
        string EmailSubject { get; set; }
        User EmailReceipient { get; set; }
        bool IsSendingEmailOnCompletion { get; set; }
    }

    internal abstract class AbstractProjectJob : IProjectJob, IProjectObject
    {
        private readonly Project _project;

        protected AbstractProjectJob(Project project)
        {
            _project = project;
            EmailReceipient = Session.CurrentUser;
            IsSendingEmailOnCompletion = true;
            Server = Session.ApplicationServers.First();
        }
        public abstract void RunAsync();
        public abstract void RunSync(TimeSpan maxWait);


        public Project Project { get { return _project; } }
        public ApplicationServer Server { get; set; }
        public string EmailMessage { get; set; }
        public string EmailSubject { get; set; }
        public User EmailReceipient { get; set; }
        public bool IsSendingEmailOnCompletion { get; set; }
        public Session Session { get { return _project.Session; } }
    }
}