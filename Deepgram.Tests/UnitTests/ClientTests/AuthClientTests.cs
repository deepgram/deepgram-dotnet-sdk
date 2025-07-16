// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Auth.v1;
using Deepgram.Clients.Auth.v1;
using Deepgram.Abstractions.v1;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class AuthClientTests
{
    DeepgramHttpClientOptions _options;
    string _apiKey;

    [SetUp]
    public void Setup()
    {
        _apiKey = new Faker().Random.Guid().ToString();
        _options = new DeepgramHttpClientOptions(_apiKey)
        {
            OnPrem = true,
        };
    }

    [Test]
    public async Task GrantToken_Should_Call_PostAsync_Returning_GrantTokenResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"auth/{UriSegments.GRANTTOKEN}");
        var expectedResponse = new AutoFaker<GrantTokenResponse>().Generate();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var authClient = Substitute.For<AuthClient>(_apiKey, _options, null);

        // Mock Methods
        authClient.When(x => x.PostAsync<object, GrantTokenResponse>(Arg.Any<string>(), Arg.Any<object>())).DoNotCallBase();
        authClient.PostAsync<object, GrantTokenResponse>(url, Arg.Any<object>()).Returns(expectedResponse);

        // Act
        var result = await authClient.GrantToken();

        // Assert
        await authClient.Received().PostAsync<object, GrantTokenResponse>(url, Arg.Any<object>());
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<GrantTokenResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task GrantToken_With_Schema_Should_Call_PostAsync_Returning_GrantTokenResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"auth/{UriSegments.GRANTTOKEN}");
        var expectedResponse = new AutoFaker<GrantTokenResponse>().Generate();
        var grantTokenSchema = new GrantTokenSchema { TtlSeconds = 300 };

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var authClient = Substitute.For<AuthClient>(_apiKey, _options, null);

        // Mock Methods
        authClient.When(x => x.PostAsync<GrantTokenSchema, GrantTokenResponse>(Arg.Any<string>(), Arg.Any<GrantTokenSchema>())).DoNotCallBase();
        authClient.PostAsync<GrantTokenSchema, GrantTokenResponse>(url, Arg.Any<GrantTokenSchema>()).Returns(expectedResponse);

        // Act
        var result = await authClient.GrantToken(grantTokenSchema);

        // Assert
        await authClient.Received().PostAsync<GrantTokenSchema, GrantTokenResponse>(url, Arg.Any<GrantTokenSchema>());
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<GrantTokenResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task GrantToken_With_Schema_Should_Pass_TtlSeconds_Parameter()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"auth/{UriSegments.GRANTTOKEN}");
        var expectedResponse = new AutoFaker<GrantTokenResponse>().Generate();
        var grantTokenSchema = new GrantTokenSchema { TtlSeconds = 1800 };

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var authClient = Substitute.For<AuthClient>(_apiKey, _options, null);

        // Mock Methods
        authClient.When(x => x.PostAsync<GrantTokenSchema, GrantTokenResponse>(Arg.Any<string>(), Arg.Any<GrantTokenSchema>())).DoNotCallBase();
        authClient.PostAsync<GrantTokenSchema, GrantTokenResponse>(url, Arg.Any<GrantTokenSchema>()).Returns(expectedResponse);

        // Act
        var result = await authClient.GrantToken(grantTokenSchema);

        // Assert
        await authClient.Received().PostAsync<GrantTokenSchema, GrantTokenResponse>(url, Arg.Is<GrantTokenSchema>(x => x.TtlSeconds == 1800));
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<GrantTokenResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task GrantToken_With_Null_Schema_Should_Throw_ArgumentNullException()
    {
        // Input and Output
        GrantTokenSchema? schema = null;
        var authClient = new AuthClient(_apiKey, _options, null);

        // Act & Assert
        var exception = await authClient.Invoking(y => y.GrantToken(schema!))
            .Should().ThrowAsync<ArgumentNullException>();
        exception.And.ParamName.Should().Be("grantTokenSchema");
    }

    [Test]
    public void GrantTokenSchema_Should_Have_Correct_Default_Values()
    {
        // Act
        var schema = new GrantTokenSchema();

        // Assert
        using (new AssertionScope())
        {
            schema.TtlSeconds.Should().BeNull();
        }
    }

    [Test]
    public void GrantTokenSchema_Should_Accept_Valid_TtlSeconds_Values()
    {
        // Test minimum value
        var schema1 = new GrantTokenSchema { TtlSeconds = 1 };
        schema1.TtlSeconds.Should().Be(1);

        // Test maximum value
        var schema2 = new GrantTokenSchema { TtlSeconds = 3600 };
        schema2.TtlSeconds.Should().Be(3600);

        // Test typical value
        var schema3 = new GrantTokenSchema { TtlSeconds = 300 };
        schema3.TtlSeconds.Should().Be(300);
    }

    [Test]
    public void GrantTokenSchema_ToString_Should_Return_Valid_Json()
    {
        // Input and Output
        var schema = new GrantTokenSchema { TtlSeconds = 600 };

        // Act
        var result = schema.ToString();

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().Contain("ttl_seconds");
            result.Should().Contain("600");

            // Verify it's valid JSON by parsing it
            var parsed = JsonDocument.Parse(result);
            parsed.RootElement.GetProperty("ttl_seconds").GetInt32().Should().Be(600);
        }
    }

    [Test]
    public void GrantTokenSchema_ToString_Should_Return_Valid_Json_When_TtlSeconds_Is_Null()
    {
        // Input and Output
        var schema = new GrantTokenSchema { TtlSeconds = null };

        // Act
        var result = schema.ToString();

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().Be("{}");

            // Verify it's valid JSON by parsing it
            var parsed = JsonDocument.Parse(result);
            parsed.RootElement.TryGetProperty("ttl_seconds", out _).Should().BeFalse();
        }
    }

    [Test]
    public async Task GrantToken_With_Schema_Should_Handle_CancellationToken()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"auth/{UriSegments.GRANTTOKEN}");
        var expectedResponse = new AutoFaker<GrantTokenResponse>().Generate();
        var grantTokenSchema = new GrantTokenSchema { TtlSeconds = 300 };
        var cancellationTokenSource = new CancellationTokenSource();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var authClient = Substitute.For<AuthClient>(_apiKey, _options, null);

        // Mock Methods
        authClient.When(x => x.PostAsync<GrantTokenSchema, GrantTokenResponse>(Arg.Any<string>(), Arg.Any<GrantTokenSchema>(), Arg.Any<CancellationTokenSource>())).DoNotCallBase();
        authClient.PostAsync<GrantTokenSchema, GrantTokenResponse>(url, Arg.Any<GrantTokenSchema>(), Arg.Any<CancellationTokenSource>()).Returns(expectedResponse);

        // Act
        var result = await authClient.GrantToken(grantTokenSchema, cancellationTokenSource);

        // Assert
        await authClient.Received().PostAsync<GrantTokenSchema, GrantTokenResponse>(url, Arg.Any<GrantTokenSchema>(), Arg.Any<CancellationTokenSource>());
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<GrantTokenResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task GrantToken_With_Schema_Should_Handle_Custom_Headers()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"auth/{UriSegments.GRANTTOKEN}");
        var expectedResponse = new AutoFaker<GrantTokenResponse>().Generate();
        var grantTokenSchema = new GrantTokenSchema { TtlSeconds = 300 };
        var customHeaders = new Dictionary<string, string> { { "X-Custom-Header", "test-value" } };

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var authClient = Substitute.For<AuthClient>(_apiKey, _options, null);

        // Mock Methods
        authClient.When(x => x.PostAsync<GrantTokenSchema, GrantTokenResponse>(Arg.Any<string>(), Arg.Any<GrantTokenSchema>(), Arg.Any<CancellationTokenSource>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>())).DoNotCallBase();
        authClient.PostAsync<GrantTokenSchema, GrantTokenResponse>(url, Arg.Any<GrantTokenSchema>(), Arg.Any<CancellationTokenSource>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>()).Returns(expectedResponse);

        // Act
        var result = await authClient.GrantToken(grantTokenSchema, null, null, customHeaders);

        // Assert
        await authClient.Received().PostAsync<GrantTokenSchema, GrantTokenResponse>(url, Arg.Any<GrantTokenSchema>(), Arg.Any<CancellationTokenSource>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>());
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<GrantTokenResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task GrantToken_With_Schema_Should_Handle_Addons()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"auth/{UriSegments.GRANTTOKEN}");
        var expectedResponse = new AutoFaker<GrantTokenResponse>().Generate();
        var grantTokenSchema = new GrantTokenSchema { TtlSeconds = 300 };
        var addons = new Dictionary<string, string> { { "addon1", "value1" } };

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var authClient = Substitute.For<AuthClient>(_apiKey, _options, null);

        // Mock Methods
        authClient.When(x => x.PostAsync<GrantTokenSchema, GrantTokenResponse>(Arg.Any<string>(), Arg.Any<GrantTokenSchema>(), Arg.Any<CancellationTokenSource>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>())).DoNotCallBase();
        authClient.PostAsync<GrantTokenSchema, GrantTokenResponse>(url, Arg.Any<GrantTokenSchema>(), Arg.Any<CancellationTokenSource>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>()).Returns(expectedResponse);

        // Act
        var result = await authClient.GrantToken(grantTokenSchema, null, addons);

        // Assert
        await authClient.Received().PostAsync<GrantTokenSchema, GrantTokenResponse>(url, Arg.Any<GrantTokenSchema>(), Arg.Any<CancellationTokenSource>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>());
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<GrantTokenResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }
}