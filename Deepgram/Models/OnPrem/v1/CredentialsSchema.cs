// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.OnPrem.v1;

public class CredentialsSchema
{
    /// <summary>
    /// comment to credentials
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("comment")]
    public string? Comment { get; set; }

    /// <summary>
    /// scopes of credentials
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("scopes")]
    public List<string>? Scopes { get; set; }

    /// <summary>
    /// provider
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("provider")]
    public string? Provider { get; set; }
}
