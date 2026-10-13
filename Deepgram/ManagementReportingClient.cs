// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;

namespace Deepgram;

/// <summary>
/// Implements the latest supported version of the management reporting client.
/// </summary>
public class ManagementReportingClient : Clients.Manage.v1.ReportingClient
{
    public ManagementReportingClient(string apiKey = "", DeepgramHttpClientOptions? deepgramClientOptions = null,
        string? httpId = null) : base(apiKey, deepgramClientOptions, httpId)
    {
    }
}
