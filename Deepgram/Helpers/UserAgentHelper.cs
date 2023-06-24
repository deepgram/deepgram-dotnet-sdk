using System.Reflection;

namespace Deepgram.Helpers
{
    internal static class UserAgentHelper
    {
        public static string GetUserAgent()
        {
#if NETSTANDARD1_6 || NETSTANDARD2_0 || NETSTANDARD2_1
            // TODO: watch the next core release; may have functionality to make this cleaner
            var languageVersion = (System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription)
                .Replace(" ", string.Empty)
                .Replace("/", string.Empty)
                .Replace(":", string.Empty)
                .Replace(";", string.Empty)
                .Replace("_", string.Empty)
                .Replace("(", string.Empty)
                .Replace(")", string.Empty)
                ;
#else
            var languageVersion = System.Diagnostics.FileVersionInfo
                .GetVersionInfo(typeof(int).Assembly.Location)
                .ProductVersion;
#endif
            var libraryVersion = typeof(UserAgentHelper)
                .GetTypeInfo()
                .Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;

            return $"deepgram/{libraryVersion} dotnet/{languageVersion}";
        }
    }
}
