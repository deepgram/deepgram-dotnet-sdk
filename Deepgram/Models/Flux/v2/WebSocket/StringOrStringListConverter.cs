// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.v2.WebSocket;

/// <summary>
/// (PREVIEW) Reads a JSON value that may be either a single string or an array of strings
/// (the Flux API's "keyterms" property is a union of both) into a List&lt;string&gt;.
/// Always writes an array.
/// </summary>
internal class StringOrStringListConverter : JsonConverter<List<string>>
{
    public override List<string>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.String:
                return new List<string> { reader.GetString()! };
            case JsonTokenType.StartArray:
                var list = new List<string>();
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    if (reader.TokenType == JsonTokenType.String)
                    {
                        list.Add(reader.GetString()!);
                    }
                    else
                    {
                        throw new JsonException("Expected a string element in the string array.");
                    }
                }
                return list;
            default:
                throw new JsonException("Expected a string or an array of strings.");
        }
    }

    public override void Write(Utf8JsonWriter writer, List<string> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var item in value)
        {
            writer.WriteStringValue(item);
        }
        writer.WriteEndArray();
    }
}
