// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.Speak.WebSocket;

/// <summary>
/// (PREVIEW) Connection options for the Deepgram Flux (v2 speak) text-to-speech WebSocket API.
/// These are sent as query parameters on the wss://api.deepgram.com/v2/speak connection.
/// <see href="https://developers.deepgram.com/docs/flux/quickstart"/>
/// </summary>
public class SpeakSchema
{
    /// <summary>
    /// REQUIRED. The Flux TTS model to use, in the form flux-{voice}-{language}
    /// (e.g. "flux-alexis-en"). Only flux models are accepted; an Aura model string is rejected
    /// on /v2/speak (use /v1/speak for Aura voices). Connect throws a DeepgramException when not set.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Encoding of the raw output audio: linear16 (default), mulaw, or alaw. The streaming
    /// WebSocket emits raw (non-containerized) audio; compressed/containerized encodings are
    /// available on the batch REST transport only.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("encoding")]
    public string? Encoding { get; set; }

    /// <summary>
    /// Output sample rate in Hz. With linear16, valid values are 8000, 16000, 24000, 32000, 44100,
    /// and 48000. With mulaw or alaw, valid values are 8000 and 16000. Defaults to the model's
    /// native sample rate. Not validated client-side; the server rejects unsupported values.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("sample_rate")]
    public int? SampleRate { get; set; }

    /// <summary>
    /// Opts out requests from the Deepgram Model Improvement Program. Default false.
    /// <see href="https://developers.deepgram.com/docs/the-deepgram-model-improvement-partnership-program"/>
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("mip_opt_out")]
    public bool? MipOptOut { get; set; }

    /// <summary>
    /// Label your requests for the purpose of identification during usage reporting. Sent as a
    /// repeated query parameter, one per tag.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("tag")]
    public List<string>? Tag { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
