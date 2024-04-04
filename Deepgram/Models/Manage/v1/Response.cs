// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Manage.v1;

public record Response
{
    /// <summary>
    /// Details of the request <see cref="Detail"/>
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("details")]
    public Details? Details { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("code")]
    public int? Code { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("completed")]
    public string? Completed { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("token_details")]
    public List<TokenDetails>? TokenDetails { get; set; }
}
