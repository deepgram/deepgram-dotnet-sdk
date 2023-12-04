
using Deepgram.Logger;

internal static class ApiKeyUtil
{
    public static string Validate(string? apiKey, string clientType)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Log.ApiKeyNotPresent(LogProvider.GetLogger(clientType), clientType);
            throw new ArgumentException("A Deepgram API Key is required when creating a client");
        }
        return apiKey;
    }
}
