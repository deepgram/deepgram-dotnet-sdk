// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Clients.Analyze.v1;
using Deepgram.Models.Authenticate.v1;

namespace Deepgram;

/// <summary>
/// Implements the latest supported version of the Analyze Client.
/// </summary>
public class AnalyzeClient : Client
{
    public AnalyzeClient(string apiKey, DeepgramClientOptions? deepgramClientOptions = null) : base(apiKey, deepgramClientOptions)
    {
    }
}
