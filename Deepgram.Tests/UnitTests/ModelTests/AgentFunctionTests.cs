// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Agent.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ModelTests;

public class AgentFunctionTests
{
    [Test]
    public void Function_Parameters_Should_Support_Multiple_Named_Properties()
    {
        // Regression coverage for #367: a function definition must be able to declare more than
        // one parameter, with arbitrary names, via the JSON-Schema dictionary on Parameters.
        var function = new Function
        {
            Name = "get_weather",
            Description = "Get the weather for a city on a date",
            Parameters = new Dictionary<string, object>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object>
                {
                    ["city"] = new Dictionary<string, object> { ["type"] = "string", ["description"] = "City name" },
                    ["date"] = new Dictionary<string, object> { ["type"] = "string", ["description"] = "ISO 8601 date" },
                    ["units"] = new Dictionary<string, object> { ["type"] = "string", ["description"] = "metric or imperial" },
                },
                ["required"] = new List<string> { "city", "date" },
            },
        };

        var json = JsonSerializer.Serialize(function);
        using var doc = JsonDocument.Parse(json);
        var properties = doc.RootElement.GetProperty("parameters").GetProperty("properties");

        using (new AssertionScope())
        {
            properties.EnumerateObject().Select(p => p.Name)
                .Should().BeEquivalentTo(new[] { "city", "date", "units" });
            properties.GetProperty("city").GetProperty("type").GetString().Should().Be("string");
            properties.GetProperty("units").GetProperty("description").GetString().Should().Be("metric or imperial");
            doc.RootElement.GetProperty("parameters").GetProperty("required")
                .EnumerateArray().Select(e => e.GetString())
                .Should().BeEquivalentTo(new[] { "city", "date" });
        }
    }
}
