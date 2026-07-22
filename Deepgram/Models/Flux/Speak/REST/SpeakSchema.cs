// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.Speak.REST;

/// <summary>
/// (PREVIEW) Query options for the Deepgram Flux (v2 speak) batch text-to-speech REST API
/// (POST /v2/speak). The text itself is sent in the request body via <see cref="TextSource"/>.
/// </summary>
public class SpeakSchema
{
    /// <summary>
    /// REQUIRED. The Flux TTS model, in the form flux-{voice}-{language} (e.g. "flux-alexis-en").
    /// Only flux models are accepted; unlike /v1/speak there is no default.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Encoding of the output audio: linear16, flac, mulaw, alaw, mp3 (default), opus, or aac.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("encoding")]
    public string? Encoding { get; set; }

    /// <summary>
    /// Container/file-format wrapper for the output audio: none, wav (default), or ogg
    /// (opus uses ogg). Availability depends on the encoding.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("container")]
    public string? Container { get; set; }

    /// <summary>
    /// Output sample rate in Hz. Valid values depend on the encoding (e.g. linear16 supports
    /// 8000, 16000, 24000, 32000, 44100, 48000). Sent as an integer; the wire rejects a decimal.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("sample_rate")]
    public int? SampleRate { get; set; }

    /// <summary>
    /// Bit rate of the output audio in bits per second, for compressed encodings (mp3, opus, aac).
    /// Sent as an integer; the wire rejects a decimal.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("bit_rate")]
    public int? BitRate { get; set; }

    /// <summary>
    /// Callback URL to have the request processed asynchronously. When supplied, the API returns
    /// an <see cref="AsyncResponse"/> ack ({request_id}) and delivers the audio to this URL.
    /// <see href="https://developers.deepgram.com/docs/callback"/>
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("callback")]
    public string? CallBack { get; set; }

    /// <summary>
    /// HTTP method used for the callback: POST (default) or PUT.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("callback_method")]
    public string? CallBackMethod { get; set; }

    /// <summary>
    /// Label your requests for the purpose of identification during usage reporting. Sent as a
    /// repeated query parameter, one per tag.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("tag")]
    public List<string>? Tag { get; set; }

    /// <summary>
    /// Opts out requests from the Deepgram Model Improvement Program. Default false.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("mip_opt_out")]
    public bool? MipOptOut { get; set; }

    /// <summary>
    /// Processing priority for asynchronous (callback) requests. The only supported value is "low".
    /// Applies only when a callback is supplied.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("priority")]
    public string? Priority { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return Regex.Unescape(JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions));
    }
}
