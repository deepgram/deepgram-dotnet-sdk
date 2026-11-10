// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Agent.v2.WebSocket;

/// <summary>
/// AWS credentials for the AWS Bedrock think provider and the AWS Polly speak provider.
/// Use "sts" for short-lived credentials and "iam" for long-lived ones.
///
/// Required by the API when supplied: <c>Type</c>, <c>Region</c>, <c>AccessKeyId</c> and
/// <c>SecretAccessKey</c>. <c>SessionToken</c> is for "sts" only. Unset properties are omitted from
/// the payload, so a missing required field is rejected by the server rather than caught locally.
/// </summary>
public record AwsCredentials
{
    /// <summary>
    /// Credentials type. Either "sts" or "iam".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// AWS region.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("region")]
    public string? Region { get; set; }

    /// <summary>
    /// AWS access key.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("access_key_id")]
    public string? AccessKeyId { get; set; }

    /// <summary>
    /// AWS secret access key.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("secret_access_key")]
    public string? SecretAccessKey { get; set; }

    /// <summary>
    /// AWS session token. Required for STS only.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("session_token")]
    public string? SessionToken { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
