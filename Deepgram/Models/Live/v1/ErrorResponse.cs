// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Live.v1;

public record ErrorResponse
{
    /// <summary>
    /// Error Description
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; } = "";

    /// <summary>
    /// Error Message
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; } = "";

    /// <summary>
    /// Error Variant
    /// </summary>
    [JsonPropertyName("variant")]
    public string? Variant { get; set; } = "";

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LiveType? Type { get; set; } = LiveType.Error;
}
