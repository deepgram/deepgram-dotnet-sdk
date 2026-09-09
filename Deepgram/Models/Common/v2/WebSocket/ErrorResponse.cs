// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Common.v2.WebSocket;

public record ErrorResponse
{
    /// <summary>
    /// Machine-readable error code, e.g. CLIENT_MESSAGE_TIMEOUT. Sent by the Voice Agent API;
    /// may be absent on other endpoints.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>
    /// Error Description
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("description")]
    public string? Description { get; set; } = "";

    /// <summary>
    /// Error Message
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("message")]
    public string? Message { get; set; } = "";

    /// <summary>
    /// Error Variant
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("variant")]
    public string? Variant { get; set; } = "";

    /// <summary>
    /// Error event type.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WebSocketType? Type { get; set; } = WebSocketType.Error;

    /// <summary>
    /// Copy method to copy the object
    /// </summary>
    public void Copy(ErrorResponse other)
    {
        if (other is null)
        {
            return;
        }

        Code = other.Code;
        Description = other.Description;
        Message = other.Message;
        Variant = other.Variant;
        Type = other.Type;
    }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
