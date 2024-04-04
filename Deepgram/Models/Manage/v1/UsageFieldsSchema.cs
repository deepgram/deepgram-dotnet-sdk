// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Manage.v1;

public class UsageFieldsSchema
{
    /// <summary>
    /// Start date of the requested date range. Format is YYYY-MM-DD. If a full timestamp is given, it will be truncated to a day. Dates are UTC. Defaults to the date of your first request.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("start")]
    public DateTime? Start { get; set; }

    /// <summary>
    /// End date of the requested date range. Format is YYYY-MM-DD. If a full timestamp is given, it will be truncated to a day. Dates are UTC. Defaults to the current date.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("end")]
    public DateTime? End { get; set; }
}
