// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Authenticate.v1;

public class DeepgramOptionsFromEnv : IDeepgramClientOptions
{
    /*****************************/
    // Thread Safety
    /*****************************/
    private readonly object _credentialLock = new object();

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
    // Live
    /*****************************/
    /// <summary>
    /// Enable sending KeepAlives for Streaming
    /// </summary>
    public bool KeepAlive { get; set; } = false;

    /// <summary>
    /// <summary>
    /// Sets the interval for automatic flushing of replies in Listen Streaming
    /// </summary>
    public decimal AutoFlushReplyDelta { get; set; } = 0;
    /// <summary>
    /// Based on the options set, do we want to inspect the Listen Messages. If yes, then return true.
    /// </summary>
    public bool InspectListenMessage()
    {
        return AutoFlushReplyDelta > 0;
    }

    /// <summary>
    /// Based on the options set, do we want to inspect the Speak Messages. If yes, then return true.
    /// </summary>
    public bool InspectSpeakMessage()
    {
        return AutoFlushSpeakDelta > 0;
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

    /// <summary>
    /// Sets the interval for automatic flushing in Speak Streaming
    /// </summary>
    public decimal AutoFlushSpeakDelta { get; set; } = 0;

    /*****************************/
    // Constructor
    /*****************************/

    public DeepgramOptionsFromEnv()
    {
        // Initialize both credentials to empty strings
        ApiKey = "";
        AccessToken = "";

        // Priority-based credential resolution from environment variables
        // 1. DEEPGRAM_ACCESS_TOKEN (highest priority)
        // 2. DEEPGRAM_API_KEY (fallback)
        var envAccessToken = Environment.GetEnvironmentVariable(Defaults.DEEPGRAM_ACCESS_TOKEN);
        var envApiKey = Environment.GetEnvironmentVariable(Defaults.DEEPGRAM_API_KEY);

        if (!string.IsNullOrWhiteSpace(envAccessToken))
        {
            AccessToken = envAccessToken;
            // ApiKey remains empty (initialized above)
        }
        else if (!string.IsNullOrWhiteSpace(envApiKey))
        {
            ApiKey = envApiKey;
            // AccessToken remains empty (initialized above)
        }

        BaseAddress = Environment.GetEnvironmentVariable("DEEPGRAM_HOST") ?? Defaults.DEFAULT_URI;
        APIVersion = Environment.GetEnvironmentVariable("DEEPGRAM_API_VERSION") ?? Defaults.DEFAULT_API_VERSION;
        var onPrem = Environment.GetEnvironmentVariable("DEEPGRAM_ON_PREM") ?? "";
        var keepAlive = Environment.GetEnvironmentVariable("DEEPGRAM_KEEP_ALIVE") ?? "";
        var autoFlushReplyDelta = Environment.GetEnvironmentVariable("DEEPGRAM_WEBSOCKET_AUTO_FLUSH") ?? "";
        var autoFlushSpeakDelta = Environment.GetEnvironmentVariable("DEEPGRAM_WEBSOCKET_AUTO_FLUSH") ?? "";

        Addons = new Dictionary<string, string>();
        for (int x = 0; x < 20; x++)
        {
            var param = Environment.GetEnvironmentVariable($"DEEPGRAM_PARAM_{x}");
            if (param != null)
            {
                var value = Environment.GetEnvironmentVariable($"DEEPGRAM_PARAM_VALUE_{x}") ?? "";
                Addons[param] = value;
            }
            else
            {
                break;
            }
        }

        Headers = new Dictionary<string, string>();
        for (int x = 0; x < 20; x++)
        {
            var param = Environment.GetEnvironmentVariable($"DEEPGRAM_HEADERS_{x}");
            if (param != null)
            {
                var value = Environment.GetEnvironmentVariable($"DEEPGRAM_HEADERS_VALUE_{x}") ?? "";
                Headers[param] = value;
            }
            else
            {
                break;
            }
        }

        OnPrem = onPrem.ToLower() == "true";
        KeepAlive = keepAlive.ToLower() == "true";
        if (decimal.TryParse(autoFlushReplyDelta, out var replyDelta))
        {
            AutoFlushReplyDelta = replyDelta;
        }

        if (decimal.TryParse(autoFlushSpeakDelta, out var speakDelta))
        {
            AutoFlushSpeakDelta = speakDelta;
        }

        // addons
        if (Addons.ContainsKey(Constants.AutoFlushReplyDelta))
        {
            var addonValue = Addons[Constants.AutoFlushReplyDelta];
            if (decimal.TryParse(addonValue, out var parsedValue))
            {
                Log.Verbose("DeepgramWsClientOptions", $"AutoFlushReplyDelta: {parsedValue}");
                AutoFlushReplyDelta = parsedValue;
            }
        }
        if (Addons.ContainsKey(Constants.AutoFlushSpeakDelta))
        {
            var addonValue = Addons[Constants.AutoFlushSpeakDelta];
            if (decimal.TryParse(addonValue, out var parsedValue))
            {
                Log.Verbose("DeepgramWsClientOptions", $"AutoFlushSpeakDelta: {parsedValue}");
                AutoFlushSpeakDelta = parsedValue;
            }
        }
    }

    /*****************************/
    // Dynamic Authentication Methods
    /*****************************/

    /// <summary>
    /// Sets the API Key for authentication (clears AccessToken)
    /// </summary>
    /// <param name="apiKey">The API Key to use</param>
    /// <exception cref="ArgumentException">Thrown when apiKey is null or empty</exception>
    public void SetApiKey(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            var exStr = "API Key cannot be null or empty";
            Log.Error("DeepgramOptionsFromEnv.SetApiKey", exStr);
            throw new ArgumentException(exStr, nameof(apiKey));
        }

        lock (_credentialLock)
        {
            ApiKey = apiKey;
            AccessToken = ""; // Clear access token when setting API key
            Log.Information("DeepgramOptionsFromEnv", "API KEY set, ACCESS TOKEN cleared");
        }
    }

    /// <summary>
    /// Sets the Access Token for authentication (clears ApiKey)
    /// </summary>
    /// <param name="accessToken">The Access Token to use</param>
    /// <exception cref="ArgumentException">Thrown when accessToken is null or empty</exception>
    public void SetAccessToken(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            var exStr = "Access Token cannot be null or empty";
            Log.Error("DeepgramOptionsFromEnv.SetAccessToken", exStr);
            throw new ArgumentException(exStr, nameof(accessToken));
        }

        lock (_credentialLock)
        {
            AccessToken = accessToken;
            ApiKey = ""; // Clear API key when setting access token
            Log.Information("DeepgramOptionsFromEnv", "ACCESS TOKEN set, API KEY cleared");
        }
    }

    /// <summary>
    /// Clears all authentication credentials
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when attempting to clear credentials in non-OnPrem deployments</exception>
    public void ClearCredentials()
    {
        // Validate that credential clearing is allowed
        if (!OnPrem)
        {
            var exStr = "Cannot clear authentication credentials in non-OnPrem deployments. Authentication is required for cloud-based Deepgram services.";
            Log.Error("DeepgramOptionsFromEnv.ClearCredentials", exStr);
            throw new InvalidOperationException(exStr);
        }

        lock (_credentialLock)
        {
            ApiKey = "";
            AccessToken = "";
            Log.Information("DeepgramOptionsFromEnv", "All authentication credentials cleared (OnPrem deployment)");
        }
    }
}
