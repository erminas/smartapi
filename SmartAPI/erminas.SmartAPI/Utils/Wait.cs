using System;
using System.Threading;

namespace erminas.SmartAPI.Utils
{
    public static class Wait
    {
        static Wait()
        {
            const int DEFAULT_PERIOD_IN_MS = 100;
            DefaultRetryPeriod = new TimeSpan(0,0,0,0,DEFAULT_PERIOD_IN_MS);
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
                if (retry.TotalMilliseconds > 0)
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
