// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Live.v1;

public record UnhandledResponse 
{
    /// <summary>
    /// Raw JSON
    /// </summary>
    [JsonPropertyName("raw")]
    public string? Raw { get; set; } = "";

    /// <summary>
    /// Unhandled event type.
    /// </summary>
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LiveType? Type { get; set; } = LiveType.Unhandled;
}
