﻿// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Speak.v1;

public record SyncResponse
{
    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("content_type")]
    public string? ContentType { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("model_uuid")]
    public string? ModelUUID { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("model_name")]
    public string? ModelName { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("characters")]
    public int? Characters { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("transfer_encoding")]
    public string? TransferEncoding { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("date")]
    public DateTime? Date { get; set; }

    /// <summary>
    /// A stream of the audio file
    /// </summary>
    public MemoryStream? Stream { get; set; }

    /// <summary>
    /// The filename of the audio file
    /// </summary>
    public string? Filename { get; set; }
}
