// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using NSubstitute;
using System.Text.Json;
using Deepgram.Models.Agent.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class AgentSpeakTests
{
    [SetUp]
    public void Setup()
    {
    }

    #region Backward Compatibility Tests

    [Test]
    public void Speak_SingleProvider_Should_Maintain_Backward_Compatibility()
    {
        // Input and Output
        var provider = new Provider();
        provider.Type = "deepgram";

        var endpoint = new Endpoint
        {
            URL = "https://api.deepgram.com/v1/speak",
            Headers = new Dictionary<string, string> { { "authorization", "Bearer test-key" } }
        };

        var speak = new Speak
        {
            Provider = provider,
            Endpoint = endpoint
        };

        // Assert
        using (new AssertionScope())
        {
            ((object)speak.Provider).Should().NotBeNull();
            ((string)speak.Provider.Type).Should().Be("deepgram");
            speak.Endpoint.Should().NotBeNull();
            speak.SpeakProviders.Should().BeNull();
        }
    }

    [Test]
    public void Speak_SingleProvider_ToString_Should_Return_Valid_Json()
    {
        // Input and Output
        var provider = new Provider();
        provider.Type = "deepgram";

        var speak = new Speak
        {
            Provider = provider,
            Endpoint = null
        };

        // Act
        var result = speak.ToString();

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().Contain("provider");

            // Verify it's valid JSON by parsing it
            var parsed = JsonDocument.Parse(result);
            parsed.RootElement.GetProperty("provider").GetProperty("type").GetString().Should().Be("deepgram");
        }
    }

    #endregion

    #region Array Format Tests

    [Test]
    public void Speak_ArrayFormat_Should_Support_Multiple_Providers()
    {
        // Input and Output
        var deepgramProvider = new Provider();
        deepgramProvider.Type = "deepgram";

        var openAiProvider = new Provider();
        openAiProvider.Type = "open_ai";

        var speak = new Speak
        {
            SpeakProviders = new List<SpeakProviderConfig>
            {
                new SpeakProviderConfig { Provider = deepgramProvider },
                new SpeakProviderConfig
                {
                    Provider = openAiProvider,
                    Endpoint = new Endpoint
                    {
                        URL = "https://api.openai.com/v1/audio/speech",
                        Headers = new Dictionary<string, string> { { "authorization", "Bearer {{OPENAI_API_KEY}}" } }
                    }
                }
            }
        };

        // Assert
        using (new AssertionScope())
        {
            speak.SpeakProviders.Should().NotBeNull();
            speak.SpeakProviders.Should().HaveCount(2);
            ((string)speak.SpeakProviders![0].Provider.Type).Should().Be("deepgram");
            ((string)speak.SpeakProviders![1].Provider.Type).Should().Be("open_ai");
            speak.SpeakProviders![1].Endpoint.Should().NotBeNull();
        }
    }

    [Test]
    public void Speak_ArrayFormat_ToString_Should_Return_Valid_Array_Json()
    {
        // Input and Output
        var deepgramProvider = new Provider();
        deepgramProvider.Type = "deepgram";

        var openAiProvider = new Provider();
        openAiProvider.Type = "open_ai";

        var speak = new Speak
        {
            SpeakProviders = new List<SpeakProviderConfig>
            {
                new SpeakProviderConfig { Provider = deepgramProvider },
                new SpeakProviderConfig { Provider = openAiProvider }
            }
        };

        // Act
        var result = speak.ToString();

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().Contain("speak");
            result.Should().Contain("[");
            result.Should().Contain("]");

            // Verify it's valid JSON by parsing it
            var parsed = JsonDocument.Parse(result);
            var speakArray = parsed.RootElement.GetProperty("speak");
            speakArray.ValueKind.Should().Be(JsonValueKind.Array);
            speakArray.GetArrayLength().Should().Be(2);
        }
    }

    [Test]
    public void SpeakProviderConfig_Should_Have_Correct_Structure()
    {
        // Input and Output
        var provider = new Provider();
        provider.Type = "deepgram";

        var endpoint = new Endpoint
        {
            URL = "https://api.deepgram.com/v1/speak",
            Headers = new Dictionary<string, string> { { "authorization", "Bearer test-key" } }
        };

        var speakProviderConfig = new SpeakProviderConfig
        {
            Provider = provider,
            Endpoint = endpoint
        };

        // Assert
        using (new AssertionScope())
        {
            ((object)speakProviderConfig.Provider).Should().NotBeNull();
            ((string)speakProviderConfig.Provider.Type).Should().Be("deepgram");
            speakProviderConfig.Endpoint.Should().NotBeNull();
            speakProviderConfig.Endpoint!.URL.Should().Be("https://api.deepgram.com/v1/speak");
        }
    }

    [Test]
    public void SpeakProviderConfig_ToString_Should_Return_Valid_Json()
    {
        // Input and Output
        var provider = new Provider();
        provider.Type = "open_ai";

        var speakProviderConfig = new SpeakProviderConfig
        {
            Provider = provider,
            Endpoint = new Endpoint
            {
                URL = "https://api.openai.com/v1/audio/speech",
                Headers = new Dictionary<string, string> { { "authorization", "Bearer {{OPENAI_API_KEY}}" } }
            }
        };

        // Act
        var result = speakProviderConfig.ToString();

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().Contain("provider");
            result.Should().Contain("endpoint");

            // Verify it's valid JSON by parsing it
            var parsed = JsonDocument.Parse(result);
            parsed.RootElement.GetProperty("provider").GetProperty("type").GetString().Should().Be("open_ai");
            parsed.RootElement.GetProperty("endpoint").GetProperty("url").GetString().Should().Be("https://api.openai.com/v1/audio/speech");
        }
    }

    #endregion

    #region JSON Serialization Tests

    [Test]
    public void Speak_With_Array_Should_Serialize_Correctly_To_Match_Expected_Format()
    {
        // Input and Output - This matches your JSON example
        var deepgramProvider = new Provider();
        deepgramProvider.Type = "deepgram";
        // Assuming Provider has a Model property based on your JSON

        var openAiProvider = new Provider();
        openAiProvider.Type = "open_ai";

        var speak = new Speak
        {
            SpeakProviders = new List<SpeakProviderConfig>
            {
                new SpeakProviderConfig
                {
                    Provider = deepgramProvider
                },
                new SpeakProviderConfig
                {
                    Provider = openAiProvider,
                    Endpoint = new Endpoint
                    {
                        URL = "https://api.openai.com/v1/audio/speech",
                        Headers = new Dictionary<string, string> { { "authorization", "Bearer {{OPENAI_API_KEY}}" } }
                    }
                }
            }
        };

        // Act
        var result = speak.ToString();

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();

            // Parse and verify structure matches expected format
            var parsed = JsonDocument.Parse(result);
            var speakArray = parsed.RootElement.GetProperty("speak");
            speakArray.ValueKind.Should().Be(JsonValueKind.Array);

            var firstProvider = speakArray[0];
            firstProvider.GetProperty("provider").GetProperty("type").GetString().Should().Be("deepgram");

            var secondProvider = speakArray[1];
            secondProvider.GetProperty("provider").GetProperty("type").GetString().Should().Be("open_ai");
            secondProvider.GetProperty("endpoint").GetProperty("url").GetString().Should().Be("https://api.openai.com/v1/audio/speech");
        }
    }

    [Test]
    public void Speak_Without_Array_Should_Serialize_As_Single_Provider()
    {
        // Input and Output
        var provider = new Provider();
        provider.Type = "deepgram";

        var speak = new Speak
        {
            Provider = provider,
            Endpoint = new Endpoint { URL = "https://api.deepgram.com/v1/speak" }
        };

        // Act
        var result = speak.ToString();

        // Assert
                using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().Contain("provider");
            result.Should().Contain("endpoint");
            result.Should().NotContain("\"speak\""); // Should not contain the array property name in quotes

            // Parse and verify single provider format
            var parsed = JsonDocument.Parse(result);
            parsed.RootElement.GetProperty("provider").GetProperty("type").GetString().Should().Be("deepgram");
        }
    }

    #endregion
}