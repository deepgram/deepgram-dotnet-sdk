// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Authenticate.v1;

/// <summary>
/// Configuration for the Deepgram client
/// </summary>
public class DeepgramClientOptions
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
    public DeepgramClientOptions(string? apiKey = null, string? baseAddress = null, bool? keepAlive = null, bool? onPrem = null, Dictionary<string, string>? headers = null, string? apiVersion = null)
    {
        ApiKey = apiKey ?? "";
        BaseAddress = baseAddress ?? Defaults.DEFAULT_URI;
        KeepAlive = keepAlive ?? false;
        OnPrem = onPrem ?? false;
        Headers = headers ?? new Dictionary<string, string>();
        APIVersion = apiVersion ?? Defaults.DEFAULT_API_VERSION;

        // user provided takes precedence
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            // then try the environment variable
            // TODO: log
            ApiKey = Environment.GetEnvironmentVariable(variable: Defaults.DEEPGRAM_API_KEY) ?? "";
            if (string.IsNullOrEmpty(ApiKey))
            {
                // TODO: log
            }
        }
        if (!OnPrem && string.IsNullOrEmpty(ApiKey))
        {
            // TODO: log
            throw new ArgumentException("Deepgram API Key is invalid");
        }

        // base url
        Regex regex = new Regex(@"\b(\/v[0-9]+)\b", RegexOptions.IgnoreCase);
        if (!regex.IsMatch(BaseAddress))
        {
            //Console.WriteLine($"REST BaseAddress: {BaseAddress}"); // TODO: logging
            BaseAddress = $"{BaseAddress}/{APIVersion}/";
        }
        // TODO: log

        //checks for http:// https:// http https - https:// is include to ensure it is all stripped out and correctly formatted 
        regex = new Regex(@"\b(http:\/\/|https:\/\/|http|https)\b", RegexOptions.IgnoreCase);
        if (!regex.IsMatch(BaseAddress))
        {
            //if no protocol in the address then https:// is added
            //Console.WriteLine($"REST BaseAddress: {BaseAddress}"); // TODO: logging
            BaseAddress = $"https://{BaseAddress}";
        }
        BaseAddress = BaseAddress.TrimEnd('/');
        //Console.WriteLine($"REST BaseAddress (Final): {BaseAddress}"); // TODO: logging
    }
}
