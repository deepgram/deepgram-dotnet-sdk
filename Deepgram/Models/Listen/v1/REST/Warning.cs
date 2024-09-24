// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Listen.v1.REST;

public record Warning
{
    /// <summary>
    /// Parameter sent in the request that resulted in the warning
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("parameter")]
    public string? Parameter { get; set; }

    /// <summary>
    /// The type of warning
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Returns the WarningType enum value based on the Type string
    /// </summary>
    public WarningType WarningType
    {
        get
        {
            return Type switch
            {
                "unsupported_language" => WarningType.UnsupportedLanguage,
                "unsupported_model" => WarningType.UnsupportedModel,
                "unsupported_encoding" => WarningType.UnsupportedEncoding,
                "deprecated" => WarningType.Deprecated,
                _ => WarningType.UnsupportedLanguage
            };
        }
    }

    /// <summary>
    /// The warning message
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return Regex.Unescape(JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions));
    }
}

