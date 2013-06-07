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
using System.Threading;

namespace erminas.SmartAPI.Utils
{
    public static class Wait
    {
        static Wait()
        {
            const int DEFAULT_PERIOD_IN_MS = 100;
            DefaultRetryPeriod = new TimeSpan(0, 0, 0, 0, DEFAULT_PERIOD_IN_MS);
        }

        public static TimeSpan DefaultRetryPeriod { get; set; }

        public static void For(Func<bool> pred, TimeSpan wait)
        {
            For(pred, wait, DefaultRetryPeriod);
        }

        public static void For(Func<bool> pred, TimeSpan wait, TimeSpan retry)
        {
            if (retry > wait)
            {
                throw new ArgumentException("Retry period must not be greater than wait");
            }
            var tt = new TimeOutTracker(wait);
            bool isSuccess;
            var lastTry = DateTime.Now;
            while (!(isSuccess = pred()) && !tt.HasTimedOut)
            {
                TimeSpan timeSpan = retry - (DateTime.Now - lastTry);
                if (timeSpan.TotalMilliseconds > 0)
                {
                    Thread.Sleep(timeSpan);
                }
                lastTry = DateTime.Now;
            }
            if (!isSuccess)
            {
                throw new TimeoutException(string.Format("timed out after waiting for {0}", wait));
            }
        }
    }
}