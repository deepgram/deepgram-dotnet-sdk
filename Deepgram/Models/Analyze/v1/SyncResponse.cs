// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Analyze.v1;

public record SyncResponse
{
    /// <summary>
    /// Metadata of response <see cref="Metadata"/>
    /// </summary>
    [JsonPropertyName("metadata")]
    public Metadata? Metadata { get; set; }

    /// <summary>
    /// Results of Response <see cref="v1.Results"/>
    /// </summary>
    [JsonPropertyName("results")]
    public Results? Results { get; set; }
}
