// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Manage.v1;

public record UsageRequestsResponse
{
    /// <summary>
    /// TODO
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("page")]
    public int? Page { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("limit")]
    public int? Limit { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("requests")]
    public IReadOnlyList<UsageRequest>? Requests { get; set; }
}
