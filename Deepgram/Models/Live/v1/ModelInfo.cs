// Copyright 2021-2023 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Live.v1;

public record ModelInfo
{
    /// <summary>
    /// Architecture of the model
    /// </summary>
    [JsonPropertyName("arch")]
    public string? Arch { get; set; }

    /// <summary>
    /// Name of the model
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Version of the model
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }
}
