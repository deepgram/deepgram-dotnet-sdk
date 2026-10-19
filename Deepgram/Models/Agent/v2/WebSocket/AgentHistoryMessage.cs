// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Agent.v2.WebSocket;

/// <summary>
/// One entry of the conversation history supplied through <see cref="AgentContext"/>.
/// A history message is either conversation text (Role and Content) or a record of
/// function calls (FunctionCalls); use the factory methods rather than setting both.
/// </summary>
public record AgentHistoryMessage
{
    /// <summary>
    /// Message type identifier. Always "History".
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; } = "History";

    /// <summary>
    /// Identifies who spoke the statement. Either "user" or "assistant".
    /// Set for conversation text only.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    /// <summary>
    /// The actual statement that was spoken. Set for conversation text only.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>
    /// Client-side or server-side function call requests and responses.
    /// Set for function-call history only.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("function_calls")]
    public List<HistoryFunctionCall>? FunctionCalls { get; set; }

    /// <summary>
    /// Builds a conversation-text history entry.
    /// </summary>
    /// <param name="role">Either "user" or "assistant".</param>
    /// <param name="content">The statement that was spoken.</param>
    public static AgentHistoryMessage FromConversation(string role, string content)
    {
        return new AgentHistoryMessage { Role = role, Content = content };
    }

    /// <summary>
    /// Builds a function-call history entry.
    /// </summary>
    public static AgentHistoryMessage FromFunctionCalls(IEnumerable<HistoryFunctionCall> functionCalls)
    {
        return new AgentHistoryMessage { FunctionCalls = functionCalls.ToList() };
    }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
