using System;

namespace Deepgram.Common
{
    public class TimeoutSingleton
    {
        public TimeSpan Timeout { get; set; } = TimeSpan.Zero;
        static TimeoutSingleton() { }

        public static TimeoutSingleton Instance = new TimeoutSingleton();

    }
}
