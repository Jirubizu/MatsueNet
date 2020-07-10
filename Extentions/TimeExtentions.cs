using System.Threading;
using System;
using Discord;
using Interactivity.Pagination;

namespace MatsueNet.Extentions
{
    public static class TimeExtentions
    {
        public static TimeSpan StripMilliseconds(this TimeSpan time)
        {
            return new TimeSpan(time.Days, time.Hours, time.Minutes, time.Seconds);
        }
    }
}