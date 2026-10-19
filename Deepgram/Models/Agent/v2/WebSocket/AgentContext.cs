// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Agent.v2.WebSocket;

/// <summary>
/// Conversation context carried in Settings, letting an agent resume a prior
/// conversation instead of starting cold. Supply the history of messages and
/// function calls that happened before this session.
/// </summary>
public record AgentContext
{
    /// <summary>
    /// Conversation history as a list of messages and function calls, oldest first.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("messages")]
    public List<AgentHistoryMessage>? Messages { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
