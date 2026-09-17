// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.Speak.REST;

/// <summary>
/// The acknowledgement returned by the Deepgram Flux (v2 speak) batch REST API when a
/// callback URL is supplied; the audio is delivered asynchronously to that URL.
/// </summary>
public record AsyncResponse
{
    /// <summary>
    /// Unique identifier for tracking the asynchronous request.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
