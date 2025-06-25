// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;
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
    public void ClearCredentials_Should_ClearBothCredentials_WhenOnPrem()
    {
        // Arrange
        var options = new DeepgramHttpClientOptions(_apiKey, null, true, null, null, _accessToken); // OnPrem = true

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
    public void SetApiKey_Should_ThrowException_When_NullOrEmpty()
    {
        // Arrange
        var options = new DeepgramHttpClientOptions(apiKey: "temp_key");

        // Act & Assert
        Action actNull = () => options.SetApiKey(null!);
        Action actEmpty = () => options.SetApiKey("");
        Action actWhitespace = () => options.SetApiKey("   ");

        using (new AssertionScope())
        {
            actNull.Should().Throw<ArgumentException>()
                .WithMessage("API Key cannot be null or empty*")
                .And.ParamName.Should().Be("apiKey");
            actEmpty.Should().Throw<ArgumentException>()
                .WithMessage("API Key cannot be null or empty*")
                .And.ParamName.Should().Be("apiKey");
            actWhitespace.Should().Throw<ArgumentException>()
                .WithMessage("API Key cannot be null or empty*")
                .And.ParamName.Should().Be("apiKey");
        }
    }

    [Test]
    public void SetAccessToken_Should_ThrowException_When_NullOrEmpty()
    {
        // Arrange
        var options = new DeepgramHttpClientOptions(apiKey: "temp_key");

        // Act & Assert
        Action actNull = () => options.SetAccessToken(null!);
        Action actEmpty = () => options.SetAccessToken("");
        Action actWhitespace = () => options.SetAccessToken("   ");

        using (new AssertionScope())
        {
            actNull.Should().Throw<ArgumentException>()
                .WithMessage("Access Token cannot be null or empty*")
                .And.ParamName.Should().Be("accessToken");
            actEmpty.Should().Throw<ArgumentException>()
                .WithMessage("Access Token cannot be null or empty*")
                .And.ParamName.Should().Be("accessToken");
            actWhitespace.Should().Throw<ArgumentException>()
                .WithMessage("Access Token cannot be null or empty*")
                .And.ParamName.Should().Be("accessToken");
        }
    }

    [Test]
    public void ClearCredentials_Should_ThrowException_When_NotOnPrem()
    {
        // Arrange
        var options = new DeepgramHttpClientOptions(_apiKey); // OnPrem defaults to false

        // Act & Assert
        Action act = () => options.ClearCredentials();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot clear authentication credentials in non-OnPrem deployments*");
    }

    [Test]
    public void ClearCredentials_Should_Work_When_OnPrem()
    {
        // Arrange
        var options = new DeepgramHttpClientOptions(_apiKey, null, true); // OnPrem = true

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
    public void WebSocketOptions_Should_Have_SameValidationBehavior()
    {
        // Arrange
        var options = new DeepgramWsClientOptions(_apiKey);

        // Act & Assert
        Action actNullApiKey = () => options.SetApiKey(null!);
        Action actNullAccessToken = () => options.SetAccessToken(null!);
        Action actClearNonOnPrem = () => options.ClearCredentials();

        using (new AssertionScope())
        {
            actNullApiKey.Should().Throw<ArgumentException>()
                .WithMessage("API Key cannot be null or empty*");
            actNullAccessToken.Should().Throw<ArgumentException>()
                .WithMessage("Access Token cannot be null or empty*");
            actClearNonOnPrem.Should().Throw<InvalidOperationException>()
                .WithMessage("Cannot clear authentication credentials in non-OnPrem deployments*");
        }
    }

    [Test]
    public void OptionsFromEnv_Should_Have_SameValidationBehavior()
    {
        // Arrange
        Environment.SetEnvironmentVariable(Defaults.DEEPGRAM_API_KEY, _apiKey);

        try
        {
            var options = new DeepgramOptionsFromEnv();

            // Act & Assert
            Action actNullApiKey = () => options.SetApiKey(null!);
            Action actNullAccessToken = () => options.SetAccessToken(null!);
            Action actClearNonOnPrem = () => options.ClearCredentials();

            using (new AssertionScope())
            {
                actNullApiKey.Should().Throw<ArgumentException>()
                    .WithMessage("API Key cannot be null or empty*");
                actNullAccessToken.Should().Throw<ArgumentException>()
                    .WithMessage("Access Token cannot be null or empty*");
                actClearNonOnPrem.Should().Throw<InvalidOperationException>()
                    .WithMessage("Cannot clear authentication credentials in non-OnPrem deployments*");
            }
        }
        finally
        {
            Environment.SetEnvironmentVariable(Defaults.DEEPGRAM_API_KEY, null);
        }
    }

    [Test]
    public void Concurrent_CredentialOperations_Should_BeThreadSafe()
    {
        // Arrange
        var options = new DeepgramHttpClientOptions(_apiKey, null, true); // OnPrem = true for clearing
        var tasks = new List<Task>();
        var exceptions = new ConcurrentBag<Exception>();

        // Act - Perform concurrent credential operations
        for (int i = 0; i < 100; i++)
        {
            var apiKey = $"api_key_{i}";
            var accessToken = $"access_token_{i}";

            tasks.Add(Task.Run(() =>
            {
                try
                {
                    options.SetApiKey(apiKey);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                try
                {
                    options.SetAccessToken(accessToken);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                try
                {
                    options.ClearCredentials();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert
        using (new AssertionScope())
        {
            exceptions.Should().BeEmpty("Thread safety should prevent race conditions");
            // After all operations, credentials should be in a consistent state
            // Either one type of credential is set OR both are cleared (but not both set)
            var bothEmpty = options.ApiKey == "" && options.AccessToken == "";
            var onlyApiKeySet = !string.IsNullOrEmpty(options.ApiKey) && options.AccessToken == "";
            var onlyAccessTokenSet = options.ApiKey == "" && !string.IsNullOrEmpty(options.AccessToken);

            (bothEmpty || onlyApiKeySet || onlyAccessTokenSet).Should().BeTrue(
                "Credentials should be in a consistent state: either both cleared, only API key set, or only access token set. " +
                $"Current state: ApiKey='{options.ApiKey}', AccessToken='{options.AccessToken}'");
        }
    }
}