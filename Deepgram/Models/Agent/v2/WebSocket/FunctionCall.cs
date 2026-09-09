// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Agent.v2.WebSocket;

/// <summary>
/// One function the agent is asking the client (or Deepgram) to invoke, as carried in the
/// functions[] array of a FunctionCallRequest message.
/// </summary>
public record FunctionCall
{
    /// <summary>
    /// Unique identifier for this call. Echo it back as function_call_id in FunctionCallResponse.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Name of the function to invoke, matching the name declared in Settings.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// JSON-encoded string of the arguments the agent chose for this call.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("arguments")]
    public string? Arguments { get; set; }

    /// <summary>
    /// True when the client is expected to execute the function and reply with FunctionCallResponse.
    /// False when Deepgram executes it server-side via the configured endpoint.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("client_side")]
    public bool? ClientSide { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
