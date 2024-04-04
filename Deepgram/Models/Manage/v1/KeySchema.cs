// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Manage.v1;

public class KeySchema
{

    /// <summary>
    /// Comment to describe key
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    /// <summary>
    /// Scopes of the key
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("scopes")]
    public List<string>? Scopes { get; set; }

    /// <summary>
    /// Tag names for key
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Date on which the key should expire if you set this do not set the TimeToLiveInSeconds
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("expiration_date")]
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Time for the key to live in seconds 
    /// if you se this do not set the ExpirationDate
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("time_to_live_in_seconds")]
    public int? TimeToLiveInSeconds { get; set; }
}
