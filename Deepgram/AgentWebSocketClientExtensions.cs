// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Clients.Interfaces.v2;
using Deepgram.Models.Agent.v2.WebSocket;

namespace Deepgram;

/// <summary>
/// Current Voice Agent control messages that are additive to the legacy client interface.
/// </summary>
public static class AgentWebSocketClientExtensions
{
    public static Task SendUpdateListen(this IAgentWebSocketClient client, AgentUpdateListenSchema message) => Send(client, message);
    public static Task SendUpdateThink(this IAgentWebSocketClient client, AgentUpdateThinkSchema message) => Send(client, message);
    public static Task SendUpdateSpeak(this IAgentWebSocketClient client, AgentUpdateSpeakSchema message) => Send(client, message);
    public static Task SendUpdatePrompt(this IAgentWebSocketClient client, AgentUpdatePromptSchema message) => Send(client, message);
    public static Task SendInjectAgentMessage(this IAgentWebSocketClient client, AgentInjectAgentMessageSchema message) => Send(client, message);
    public static Task SendFunctionCallResponse(this IAgentWebSocketClient client, AgentFunctionCallResponseSchema message) => Send(client, message);
    public static Task SendForceEndTurn(this IAgentWebSocketClient client)
    {
        if (client is null)
        {
            throw new ArgumentNullException(nameof(client));
        }

        if (client is IAgentProtocolClient protocolClient)
        {
            return protocolClient.SendForceEndTurn();
        }

        throw new NotSupportedException(
            "SendForceEndTurn requires IAgentProtocolClient so queued audio can be flushed before the control frame is sent.");
    }

    private static Task Send(IAgentWebSocketClient client, object message)
    {
        if (client is null)
        {
            throw new ArgumentNullException(nameof(client));
        }
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        Validate(message);

        var payload = Encoding.UTF8.GetBytes(message.ToString()!);
        return client.SendMessageImmediately(payload);
    }

    private static void Validate(object message)
    {
        switch (message)
        {
            case AgentUpdateListenSchema updateListen when updateListen.Listen is null || !HasListenProvider(updateListen.Listen):
                throw new ArgumentException("UpdateListen requires Listen with a valid provider.", nameof(message));
            case AgentUpdateThinkSchema updateThink when updateThink.Think is null || !HasProviderTypeAndModel(updateThink.Think):
                throw new ArgumentException("UpdateThink requires Think with provider type and model.", nameof(message));
            case AgentUpdateSpeakSchema updateSpeak when updateSpeak.Speak is null || !HasSpeakProviderType(updateSpeak.Speak):
                throw new ArgumentException("UpdateSpeak requires Speak with a provider type.", nameof(message));
            case AgentUpdatePromptSchema updatePrompt when string.IsNullOrWhiteSpace(updatePrompt.Prompt):
                throw new ArgumentException("UpdatePrompt requires Prompt.", nameof(message));
            case AgentInjectAgentMessageSchema injectAgent when string.IsNullOrWhiteSpace(injectAgent.Message):
                throw new ArgumentException("InjectAgentMessage requires Message.", nameof(message));
            case AgentFunctionCallResponseSchema functionResponse when string.IsNullOrWhiteSpace(functionResponse.Id) || string.IsNullOrWhiteSpace(functionResponse.Name) || functionResponse.Content is null:
                throw new ArgumentException("FunctionCallResponse requires Id, Name, and Content.", nameof(message));
        }
    }

    private static bool HasListenProvider(Listen listen)
    {
        using var document = JsonDocument.Parse(listen.ToString());
        if (!document.RootElement.TryGetProperty("provider", out var provider) ||
            provider.ValueKind != JsonValueKind.Object ||
            !HasStringProperty(provider, "type"))
        {
            return false;
        }

        return !provider.TryGetProperty("version", out var version) ||
            version.ValueKind != JsonValueKind.String ||
            !string.Equals(version.GetString(), "v2", StringComparison.OrdinalIgnoreCase) ||
            HasStringProperty(provider, "model");
    }

    private static bool HasSpeakProviderType(object configuration)
    {
        if (configuration is Speak { SpeakProviders: not null } speak)
        {
            return speak.SpeakProviders.Count > 0 && speak.SpeakProviders.All(HasProviderType);
        }

        return HasProviderType(configuration);
    }

    private static bool HasProviderType(object configuration)
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(configuration, JsonSerializeOptions.DefaultOptions));
        return document.RootElement.ValueKind switch
        {
            JsonValueKind.Object => HasProviderType(document.RootElement),
            JsonValueKind.Array => document.RootElement.GetArrayLength() > 0 && document.RootElement.EnumerateArray().All(HasProviderType),
            _ => false,
        };
    }

    private static bool HasProviderTypeAndModel(object configuration)
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(configuration, JsonSerializeOptions.DefaultOptions));
        return document.RootElement.ValueKind switch
        {
            JsonValueKind.Object => HasProviderTypeAndModel(document.RootElement),
            JsonValueKind.Array => document.RootElement.GetArrayLength() > 0 && document.RootElement.EnumerateArray().All(HasProviderTypeAndModel),
            _ => false,
        };
    }

    private static bool HasProviderTypeAndModel(JsonElement configuration)
    {
        return configuration.TryGetProperty("provider", out var provider) &&
            provider.ValueKind == JsonValueKind.Object &&
            HasStringProperty(provider, "type") &&
            HasStringProperty(provider, "model");
    }

    private static bool HasProviderType(JsonElement configuration)
    {
        return configuration.TryGetProperty("provider", out var provider) &&
            provider.ValueKind == JsonValueKind.Object &&
            HasStringProperty(provider, "type");
    }

    private static bool HasStringProperty(JsonElement parent, string propertyName)
    {
        return parent.TryGetProperty(propertyName, out var property) &&
            property.ValueKind == JsonValueKind.String &&
            !string.IsNullOrWhiteSpace(property.GetString());
    }
}
