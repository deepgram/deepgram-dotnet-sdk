// Copyright 2021-2023 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Manage.v1;

public record Key
{
    /// <summary>
    /// Unique identifier of the Deepgram API key
    /// </summary>
    [JsonPropertyName("api_key_id")]
    public string? ApiKeyId { get; set; }

    /// <summary>
    /// Comment for the Deepgram API key
    /// </summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("scopes")]
    public IReadOnlyList<string>? Scopes { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("created")]
    public DateTime? Created { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("tags")]
    public IReadOnlyList<string>? Tags { get; set; }
}

