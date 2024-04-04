// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Manage.v1;

public record TokenDetails
{
    /// <summary>
    /// Feature name
    /// </summary>
    [JsonPropertyName("feature")]
    public string? Feature { get; set; }

    /// <summary>
    /// Input tokens
    /// </summary>
    [JsonPropertyName("input")]
    public int? Input { get; set; }

    /// <summary>
    /// Output tokens
    /// </summary>
    [JsonPropertyName("output")]
    public int? Output { get; set; }

    /// <summary>
    /// Model name
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }
}
