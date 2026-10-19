// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Agent.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class AgentTypedProviderTests
{
    private static JsonElement Serialize(object value)
    {
        return JsonDocument.Parse(JsonSerializer.Serialize(value, JsonSerializeOptions.DefaultOptions)).RootElement;
    }

    private static JsonElement ProviderOf(SettingsSchema settings, string section)
    {
        var root = Serialize(settings);
        return root.GetProperty("agent").GetProperty(section).GetProperty("provider");
    }

    // ---------- the mechanism ----------

    [Test]
    public void Typed_Provider_Assigned_To_Dynamic_Should_Serialize_By_Runtime_Type()
    {
        // Listen.Provider is declared dynamic. The whole typed-provider approach rests on
        // System.Text.Json serializing the runtime type rather than the declared one.
        var settings = new SettingsSchema
        {
            Agent = new Agent
            {
                Listen = new Listen
                {
                    Provider = new DeepgramListenProviderV2 { Model = "flux-general-en" },
                },
            },
        };

        var provider = ProviderOf(settings, "listen");

        using (new AssertionScope())
        {
            provider.GetProperty("type").GetString().Should().Be("deepgram");
            provider.GetProperty("version").GetString().Should().Be("v2");
            provider.GetProperty("model").GetString().Should().Be("flux-general-en");
        }
    }

    [Test]
    public void Untyped_Dynamic_Provider_Should_Still_Work()
    {
        // Back-compat: the pre-existing DynamicObject route must keep producing the same JSON.
        dynamic provider = new Provider();
        provider.Type = "deepgram";
        provider.Model = "nova-3";

        var settings = new SettingsSchema
        {
            Agent = new Agent { Listen = new Listen { Provider = provider } },
        };

        var serialized = ProviderOf(settings, "listen");

        using (new AssertionScope())
        {
            serialized.GetProperty("type").GetString().Should().Be("deepgram");
            serialized.GetProperty("model").GetString().Should().Be("nova-3");
        }
    }

    // ---------- listen providers ----------

    [Test]
    public void DeepgramListenProviderV1_Should_Default_Type_And_Version()
    {
        var json = Serialize(new DeepgramListenProviderV1());

        using (new AssertionScope())
        {
            json.GetProperty("type").GetString().Should().Be("deepgram");
            json.GetProperty("version").GetString().Should().Be("v1");
        }
    }

    [Test]
    public void DeepgramListenProviderV1_Should_Serialize_Full_Surface()
    {
        var json = Serialize(new DeepgramListenProviderV1
        {
            Model = "nova-3",
            Language = "multi",
            Keyterms = new List<string> { "Deepgram" },
            SmartFormat = true,
        });

        using (new AssertionScope())
        {
            json.GetProperty("model").GetString().Should().Be("nova-3");
            json.GetProperty("language").GetString().Should().Be("multi");
            json.GetProperty("keyterms").EnumerateArray().Single().GetString().Should().Be("Deepgram");
            json.GetProperty("smart_format").GetBoolean().Should().BeTrue();
        }
    }

    [Test]
    public void DeepgramListenProviderV2_Should_Serialize_Full_Surface()
    {
        var json = Serialize(new DeepgramListenProviderV2
        {
            Model = "flux-general-multi",
            LanguageHints = new List<string> { "en", "es" },
            EotThreshold = 0.7,
            EagerEotThreshold = 0.35,
            EotTimeoutMs = 5000,
            Keyterms = new List<string> { "Deepgram" },
        });

        using (new AssertionScope())
        {
            json.GetProperty("type").GetString().Should().Be("deepgram");
            json.GetProperty("version").GetString().Should().Be("v2");
            json.GetProperty("model").GetString().Should().Be("flux-general-multi");
            json.GetProperty("language_hints").EnumerateArray().Should().HaveCount(2);
            json.GetProperty("eot_threshold").GetDouble().Should().Be(0.7);
            json.GetProperty("eager_eot_threshold").GetDouble().Should().Be(0.35);
            json.GetProperty("eot_timeout_ms").GetInt32().Should().Be(5000);
            json.GetProperty("keyterms").EnumerateArray().Single().GetString().Should().Be("Deepgram");
        }
    }

    // ---------- think providers ----------

    [Test]
    public void AnthropicThinkProvider_Should_Serialize()
    {
        var json = Serialize(new AnthropicThinkProvider
        {
            Version = "v1",
            Model = "claude-3-5-haiku-latest",
            Temperature = 0.5,
        });

        using (new AssertionScope())
        {
            json.GetProperty("type").GetString().Should().Be("anthropic");
            json.GetProperty("model").GetString().Should().Be("claude-3-5-haiku-latest");
            json.GetProperty("temperature").GetDouble().Should().Be(0.5);
        }
    }

    [Test]
    public void OpenAiThinkProvider_Should_Serialize_Snake_Case_Type_And_ReasoningMode()
    {
        var json = Serialize(new OpenAiThinkProvider
        {
            Model = "gpt-4o-mini",
            ReasoningMode = "medium",
        });

        using (new AssertionScope())
        {
            json.GetProperty("type").GetString().Should().Be("open_ai");
            json.GetProperty("reasoning_mode").GetString().Should().Be("medium");
        }
    }

    [Test]
    public void GroqThinkProvider_Should_Serialize()
    {
        var json = Serialize(new GroqThinkProvider { Model = "openai/gpt-oss-20b", ReasoningMode = "low" });

        using (new AssertionScope())
        {
            json.GetProperty("type").GetString().Should().Be("groq");
            json.GetProperty("model").GetString().Should().Be("openai/gpt-oss-20b");
            json.GetProperty("reasoning_mode").GetString().Should().Be("low");
        }
    }

    [Test]
    public void GoogleThinkProvider_Should_Serialize()
    {
        var json = Serialize(new GoogleThinkProvider { Version = "ai-studio-v1beta", Model = "gemini-2.5-flash" });

        using (new AssertionScope())
        {
            json.GetProperty("type").GetString().Should().Be("google");
            json.GetProperty("version").GetString().Should().Be("ai-studio-v1beta");
        }
    }

    [Test]
    public void AwsBedrockThinkProvider_Should_Serialize_Nested_Credentials()
    {
        var json = Serialize(new AwsBedrockThinkProvider
        {
            Model = "anthropic/claude-3-5-haiku-20240307-v1:0",
            Temperature = 1.5,
            Credentials = new AwsCredentials
            {
                Type = "sts",
                Region = "us-east-1",
                AccessKeyId = "AKIAEXAMPLE",
                SecretAccessKey = "secret",
                SessionToken = "token",
            },
        });

        using (new AssertionScope())
        {
            json.GetProperty("type").GetString().Should().Be("aws_bedrock");
            var creds = json.GetProperty("credentials");
            creds.GetProperty("type").GetString().Should().Be("sts");
            creds.GetProperty("region").GetString().Should().Be("us-east-1");
            creds.GetProperty("access_key_id").GetString().Should().Be("AKIAEXAMPLE");
            creds.GetProperty("secret_access_key").GetString().Should().Be("secret");
            creds.GetProperty("session_token").GetString().Should().Be("token");
        }
    }

    [Test]
    public void AwsCredentials_Should_Omit_SessionToken_For_Iam()
    {
        // session_token is STS-only; an IAM credential block must not carry an empty key.
        var json = Serialize(new AwsCredentials { Type = "iam", Region = "us-east-1" });

        json.TryGetProperty("session_token", out _).Should().BeFalse();
    }

    // ---------- speak providers ----------

    [Test]
    public void DeepgramSpeakProvider_Should_Serialize_Aura_And_Flux_Variants()
    {
        var aura = Serialize(new DeepgramSpeakProvider { Version = "v1", Model = "aura-2-thalia-en", Speed = 1.1 });
        var flux = Serialize(new DeepgramSpeakProvider { Version = "v2", Model = "flux-kit-en", Expressivity = 1 });

        using (new AssertionScope())
        {
            aura.GetProperty("type").GetString().Should().Be("deepgram");
            aura.GetProperty("version").GetString().Should().Be("v1");
            aura.GetProperty("speed").GetDouble().Should().Be(1.1);
            aura.TryGetProperty("expressivity", out _).Should().BeFalse();

            flux.GetProperty("version").GetString().Should().Be("v2");
            flux.GetProperty("expressivity").GetInt32().Should().Be(1);
        }
    }

    [Test]
    public void CartesiaSpeakProvider_Should_Serialize_Nested_Voice()
    {
        var json = Serialize(new CartesiaSpeakProvider
        {
            Version = "2025-03-17",
            ModelId = "sonic-2",
            Voice = new CartesiaVoice { Mode = "id", Id = "voice-123" },
            Language = "en",
            Volume = 1.5,
        });

        using (new AssertionScope())
        {
            json.GetProperty("type").GetString().Should().Be("cartesia");
            json.GetProperty("model_id").GetString().Should().Be("sonic-2");
            json.GetProperty("voice").GetProperty("mode").GetString().Should().Be("id");
            json.GetProperty("voice").GetProperty("id").GetString().Should().Be("voice-123");
            json.GetProperty("volume").GetDouble().Should().Be(1.5);
        }
    }

    [Test]
    public void ElevenLabsSpeakProvider_Should_Serialize()
    {
        var json = Serialize(new ElevenLabsSpeakProvider { ModelId = "eleven_turbo_v2_5", Language = "en-US" });

        using (new AssertionScope())
        {
            json.GetProperty("type").GetString().Should().Be("eleven_labs");
            json.GetProperty("model_id").GetString().Should().Be("eleven_turbo_v2_5");
            json.GetProperty("language").GetString().Should().Be("en-US");
            json.TryGetProperty("language_code", out _).Should().BeFalse("language_code is deprecated and must stay opt-in");
        }
    }

    [Test]
    public void OpenAiSpeakProvider_Should_Serialize()
    {
        var json = Serialize(new OpenAiSpeakProvider { Model = "tts-1-hd", Voice = "nova" });

        using (new AssertionScope())
        {
            json.GetProperty("type").GetString().Should().Be("open_ai");
            json.GetProperty("model").GetString().Should().Be("tts-1-hd");
            json.GetProperty("voice").GetString().Should().Be("nova");
        }
    }

    [Test]
    public void AwsPollySpeakProvider_Should_Serialize_Nested_Credentials()
    {
        var json = Serialize(new AwsPollySpeakProvider
        {
            Voice = "Joanna",
            Language = "en-US",
            Engine = "generative",
            Credentials = new AwsCredentials { Type = "iam", Region = "us-east-1", AccessKeyId = "k", SecretAccessKey = "s" },
        });

        using (new AssertionScope())
        {
            json.GetProperty("type").GetString().Should().Be("aws_polly");
            json.GetProperty("voice").GetString().Should().Be("Joanna");
            json.GetProperty("engine").GetString().Should().Be("generative");
            json.GetProperty("credentials").GetProperty("region").GetString().Should().Be("us-east-1");
        }
    }

    // ---------- shared guarantees ----------

    [Test]
    public void Providers_Should_Omit_Every_Unset_Property()
    {
        // Agent Settings is strict-checked server side, so an unset field must not
        // be emitted as null.
        var cases = new object[]
        {
            new DeepgramListenProviderV1(),
            new DeepgramListenProviderV2(),
            new AnthropicThinkProvider(),
            new AwsBedrockThinkProvider(),
            new GoogleThinkProvider(),
            new GroqThinkProvider(),
            new OpenAiThinkProvider(),
            new DeepgramSpeakProvider(),
            new CartesiaSpeakProvider(),
            new ElevenLabsSpeakProvider(),
            new OpenAiSpeakProvider(),
            new AwsPollySpeakProvider(),
        };

        using (new AssertionScope())
        {
            foreach (var provider in cases)
            {
                var json = JsonSerializer.Serialize(provider, JsonSerializeOptions.DefaultOptions);
                json.Should().NotContain("null", $"{provider.GetType().Name} must omit unset properties");
            }
        }
    }

    [Test]
    public void Typed_Providers_Should_Round_Trip()
    {
        var original = new DeepgramListenProviderV2
        {
            Model = "flux-general-en",
            EotThreshold = 0.8,
            Keyterms = new List<string> { "Deepgram" },
        };

        var restored = JsonSerializer.Deserialize<DeepgramListenProviderV2>(original.ToString());

        restored.Should().BeEquivalentTo(original);
    }

    [Test]
    public void Settings_Should_Carry_Typed_Providers_For_All_Three_Sections()
    {
        var settings = new SettingsSchema
        {
            Agent = new Agent
            {
                Listen = new Listen { Provider = new DeepgramListenProviderV2 { Model = "flux-general-en" } },
                Think = new Think { Provider = new OpenAiThinkProvider { Model = "gpt-4o-mini" } },
                Speak = new Speak { Provider = new DeepgramSpeakProvider { Version = "v2", Model = "flux-kit-en" } },
            },
        };

        using (new AssertionScope())
        {
            ProviderOf(settings, "listen").GetProperty("model").GetString().Should().Be("flux-general-en");
            ProviderOf(settings, "think").GetProperty("type").GetString().Should().Be("open_ai");
            ProviderOf(settings, "speak").GetProperty("model").GetString().Should().Be("flux-kit-en");
        }
    }
}
