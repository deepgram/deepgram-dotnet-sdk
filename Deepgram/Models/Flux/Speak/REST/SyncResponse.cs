// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.Speak.REST;

/// <summary>
/// (PREVIEW) The synchronous response from the Deepgram Flux (v2 speak) batch REST API. The
/// synthesized audio is exposed as a <see cref="Stream"/>; the remaining properties are populated
/// from the response headers.
/// </summary>
// [verify] The exact /v2/speak response header set is not enumerated in the OpenAPI spec; these
// mirror /v1/speak and are confirmed/adjusted against the live staging endpoint during validation.
public record SyncResponse
{
    /// <summary>
    /// Content type of the audio file.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("content_type")]
    public string? ContentType { get; set; }

    /// <summary>
    /// Request ID for support purposes.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }

    /// <summary>
    /// Model UUID.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("model_uuid")]
    public string? ModelUUID { get; set; }

    /// <summary>
    /// Model name.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("model_name")]
    public string? ModelName { get; set; }

    /// <summary>
    /// Character count.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("characters")]
    public int? Characters { get; set; }

    /// <summary>
    /// Transfer encoding of the audio file.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("transfer_encoding")]
    public string? TransferEncoding { get; set; }

    /// <summary>
    /// Date/time of the response.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("date")]
    public DateTime? Date { get; set; }

    /// <summary>
    /// A stream of the synthesized audio.
    /// </summary>
    public MemoryStream? Stream { get; set; }

    /// <summary>
    /// The filename of the audio file, when written via ToFile.
    /// </summary>
    public string? Filename { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return Regex.Unescape(JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions));
    }
}
