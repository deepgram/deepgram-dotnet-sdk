// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Reflection;
using System.Text.Json.Serialization;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class ApiSpecParityContractTests
{
    [Test]
    public void Current_Parity_Contract_Should_Expose_Each_Recorded_Public_Wire_Field()
    {
        var fixturePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "Fixtures",
            "ApiParity",
            "current-contract.json");
        using var document = JsonDocument.Parse(File.ReadAllText(fixturePath));

        foreach (var contract in document.RootElement.GetProperty("contracts").EnumerateArray())
        {
            var typeName = contract.GetProperty("type").GetString();
            var propertyName = contract.GetProperty("property").GetString();
            var wireName = contract.GetProperty("wireName").GetString();

            var type = typeof(ClientFactory).Assembly.GetType(typeName!);
            type.Should().NotBeNull($"the API parity manifest records {typeName}");

            var property = type!.GetProperty(propertyName!, BindingFlags.Instance | BindingFlags.Public);
            property.Should().NotBeNull($"{typeName} must keep its {propertyName} public property");

            var attribute = property!.GetCustomAttribute<JsonPropertyNameAttribute>();
            attribute.Should().NotBeNull($"{typeName}.{propertyName} must retain its wire name");
            attribute!.Name.Should().Be(wireName);
        }
    }
}
