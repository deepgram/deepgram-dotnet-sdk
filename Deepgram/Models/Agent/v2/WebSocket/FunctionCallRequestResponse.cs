// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Agent.v2.WebSocket;

/// <summary>
/// Sent by the agent when it wants one or more functions invoked. For each entry with
/// client_side = true the client must run the function and send a FunctionCallResponse
/// carrying the same id.
/// </summary>
public record FunctionCallRequestResponse
{
    /// <summary>
    /// FunctionCallRequest event type.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AgentType? Type { get; } = AgentType.FunctionCallRequest;

    /// <summary>
    /// The functions the agent wants invoked, in the order the agent requested them.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("functions")]
    public List<FunctionCall>? Functions { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
