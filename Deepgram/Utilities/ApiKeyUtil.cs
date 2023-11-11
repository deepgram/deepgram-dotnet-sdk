namespace Deepgram.Utilities;

internal static class ApiKeyUtil
{
    internal static string Configure(string? apiKey)
    {
        if (!string.IsNullOrEmpty(apiKey))
        {
            return apiKey;
        }
        else
        {
            var key = Environment.GetEnvironmentVariable(Constants.APIKEY_ENVIRONMENT_NAME);
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("A Deepgram API Key is required");
            }
            return key;
        }
    }
}