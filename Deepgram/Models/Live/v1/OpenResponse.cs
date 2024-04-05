// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Live.v1;

public record OpenResponse
{
    /// <summary>
    /// Open event type.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LiveType? Type { get; set; } = LiveType.Open;
}
