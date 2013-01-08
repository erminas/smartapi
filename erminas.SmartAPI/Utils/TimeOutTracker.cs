using System;

namespace erminas.SmartAPI.Utils
{
    class TimeOutTracker
    {
        private readonly TimeSpan _timeToWait;
        private readonly DateTime _start;

        internal TimeOutTracker(int timeoutInMs)
        {
            _timeToWait = new TimeSpan(0,0,0,0,timeoutInMs);
            _start = DateTime.Now;
        }

        internal TimeOutTracker(TimeSpan timeToWait)
        {
            _timeToWait = timeToWait;
            _start = DateTime.Now;
        }

        internal bool HasTimedOut
        {
            get { return DateTime.Now - _start > _timeToWait; }
        }
    }
}
