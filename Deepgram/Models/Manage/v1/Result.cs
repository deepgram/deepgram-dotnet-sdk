// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Manage.v1;

public record Result
{
    /// <summary>
    /// Start date for included requests.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("start")]
    public DateTime? StartDateTime { get; set; }

    /// <summary>
    /// End date for included requests.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("end")]
    public DateTime? EndDateTime { get; set; }

    /// <summary>
    /// Length of time (in hours) of audio submitted in included requests.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("hours")]
    public double? Hours { get; set; }

    /// <summary>
    /// Length of time (in hours) of audio processed in included requests.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("total_hours")]
    public double? TotalHours { get; set; }

    /// <summary>
    /// Number of included requests.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("requests")]
    public int? Requests { get; set; }

    /// <summary>
    /// Token information
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("tokens")]
    public Token? Tokens { get; set; }
}

