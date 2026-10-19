// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Agent.v2.WebSocket;

/// <summary>
/// Deepgram speech-to-text provider using the V1 API (Nova models).
/// Assign to Listen.Provider.
///
/// Required by the API: <c>Type</c> (defaulted). Unset properties are omitted from the payload, so
/// a missing required field is rejected by the server rather than caught locally.
/// </summary>
public record DeepgramListenProviderV1
{
    /// <summary>
    /// Provider type for speech-to-text. Always "deepgram".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    public string? Type { get; set; } = "deepgram";

    /// <summary>
    /// Specifies usage of the V1 Deepgram speech-to-text API.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("version")]
    public string? Version { get; set; } = "v1";

    /// <summary>
    /// Model to use for speech to text using the V1 API, e.g. "nova-3" or "nova-2".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Language code to use for speech-to-text. A BCP-47 language tag such as "en",
    /// or "multi" for code-switching transcription. Defaults to en-US.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>
    /// Prompt keyterm recognition to improve Keyword Recall Rate.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("keyterms")]
    public List<string>? Keyterms { get; set; }

    /// <summary>
    /// Applies smart formatting to improve transcript readability. Defaults to false.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("smart_format")]
    public bool? SmartFormat { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
