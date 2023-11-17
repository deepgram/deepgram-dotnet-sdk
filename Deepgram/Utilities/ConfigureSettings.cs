using System.Text.RegularExpressions;

namespace Deepgram.Utilities;
public static class ClientOptionsUtil
{
    public static DeepgramClientOptions Configure(DeepgramClientOptions? clientOptions = null)
    {
        var options = clientOptions ?? new DeepgramClientOptions();
        options.Url = options.Url is null ? Constants.DEFAULT_URI : CleanUrl(options.Url);
        return options;
    }

    private static string CleanUrl(string value)
    {
        string url;
        string pattern = @"http[s]://";
        url = Regex.Replace(value, pattern, string.Empty);
        return url.TrimEnd('/');
    }

}
