using System.Reflection;

namespace Deepgram.Utilities
{
    internal static class UserAgentUtil
    {
        public static string GetUserAgent()
        {

            // TODO: watch the next core release; may have functionality to make this cleaner
            var languageVersion = (System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription)
                .Replace(" ", string.Empty)
                .Replace("/", string.Empty)
                .Replace(":", string.Empty)
                .Replace(";", string.Empty)
                .Replace("_", string.Empty)
                .Replace("(", string.Empty)
                .Replace(")", string.Empty);



            var libraryVersion = typeof(UserAgentUtil)
                .GetTypeInfo()
                .Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;

            return $"deepgram/{libraryVersion} dotnet/{languageVersion}";
        }
    }
}
