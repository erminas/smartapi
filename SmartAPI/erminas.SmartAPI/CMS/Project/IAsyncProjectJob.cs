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
using System.Linq;
using erminas.SmartAPI.CMS.Administration;

namespace erminas.SmartAPI.CMS.Project
{
    public interface IAsyncJob : ISessionObject
    {
        string EmailMessage { get; set; }
        IUser EmailReceipient { get; set; }
        string EmailSubject { get; set; }
        bool IsSendingEmailOnCompletion { get; set; }

        /// <summary>
        ///     Run this job asynchronously.
        ///     If you need to wait for the job to finish, use <see cref="RunSync" /> instead.
        /// </summary>
        void RunAsync();

        /// <summary>
        ///     Run this job and wait until it is finished. Jobs can take some time, so make sure you have a large enough maxWait for your server to finish.
        /// </summary>
        /// <exception cref="TimeoutException">Thrown, if the copy job wasn't finished in time</exception>
        void RunSync(TimeSpan maxWait);

        IApplicationServer Server { get; set; }
    }

    public interface IAsyncProjectJob : IAsyncJob, IProjectObject
    {
    }

    internal abstract class AbstractAsyncJob : IAsyncJob
    {
        protected AbstractAsyncJob(ISession session)
        {
            Session = session;
            EmailReceipient = Session.CurrentUser;
            IsSendingEmailOnCompletion = true;
            Server = Session.ApplicationServers.First();
        }

        public string EmailMessage { get; set; }
        public IUser EmailReceipient { get; set; }
        public string EmailSubject { get; set; }
        public bool IsSendingEmailOnCompletion { get; set; }

        public abstract void RunAsync();
        public abstract void RunSync(TimeSpan maxWait);
        public IApplicationServer Server { get; set; }

        public ISession Session { get; private set; }
    }

    internal abstract class AbstractAsyncProjectJob : AbstractAsyncJob, IAsyncProjectJob
    {
        private readonly IProject _project;

        protected AbstractAsyncProjectJob(IProject project) : base(project.Session)
        {
            _project = project;
        }

        public IProject Project
        {
            get { return _project; }
        }
    }
}