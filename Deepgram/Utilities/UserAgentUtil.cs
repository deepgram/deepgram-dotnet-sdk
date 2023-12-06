namespace Deepgram.Utilities;

internal static class UserAgentUtil
{
    /// <summary>
    /// determines the UserAgent Library version
    /// </summary>
    /// <returns></returns>
    public static string GetInfo()
    {
        var libraryVersion = Assembly.GetExecutingAssembly().GetName().Version;

        var languageVersion = new Regex("[ ,/,:,;,_,(,)]")
             .Replace(System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
             string.Empty);

        return $"deepgram/{libraryVersion} dotnet/{languageVersion} ";
    }
}
