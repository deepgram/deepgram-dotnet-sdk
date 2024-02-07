// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Manage.v1;

public record TokenDetails
{
    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("feature")]
    public string? Feature { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("input")]
    public int? Input { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("output")]
    public int? Output { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }
}
