// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.WebSocket;

/// <summary>
/// (PREVIEW) Connection options for the Deepgram Flux (v2 listen) WebSocket API.
/// These are sent as query parameters on the wss://api.deepgram.com/v2/listen connection.
/// <see href="https://developers.deepgram.com/docs/flux/quickstart"/>
/// </summary>
public class FluxSchema
{
    /// <summary>
    /// REQUIRED. The Flux model to use: "flux-general-en" (English) or "flux-general-multi"
    /// (multilingual). Connect throws a DeepgramException when not set.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Expected encoding of the submitted audio: linear16, linear32, mulaw, alaw, opus, or
    /// ogg-opus. Required if sending non-containerized/raw audio. If sending containerized
    /// audio (WAV, Ogg, WebM), omit this parameter.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("encoding")]
    public string? Encoding { get; set; }

    /// <summary>
    /// Sample rate of the audio stream in Hz. Required if sending non-containerized/raw audio.
    /// If sending containerized audio, omit this parameter. Supported rates per the Flux docs:
    /// 8000, 16000, 24000, 44100, 48000 (16000 recommended).
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("sample_rate")]
    public int? SampleRate { get; set; }

    /// <summary>
    /// End-of-turn confidence required to finish a turn. Valid values 0.5 - 0.9. Default 0.7.
    /// Not validated client-side; the server rejects out-of-range values.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("eot_threshold")]
    public double? EotThreshold { get; set; }

    /// <summary>
    /// End-of-turn confidence required to fire an eager end-of-turn event. When set, enables
    /// the EagerEndOfTurn and TurnResumed events. Valid values 0.3 - 0.9, and must not exceed
    /// EotThreshold. Off by default. Not validated client-side.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("eager_eot_threshold")]
    public double? EagerEotThreshold { get; set; }

    /// <summary>
    /// A turn will be finished when this much time (in milliseconds) has passed after speech,
    /// regardless of end-of-turn confidence. Valid values 500 - 10000. Default 5000.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("eot_timeout_ms")]
    public int? EotTimeoutMs { get; set; }

    /// <summary>
    /// Key terms to boost recognition of. Sent as a repeated query parameter, one per term
    /// (multi-word phrases are allowed). Maximum 100 terms.
    /// <see href="https://developers.deepgram.com/docs/keyterm"/>
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("keyterm")]
    public List<string>? Keyterm { get; set; }

    /// <summary>
    /// Language hints for multilingual transcription. Only valid when Model is
    /// "flux-general-multi". Sent as a repeated query parameter, one per hint.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("language_hint")]
    public List<string>? LanguageHint { get; set; }

    /// <summary>
    /// Removes profanity from the transcript. Default false.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("profanity_filter")]
    public bool? ProfanityFilter { get; set; }

    /// <summary>
    /// Opts out requests from the Deepgram Model Improvement Program.
    /// <see href="https://developers.deepgram.com/docs/the-deepgram-model-improvement-partnership-program"/>
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("mip_opt_out")]
    public bool? MipOptOut { get; set; }

    /// <summary>
    /// Label your requests for the purpose of identification during usage reporting.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return Regex.Unescape(JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions));
    }
}
