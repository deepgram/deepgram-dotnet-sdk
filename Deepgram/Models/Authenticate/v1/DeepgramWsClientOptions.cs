// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Authenticate.v1;

/// <summary>
/// Configuration for the Deepgram client
/// </summary>
public class DeepgramWsClientOptions : IDeepgramClientOptions
{
    /*****************************/
    // General Options
    /*****************************/
    /// <summary>
    /// Deepgram API KEY
    /// </summary>
    public string ApiKey { get; set; }

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
    // Live
    /*****************************/
    /// <summary>
    /// Enable sending KeepAlives for Streaming
    /// </summary>
    public bool KeepAlive { get; set; } = false;

    /// <summary>
    /// Enable sending KeepAlives for Listen Streaming
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
    // Speak
    /*****************************/

    /// <summary>
    /// Enable sending Flush for Speak Streaming
    /// </summary>
    public decimal AutoFlushSpeakDelta { get; set; } = 0;

    /*****************************/
    // Constructor
    /*****************************/
    public DeepgramWsClientOptions(string? apiKey = null, string? baseAddress = null, bool? keepAlive = null, bool? onPrem = null, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("DeepgramWsClientOptions", "ENTER");
        Log.Debug("DeepgramWsClientOptions", apiKey == null ? "API KEY is null" : "API KEY provided");
        Log.Debug("DeepgramWsClientOptions", baseAddress == null ? "BaseAddress is null" : "BaseAddress provided");
        Log.Debug("DeepgramWsClientOptions", keepAlive == null ? "KeepAlive is null" : "KeepAlive provided");
        Log.Debug("DeepgramWsClientOptions", onPrem == null ? "OnPrem is null" : "OnPrem provided");
        Log.Debug("DeepgramWsClientOptions", headers == null ? "Headers is null" : "Headers provided");
        Log.Debug("DeepgramWsClientOptions", addons == null ? "Addons is null" : "Addons provided");

        ApiKey = apiKey ?? "";
        BaseAddress = baseAddress ?? Defaults.DEFAULT_URI;
        KeepAlive = keepAlive ?? false;
        OnPrem = onPrem ?? false;
        Addons = addons ?? new Dictionary<string, string>();
        Headers = headers ?? new Dictionary<string, string>();

        Log.Information("DeepgramWsClientOptions", $"KeepAlive: {KeepAlive}");
        Log.Information("DeepgramWsClientOptions", $"OnPrem: {OnPrem}");
        Log.Information("DeepgramWsClientOptions", $"APIVersion: {APIVersion}");

        // user provided takes precedence
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            // then try the environment variable
            Log.Debug("DeepgramWsClientOptions", "API KEY is not set");
            ApiKey = Environment.GetEnvironmentVariable(variable: Defaults.DEEPGRAM_API_KEY) ?? "";
            if (!string.IsNullOrEmpty(ApiKey))
            {
                Log.Information("DeepgramWsClientOptions", "API KEY set from environment variable");
            } else {
                Log.Warning("DeepgramWsClientOptions", "API KEY environment variable not set");
            }
        }
        if (!OnPrem && string.IsNullOrEmpty(ApiKey))
        {
            var exStr = "Deepgram API Key is invalid";
            Log.Error("DeepgramWsClientOptions", exStr);
            throw new ArgumentException(exStr);
        }

        // base url
        Log.Debug("DeepgramWsClientOptions", $"WS BaseAddress: {BaseAddress}");

        Regex regex = new Regex(@"\b(\/v[0-9]+)\b", RegexOptions.IgnoreCase);
        if (!regex.IsMatch(BaseAddress))
        {
            Log.Information("DeepgramWsClientOptions", $"WS BaseAddress does not contain API version: {BaseAddress}");
            BaseAddress = $"{BaseAddress}/{APIVersion}";
            Log.Debug("DeepgramWsClientOptions", $"BaseAddress: {BaseAddress}");
        }

        //checks for http:// https:// http https - https:// is include to ensure it is all stripped out and correctly formatted 
        regex = new Regex(@"\b(http:\/\/|https:\/\/|http|https)\b", RegexOptions.IgnoreCase);
        if (regex.IsMatch(BaseAddress))
        {
            Log.Information("DeepgramWsClientOptions", $"BaseAddress contains HTTP(s) protocol. Remove: {BaseAddress}");
            BaseAddress = BaseAddress.Substring(BaseAddress.IndexOf("/") + 2);
            Log.Debug("DeepgramWsClientOptions", $"BaseAddress: {BaseAddress}");
        }

        //checks for ws:// wss:// ws wss - wss:// is include to ensure it is all stripped out and correctly formatted
        regex = new Regex(@"\b(ws:\/\/|wss:\/\/|ws|wss)\b", RegexOptions.IgnoreCase);
        if (!regex.IsMatch(BaseAddress))
        {
            Log.Information("DeepgramWsClientOptions", $"BaseAddress does not contain protocol: {BaseAddress}");
            BaseAddress = $"wss://{BaseAddress}";
            Log.Debug("DeepgramWsClientOptions", $"BaseAddress: {BaseAddress}");
        }
        BaseAddress = BaseAddress.TrimEnd('/');

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

        Log.Information("DeepgramWsClientOptions", $"BaseAddress: {BaseAddress}");
        Log.Verbose("DeepgramWsClientOptions", "LEAVE");
    }
}
