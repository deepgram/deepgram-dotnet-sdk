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
/// Accepts exactly two list shapes: the documented {"variables":[...]} object and the bare
/// [...] array the live API currently returns. Any other shape (unknown envelope, non-array
/// "variables", scalar, null) throws <see cref="JsonException"/> so API contract drift fails
/// visibly instead of silently looking like a project with no variables. Always serializes to
/// the documented object shape.
/// </summary>
public class AgentVariablesResponseConverter : JsonConverter<AgentVariablesResponse>
{
    // Ensure Read is invoked for a JSON null so it throws rather than silently producing a
    // null response that is indistinguishable from an empty project.
    public override bool HandleNull => true;

    public override AgentVariablesResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            return new AgentVariablesResponse
            {
                Variables = JsonSerializer.Deserialize<List<AgentVariableResponse>>(ref reader, options),
            };
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            if (doc.RootElement.TryGetProperty("variables", out var variables) &&
                variables.ValueKind == JsonValueKind.Array)
            {
                return new AgentVariablesResponse
                {
                    Variables = variables.Deserialize<List<AgentVariableResponse>>(options),
                };
            }

            throw new JsonException(
                "Unrecognized agent variables list shape: expected an object with a \"variables\" array or a bare array.");
        }

        throw new JsonException(
            $"Unrecognized agent variables list shape: expected an object with a \"variables\" array or a bare array, got {reader.TokenType}.");
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
