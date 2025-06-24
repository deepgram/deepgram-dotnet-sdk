// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;

namespace Deepgram.Tests.UnitTests.AuthenticationTests;

[TestFixture]
public class BearerTokenTests
{
    private string _apiKey;
    private string _accessToken;

    [SetUp]
    public void Setup()
    {
        _apiKey = "test_api_key_12345";
        _accessToken = "test_access_token_67890";
    }

    [Test]
    public void Should_Prioritize_ExplicitAccessToken_Over_ExplicitApiKey()
    {
        // Arrange & Act
        var options = new DeepgramHttpClientOptions(_apiKey, null, false, null, null, _accessToken);

        // Assert
        using (new AssertionScope())
        {
            options.AccessToken.Should().Be(_accessToken);
            options.ApiKey.Should().Be("");
        }
    }

    [Test]
    public void Should_Use_ExplicitApiKey_When_NoAccessToken()
    {
        // Arrange & Act
        var options = new DeepgramHttpClientOptions(_apiKey, null, false, null, null, null);

        // Assert
        using (new AssertionScope())
        {
            options.ApiKey.Should().Be(_apiKey);
            options.AccessToken.Should().Be("");
        }
    }

    [Test]
    public void Should_Prioritize_EnvironmentAccessToken_Over_EnvironmentApiKey()
    {
        // Arrange
        Environment.SetEnvironmentVariable(Defaults.DEEPGRAM_ACCESS_TOKEN, _accessToken);
        Environment.SetEnvironmentVariable(Defaults.DEEPGRAM_API_KEY, _apiKey);

        try
        {
            // Act
            var options = new DeepgramHttpClientOptions();

            // Assert
            using (new AssertionScope())
            {
                options.AccessToken.Should().Be(_accessToken);
                options.ApiKey.Should().Be("");
            }
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable(Defaults.DEEPGRAM_ACCESS_TOKEN, null);
            Environment.SetEnvironmentVariable(Defaults.DEEPGRAM_API_KEY, null);
        }
    }

    [Test]
    public void Should_Use_EnvironmentApiKey_When_NoEnvironmentAccessToken()
    {
        // Arrange
        Environment.SetEnvironmentVariable(Defaults.DEEPGRAM_ACCESS_TOKEN, null);
        Environment.SetEnvironmentVariable(Defaults.DEEPGRAM_API_KEY, _apiKey);

        try
        {
            // Act
            var options = new DeepgramHttpClientOptions();

            // Assert
            using (new AssertionScope())
            {
                options.ApiKey.Should().Be(_apiKey);
                options.AccessToken.Should().Be("");
            }
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable(Defaults.DEEPGRAM_API_KEY, null);
        }
    }

    [Test]
    public void Should_ThrowException_When_NoCredentials_And_NotOnPrem()
    {
        // Arrange
        Environment.SetEnvironmentVariable(Defaults.DEEPGRAM_ACCESS_TOKEN, null);
        Environment.SetEnvironmentVariable(Defaults.DEEPGRAM_API_KEY, null);

        try
        {
            // Act & Assert
            Action act = () => new DeepgramHttpClientOptions();
            act.Should().Throw<ArgumentException>()
                .WithMessage("Deepgram authentication is required. Please provide either an API Key or Access Token.");
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable(Defaults.DEEPGRAM_ACCESS_TOKEN, null);
            Environment.SetEnvironmentVariable(Defaults.DEEPGRAM_API_KEY, null);
        }
    }

    [Test]
    public void Should_AllowNoCredentials_When_OnPrem()
    {
        // Arrange
        Environment.SetEnvironmentVariable(Defaults.DEEPGRAM_ACCESS_TOKEN, null);
        Environment.SetEnvironmentVariable(Defaults.DEEPGRAM_API_KEY, null);

        try
        {
            // Act
            var options = new DeepgramHttpClientOptions(null, null, true);

            // Assert
            using (new AssertionScope())
            {
                options.ApiKey.Should().Be("");
                options.AccessToken.Should().Be("");
                options.OnPrem.Should().BeTrue();
            }
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable(Defaults.DEEPGRAM_ACCESS_TOKEN, null);
            Environment.SetEnvironmentVariable(Defaults.DEEPGRAM_API_KEY, null);
        }
    }

    [Test]
    public void SetAccessToken_Should_ClearApiKey_And_SetAccessToken()
    {
        // Arrange
        var options = new DeepgramHttpClientOptions(_apiKey);

        // Act
        options.SetAccessToken(_accessToken);

        // Assert
        using (new AssertionScope())
        {
            options.AccessToken.Should().Be(_accessToken);
            options.ApiKey.Should().Be("");
        }
    }

    [Test]
    public void SetApiKey_Should_ClearAccessToken_And_SetApiKey()
    {
        // Arrange
        var options = new DeepgramHttpClientOptions(null, null, false, null, null, _accessToken);

        // Act
        options.SetApiKey(_apiKey);

        // Assert
        using (new AssertionScope())
        {
            options.ApiKey.Should().Be(_apiKey);
            options.AccessToken.Should().Be("");
        }
    }

    [Test]
    public void ClearCredentials_Should_ClearBothCredentials()
    {
        // Arrange
        var options = new DeepgramHttpClientOptions(_apiKey, null, false, null, null, _accessToken);

        // Act
        options.ClearCredentials();

        // Assert
        using (new AssertionScope())
        {
            options.ApiKey.Should().Be("");
            options.AccessToken.Should().Be("");
        }
    }

    [Test]
    public void WebSocketOptions_Should_Have_SamePriorityBehavior()
    {
        // Arrange & Act
        var options = new DeepgramWsClientOptions(_apiKey, null, false, false, null, null, _accessToken);

        // Assert
        using (new AssertionScope())
        {
            options.AccessToken.Should().Be(_accessToken);
            options.ApiKey.Should().Be("");
        }
    }

    [Test]
    public void DeepgramOptionsFromEnv_Should_PrioritizeAccessToken()
    {
        // Arrange
        Environment.SetEnvironmentVariable(Defaults.DEEPGRAM_ACCESS_TOKEN, _accessToken);
        Environment.SetEnvironmentVariable(Defaults.DEEPGRAM_API_KEY, _apiKey);

        try
        {
            // Act
            var options = new DeepgramOptionsFromEnv();

            // Assert
            using (new AssertionScope())
            {
                options.AccessToken.Should().Be(_accessToken);
                options.ApiKey.Should().Be("");
            }
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable(Defaults.DEEPGRAM_ACCESS_TOKEN, null);
            Environment.SetEnvironmentVariable(Defaults.DEEPGRAM_API_KEY, null);
        }
    }

    [Test]
    public void Should_Handle_NullParameters_Gracefully()
    {
        // Arrange & Act
        var options = new DeepgramHttpClientOptions(apiKey: "temp_key"); // Provide a key to avoid constructor exception

        // These should not throw exceptions
        options.SetApiKey(null!);
        options.SetAccessToken(null!);

        // Assert
        using (new AssertionScope())
        {
            options.ApiKey.Should().Be("");
            options.AccessToken.Should().Be("");
        }
    }
}