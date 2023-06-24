using System;

namespace Deepgram
{
    public sealed class Config
    {
        internal static Credentials Credentials { get; set; } = new Credentials();
        internal static TimeSpan HttpClientTimeOut { get; set; } = TimeSpan.Zero;


        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Config()
        {
        }
    }
}
