// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Speak.v1.REST;

public class SpeakSchema
{
    /// <summary>
    /// AI model used to process submitted audio
    /// <see href="https://developers.deepgram.com/docs/model">
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Bit Rate allows you to specify the bit rate of your desired audio.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("bit_rate")]
    public string? BitRate { get; set; }

    /// <summary>
    /// CallBack allows you to have your submitted audio processed asynchronously.
    /// <see href="https://developers.deepgram.com/docs/callback">
    /// default is nulls
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("callback")]
    public string? CallBack { get; set; }

    /// <summary>
    /// CallBack allows you to have your submitted audio processed asynchronously.
    /// <see href="https://developers.deepgram.com/docs/callback">
    /// default is nulls
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("callback_method")]
    public string? CallBackMethod { get; set; }

    /// <summary>
    /// Audio container format
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("container")]
    public string? Container { get; set; }

    /// <summary>
    /// Encoding allows you to specify the expected encoding of your submitted audio.
    /// <see href="https://developers.deepgram.com/docs/encoding">
    /// supported encodings <see cref="AudioEncoding"/>
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("encoding")]
    public string? Encoding { get; set; }

    /// <summary>
    /// Sample Rate allows you to specify the sample rate of your submitted audio.
    /// <see href="https://developers.deepgram.com/docs/sample-rate">
    /// only applies when Encoding has a values
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("sample_rate")]
    public string? SampleRate { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return Regex.Unescape(JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions));
    }
}
