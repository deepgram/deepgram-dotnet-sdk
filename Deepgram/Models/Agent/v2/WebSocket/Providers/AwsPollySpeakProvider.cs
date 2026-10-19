// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Agent.v2.WebSocket;

/// <summary>
/// AWS Polly text-to-speech provider. Assign to Speak.Provider.
///
/// Required by the API: <c>Type</c> (defaulted), <c>Voice</c>, <c>Language</c>, <c>Engine</c> and
/// <c>Credentials</c>. Unset properties are omitted from the payload, so a missing required field
/// is rejected by the server rather than caught locally.
/// </summary>
public record AwsPollySpeakProvider
{
    /// <summary>
    /// Provider type. Always "aws_polly".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    public string? Type { get; set; } = "aws_polly";

    /// <summary>
    /// AWS Polly voice name, e.g. "Matthew", "Joanna", "Amy" or "Emma".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("voice")]
    public string? Voice { get; set; }

    /// <summary>
    /// Language code to use, e.g. "en-US". Corresponds to the language_code
    /// parameter in the AWS Polly API.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>
    /// Deprecated. Use Language instead.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("language_code")]
    public string? LanguageCode { get; set; }

    /// <summary>
    /// AWS Polly engine. One of "generative", "long-form", "standard" or "neural".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("engine")]
    public string? Engine { get; set; }

    /// <summary>
    /// AWS credentials, short-lived STS or long-lived IAM.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("credentials")]
    public AwsCredentials? Credentials { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
