using System.Reflection;
using System.Text.RegularExpressions;

namespace Deepgram.Utilities
{
    internal static class UserAgentUtil
    {
        /// <summary>
        /// determines the useragent for the httpclient
        /// </summary>
        /// <returns></returns>
        public static string GetUserAgent()
        {
            var languageVersion = new Regex("[ ,/,:,;,_,(,)]")
                       .Replace(System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                       string.Empty);

            var libraryVersion = typeof(UserAgentUtil)
                .GetTypeInfo()
                .Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;

            return $"deepgram/{libraryVersion} dotnet/{languageVersion}";
        }
    }
}
