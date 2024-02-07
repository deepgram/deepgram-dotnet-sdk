// Copyright 2021-2023 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.PreRecorded.v1;

public record Warning
{
    /// <summary>
    /// Parameter sent in the request that resulted in the warning
    /// </summary>
    [JsonPropertyName("parameter")]
    public string? Parameter { get; set; }

    /// <summary>
    /// The type of warning
    /// </summary>
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WarningType? Type { get; set; }

    /// <summary>
    /// The warning message
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

