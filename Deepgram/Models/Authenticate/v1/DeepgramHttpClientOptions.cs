// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Authenticate.v1;

/// <summary>
/// Configuration for the Deepgram client
/// </summary>
public class DeepgramHttpClientOptions : IDeepgramClientOptions
{
    /*****************************/
    // General Options
    /*****************************/
    /// <summary>
    /// Deepgram API KEY
    /// </summary>
    public string ApiKey { get; set; }

    /// <summary>
    /// Deepgram Access Token (OAuth 2.0 compliant)
    /// </summary>
    public string AccessToken { get; set; }

    /// <summary>
    /// BaseAddress of the server :defaults to api.deepgram.com
    /// no need to attach the protocol it will be added internally
    /// </summary>
    public string BaseAddress { get; set; } = Defaults.DEFAULT_URI;

    /// <summary>
    /// Api endpoint version
    /// </summary>
    public string APIVersion { get; set; } = Defaults.DEFAULT_API_VERSION;

    /// <summary>
    /// Global headers to always be added to the request
    /// </summary>
    public Dictionary<string, string> Headers { get; set; }

    /// <summary>
    /// Global addons to always be added to the request
    /// </summary>
    public Dictionary<string, string> Addons { get; set; }

    /*****************************/
    // Prerecorded
    /*****************************/

    /*****************************/
    // Live (These aren't used in this class)
    /*****************************/
    public bool KeepAlive { get; }
    public decimal AutoFlushReplyDelta { get; }

    /// <summary>
    /// Based on the options set, do we want to inspect the Listen Messages. If yes, then return true.
    /// </summary>
    public bool InspectListenMessage()
    {
        // This is only a WebSocket capability
        return false;
    }

    /// <summary>
    /// Based on the options set, do we want to inspect the Speak Messages. If yes, then return true.
    /// </summary>
    public bool InspectSpeakMessage()
    {
        // This is only a WebSocket capability
        return false;
    }

    /*****************************/
    // OnPrem
    /*****************************/
    /// <summary>
    /// Enable when using OnPrem mode
    /// </summary>
    public bool OnPrem { get; set; } = false;

    /*****************************/
    // Manage
    /*****************************/

    /*****************************/
    // Analyze
    /*****************************/

    /*****************************/
    // Speak
    /*****************************/

    public decimal AutoFlushSpeakDelta { get; }

    /*****************************/
    // Constructor
    /*****************************/
    public DeepgramHttpClientOptions(string? apiKey = null, string? baseAddress = null, bool? onPrem = null, Dictionary<string, string>? options = null, Dictionary<string, string>? headers = null, string? accessToken = null)
    {
        Log.Verbose("DeepgramHttpClientOptions", "ENTER");
        Log.Debug("DeepgramHttpClientOptions", apiKey == null ? "API KEY is null" : "API KEY provided");
        Log.Debug("DeepgramHttpClientOptions", baseAddress == null ? "BaseAddress is null" : "BaseAddress provided");
        Log.Debug("DeepgramHttpClientOptions", accessToken == null ? "ACCESS TOKEN is null" : "ACCESS TOKEN provided");
        Log.Debug("DeepgramHttpClientOptions", onPrem == null ? "OnPrem is null" : "OnPrem provided");
        Log.Debug("DeepgramHttpClientOptions", headers == null ? "Headers is null" : "Headers provided");

        KeepAlive = false;
        BaseAddress = baseAddress ?? Defaults.DEFAULT_URI;
        OnPrem = onPrem ?? false;
        Addons = headers ?? new Dictionary<string, string>();
        Headers = headers ?? new Dictionary<string, string>();

        Log.Information("DeepgramHttpClientOptions", $"OnPrem: {OnPrem}");
        Log.Information("DeepgramHttpClientOptions", $"APIVersion: {APIVersion}");

        // Priority-based credential resolution
        // 1. Explicit accessToken parameter (highest priority)
        // 2. Explicit apiKey parameter
        // 3. DEEPGRAM_ACCESS_TOKEN environment variable
        // 4. DEEPGRAM_API_KEY environment variable (lowest priority)

        // Initialize both credentials to empty strings
        ApiKey = "";
        AccessToken = "";

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            AccessToken = accessToken;
            // ApiKey remains empty (cleared above)
            Log.Information("DeepgramHttpClientOptions", "ACCESS TOKEN set from parameter");
        }
        else if (!string.IsNullOrWhiteSpace(apiKey))
        {
            ApiKey = apiKey;
            // AccessToken remains empty (cleared above)
            Log.Information("DeepgramHttpClientOptions", "API KEY set from parameter");
        }
        else
        {
            // Try environment variables with same priority order
            var envAccessToken = Environment.GetEnvironmentVariable(Defaults.DEEPGRAM_ACCESS_TOKEN);
            var envApiKey = Environment.GetEnvironmentVariable(Defaults.DEEPGRAM_API_KEY);

            if (!string.IsNullOrWhiteSpace(envAccessToken))
            {
                AccessToken = envAccessToken;
                // ApiKey remains empty (cleared above)
                Log.Information("DeepgramHttpClientOptions", "ACCESS TOKEN set from environment variable");
            }
            else if (!string.IsNullOrWhiteSpace(envApiKey))
            {
                ApiKey = envApiKey;
                // AccessToken remains empty (cleared above)
                Log.Information("DeepgramHttpClientOptions", "API KEY set from environment variable");
            }
            else
            {
                Log.Warning("DeepgramHttpClientOptions", "No authentication credentials found in parameters or environment variables");
            }
        }

        // Ensure we have some form of authentication for non-OnPrem deployments
        if (!OnPrem && string.IsNullOrWhiteSpace(AccessToken) && string.IsNullOrWhiteSpace(ApiKey))
        {
            var exStr = "Deepgram authentication is required. Please provide either an API Key or Access Token.";
            Log.Error("DeepgramHttpClientOptions", exStr);
            throw new ArgumentException(exStr);
        }

        // base url
        Log.Debug("DeepgramHttpClientOptions", $"REST BaseAddress: {BaseAddress}");

        Regex regex = new Regex(@"\b(\/v[0-9]+)\b", RegexOptions.IgnoreCase);
        if (!regex.IsMatch(BaseAddress))
        {
            Log.Information("DeepgramHttpClientOptions", $"REST BaseAddress does not contain API version: {BaseAddress}");
            BaseAddress = $"{BaseAddress}/{APIVersion}";
            Log.Debug("DeepgramHttpClientOptions", $"BaseAddress: {BaseAddress}");
        }

        //checks for http:// https:// http https - https:// is include to ensure it is all stripped out and correctly formatted 
        regex = new Regex(@"\b(http:\/\/|https:\/\/|http|https)\b", RegexOptions.IgnoreCase);
        if (!regex.IsMatch(BaseAddress))
        {
            Log.Information("DeepgramHttpClientOptions", $"BaseAddress contains no protocol: {BaseAddress}");
            BaseAddress = $"https://{BaseAddress}";
            Log.Debug("DeepgramHttpClientOptions", $"BaseAddress: {BaseAddress}");
        }
        BaseAddress = BaseAddress.TrimEnd('/');

        Log.Information("DeepgramHttpClientOptions", $"BaseAddress: {BaseAddress}");
        Log.Verbose("DeepgramHttpClientOptions", "LEAVE");
    }

    /*****************************/
    // Dynamic Authentication Methods
    /*****************************/

    /// <summary>
    /// Sets the API Key for authentication (clears AccessToken)
    /// </summary>
    /// <param name="apiKey">The API Key to use</param>
    public void SetApiKey(string apiKey)
    {
        ApiKey = apiKey ?? "";
        AccessToken = ""; // Clear access token when setting API key
        Log.Information("DeepgramHttpClientOptions", "API KEY set, ACCESS TOKEN cleared");
    }

    /// <summary>
    /// Sets the Access Token for authentication (clears ApiKey)
    /// </summary>
    /// <param name="accessToken">The Access Token to use</param>
    public void SetAccessToken(string accessToken)
    {
        AccessToken = accessToken ?? "";
        ApiKey = ""; // Clear API key when setting access token
        Log.Information("DeepgramHttpClientOptions", "ACCESS TOKEN set, API KEY cleared");
    }

    /// <summary>
    /// Clears all authentication credentials
    /// </summary>
    public void ClearCredentials()
    {
        ApiKey = "";
        AccessToken = "";
        Log.Information("DeepgramHttpClientOptions", "All authentication credentials cleared");
    }
}
