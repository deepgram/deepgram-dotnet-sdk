// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.ComponentModel.DataAnnotations;

namespace Deepgram.Models.Auth.v1;

public record GrantTokenSchema
{
    /// <summary>
    /// Time to live in seconds for the token. Defaults to 30 seconds.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("ttl_seconds")]
    [Range(1, 3600, ErrorMessage = "TTL must be between 1 and 3600 seconds")]
    public int? TtlSeconds { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return Regex.Unescape(JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions));
    }
}