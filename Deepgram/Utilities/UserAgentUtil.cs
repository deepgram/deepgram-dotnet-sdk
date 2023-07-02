using System.Reflection;

namespace Deepgram.Utilities;

internal static class UserAgentUtil
{
    /// <summary>
    /// determines the useragent for the httpclient
    /// </summary>
    /// <returns></returns>
    public static string GetUserAgent()
    {

        var languageVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription.Replace(" ", string.Empty)
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
