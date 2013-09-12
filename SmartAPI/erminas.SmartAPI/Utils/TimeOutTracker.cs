// SmartAPI - .Net programmatic access to RedDot servers
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

namespace erminas.SmartAPI.Utils
{
    internal class TimeOutTracker
    {
        private readonly DateTime _start;
        private readonly TimeSpan _timeToWait;

        internal TimeOutTracker(int timeoutInMs)
        {
            _timeToWait = new TimeSpan(0, 0, 0, 0, timeoutInMs);
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