// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.AgentManage.v1;

/// <summary>
/// The list of template variables for a project. The documented shape is an object with a
/// "variables" array, but the live API currently returns a bare array; both are accepted.
/// </summary>
[JsonConverter(typeof(AgentVariablesResponseConverter))]
public record AgentVariablesResponse
{
    /// <summary>
    /// A list of agent variables for the project.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("variables")]
    public List<AgentVariableResponse>? Variables { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}

/// <summary>
/// Accepts both list shapes: the documented {"variables":[...]} object and the bare [...]
/// array the live API currently returns. Always serializes to the documented object shape.
/// </summary>
public class AgentVariablesResponseConverter : JsonConverter<AgentVariablesResponse>
{
    public override AgentVariablesResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            return new AgentVariablesResponse
            {
                Variables = JsonSerializer.Deserialize<List<AgentVariableResponse>>(ref reader, options),
            };
        }

        using var doc = JsonDocument.ParseValue(ref reader);
        if (doc.RootElement.ValueKind == JsonValueKind.Object &&
            doc.RootElement.TryGetProperty("variables", out var variables) &&
            variables.ValueKind == JsonValueKind.Array)
        {
            return new AgentVariablesResponse
            {
                Variables = variables.Deserialize<List<AgentVariableResponse>>(options),
            };
        }

        return new AgentVariablesResponse();
    }

    public override void Write(Utf8JsonWriter writer, AgentVariablesResponse value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        if (value.Variables != null)
        {
            writer.WritePropertyName("variables");
            JsonSerializer.Serialize(writer, value.Variables, options);
        }
        writer.WriteEndObject();
    }
}
