// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Authenticate.v1;

/// <summary>
/// Configuration for the Deepgram client
/// </summary>
public class DeepgramHttpClientOptions : DeepgramClientOptions
{
    public DeepgramHttpClientOptions(string? apiKey = null, string? baseAddress = null, bool? keepAlive = null, bool? onPrem = null, Dictionary<string, string>? headers = null, string? apiVersion = null) : base(apiKey, baseAddress, keepAlive, onPrem, headers, apiVersion)
    {
    }
}
