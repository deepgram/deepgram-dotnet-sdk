// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Agent.v2.WebSocket;

public record ListenUpdatedResponse
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AgentType? Type { get; } = AgentType.ListenUpdated;
}

public record ThinkUpdatedResponse
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AgentType? Type { get; } = AgentType.ThinkUpdated;
}

public record FunctionCallCancelledResponse
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AgentType? Type { get; } = AgentType.FunctionCallCancelled;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("functions")]
    public List<CancelledFunctionCall>? Functions { get; set; }
}

/// <summary>
/// A function call result produced by the Agent after a server-side function invocation.
/// </summary>
public record FunctionCallResponse
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AgentType? Type { get; } = AgentType.FunctionCallResponse;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}

/// <summary>
/// An arbitrary JSON response forwarded from a custom Think provider.
/// </summary>
public record CustomFromThinkProviderResponse
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("content")]
    public JsonElement Content { get; set; }
}

public record CancelledFunctionCall
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public record LatencyReportResponse
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AgentType? Type { get; } = AgentType.LatencyReport;

    [JsonPropertyName("stt_latency")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? SttLatency { get; set; }

    [JsonPropertyName("ttt_token_latency")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? TttTokenLatency { get; set; }

    [JsonPropertyName("ttt_text_latency")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? TttTextLatency { get; set; }

    [JsonPropertyName("ttt_tool_latency")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? TttToolLatency { get; set; }

    [JsonPropertyName("ttt_thinking_latency")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? TttThinkingLatency { get; set; }

    [JsonPropertyName("tts_latency")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? TtsLatency { get; set; }

    [JsonPropertyName("total_latency")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? TotalLatency { get; set; }
}

public record AgentWarningResponse
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AgentType? Type { get; } = AgentType.Warning;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

public record AgentHistoryResponse
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AgentType? Type { get; } = AgentType.History;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("function_calls")]
    public List<HistoryFunctionCall>? FunctionCalls { get; set; }
}

public record HistoryFunctionCall
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("client_side")]
    public bool? ClientSide { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("arguments")]
    public string? Arguments { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("response")]
    public string? Response { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("thought_signature")]
    public string? ThoughtSignature { get; set; }
}
