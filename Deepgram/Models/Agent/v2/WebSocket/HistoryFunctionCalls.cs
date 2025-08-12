// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Deepgram.Utilities;

namespace Deepgram.Models.Agent.v2.WebSocket;

public record HistoryFunctionCalls
{
    /// <summary>
    /// Message type identifier for function calls
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    public string? Type { get; set; } = "History";

    /// <summary>
    /// List of function call objects
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("function_calls")]
    public List<HistoryFunctionCall>? FunctionCalls { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return Regex.Unescape(JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions));
    }
}

public record HistoryFunctionCall
{
    /// <summary>
    /// Unique identifier for the function call
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Name of the function called
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Indicates if the call was client-side or server-side
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("client_side")]
    public bool? ClientSide { get; set; }

    /// <summary>
    /// Arguments passed to the function
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("arguments")]
    public string? Arguments { get; set; }

    /// <summary>
    /// Response received from the function
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("response")]
    public string? Response { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return Regex.Unescape(JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions));
    }
}