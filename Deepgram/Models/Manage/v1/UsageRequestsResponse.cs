// Copyright 2021-2023 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Manage.v1;

public record UsageRequestsResponse
{
    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("page")]
    public int? Page { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("requests")]
    public IReadOnlyList<UsageRequestResponse>? Requests { get; set; }
}
