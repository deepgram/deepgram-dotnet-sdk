// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Authenticate.v1;

/// <summary>
/// Configuration for the Deepgram client
/// </summary>
public class DeepgramHttpClientOptions
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
    // Constructor
    /*****************************/
    public DeepgramHttpClientOptions(string? apiKey = null, string? baseAddress = null, bool? keepAlive = null, bool? onPrem = null, Dictionary<string, string>? headers = null, string? apiVersion = null)
    {
        Log.Verbose("DeepgramHttpClientOptions", "ENTER");
        Log.Debug("DeepgramHttpClientOptions", apiKey == null ? "API KEY is null" : "API KEY provided");
        Log.Debug("DeepgramHttpClientOptions", baseAddress == null ? "BaseAddress is null" : "BaseAddress provided");
        Log.Debug("DeepgramHttpClientOptions", keepAlive == null ? "KeepAlive is null" : "KeepAlive provided");
        Log.Debug("DeepgramHttpClientOptions", onPrem == null ? "OnPrem is null" : "OnPrem provided");
        Log.Debug("DeepgramHttpClientOptions", headers == null ? "Headers is null" : "Headers provided");
        Log.Debug("DeepgramHttpClientOptions", apiVersion == null ? "API Version is null" : "API Version provided");

        ApiKey = apiKey ?? "";
        BaseAddress = baseAddress ?? Defaults.DEFAULT_URI;
        KeepAlive = keepAlive ?? false;
        OnPrem = onPrem ?? false;
        Headers = headers ?? new Dictionary<string, string>();
        APIVersion = apiVersion ?? Defaults.DEFAULT_API_VERSION;

        Log.Information("DeepgramHttpClientOptions", $"KeepAlive: {KeepAlive}");
        Log.Information("DeepgramHttpClientOptions", $"OnPrem: {OnPrem}");
        Log.Information("DeepgramHttpClientOptions", $"APIVersion: {APIVersion}");

        // user provided takes precedence
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            // then try the environment variable
            Log.Debug("DeepgramHttpClientOptions", "API KEY is not set");
            ApiKey = Environment.GetEnvironmentVariable(variable: Defaults.DEEPGRAM_API_KEY) ?? "";
            if (!string.IsNullOrEmpty(ApiKey))
            {
                Log.Information("DeepgramHttpClientOptions", "API KEY set from environment variable");
            }
            else
            {
                Log.Warning("DeepgramHttpClientOptions", "API KEY environment variable not set");
            }
        }
        if (!OnPrem && string.IsNullOrEmpty(ApiKey))
        {
            var exStr = "Deepgram API Key is invalid";
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
}
