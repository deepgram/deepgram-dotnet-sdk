// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Authenticate.v1;

/// <summary>
/// Configuration for the Deepgram client
/// </summary>
public class DeepgramWsClientOptions : DeepgramClientOptions
{
    public DeepgramWsClientOptions(string? apiKey = null, string? baseAddress = null, bool? keepAlive = null, bool? onPrem = null, Dictionary<string, string>? headers = null, string? apiVersion = null)
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
            //Console.WriteLine($"WS BaseAddress: {BaseAddress}"); // TODO: logging
            BaseAddress = $"{BaseAddress}/{APIVersion}";
        }
        // TODO: log

        //checks for ws:// wss:// ws wss - wss:// is include to ensure it is all stripped out and correctly formatted
        regex = new Regex(@"\b(http:\/\/|https:\/\/|http|https)\b", RegexOptions.IgnoreCase);
        if (regex.IsMatch(BaseAddress))
        {
            // if protocol https/http is in the address, remove it
            //Console.WriteLine($"WS BaseAddress (Remove https/http): {BaseAddress}"); // TODO: logging
            BaseAddress = BaseAddress.Substring(BaseAddress.IndexOf("/") + 2);
        }

        //checks for ws:// wss:// ws wss - wss:// is include to ensure it is all stripped out and correctly formatted
        regex = new Regex(@"\b(ws:\/\/|wss:\/\/|ws|wss)\b", RegexOptions.IgnoreCase);
        if (!regex.IsMatch(BaseAddress))
        {
            // if no protocol in the address then https:// is added
            //Console.WriteLine($"WS BaseAddress (Add wss/ws): {BaseAddress}"); // TODO: logging
            BaseAddress = $"wss://{BaseAddress}";
        }

        BaseAddress = BaseAddress.TrimEnd('/');
        //Console.WriteLine($"WS BaseAddress (Final): {BaseAddress}"); // TODO: logging
    }
}
