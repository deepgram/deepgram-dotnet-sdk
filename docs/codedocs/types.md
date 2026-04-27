---
title: "Types"
description: "Key C# request and configuration models exported by the Deepgram .NET SDK."
---

This project does not export TypeScript interfaces. Instead, it exports C# classes and records that serve the same purpose: request payloads, response models, and client configuration types. The most important exported models are the option classes and the request schemas that drive each client family.

## Core configuration types

`Deepgram.Models.Authenticate.v1.DeepgramHttpClientOptions`

```csharp
public class DeepgramHttpClientOptions : IDeepgramClientOptions
{
    public string ApiKey { get; set; }
    public string AccessToken { get; set; }
    public string BaseAddress { get; set; } = Defaults.DEFAULT_URI;
    public string APIVersion { get; set; } = Defaults.DEFAULT_API_VERSION;
    public Dictionary<string, string> Headers { get; set; }
    public Dictionary<string, string> Addons { get; set; }
    public bool OnPrem { get; set; } = false;
}
```

`Deepgram.Models.Authenticate.v1.DeepgramWsClientOptions`

```csharp
public class DeepgramWsClientOptions : IDeepgramClientOptions
{
    public string ApiKey { get; set; }
    public string AccessToken { get; set; }
    public string BaseAddress { get; set; } = Defaults.DEFAULT_URI;
    public string APIVersion { get; set; } = Defaults.DEFAULT_API_VERSION;
    public Dictionary<string, string> Headers { get; set; }
    public Dictionary<string, string> Addons { get; set; }
    public bool KeepAlive { get; set; } = false;
    public decimal AutoFlushReplyDelta { get; set; } = 0;
    public bool OnPrem { get; set; } = false;
    public decimal AutoFlushSpeakDelta { get; set; } = 0;
}
```

Use these types whenever you need to override host, transport behavior, or credentials beyond the default environment-variable flow.

## Speech-to-text request models

`Deepgram.Models.Listen.v1.REST.UrlSource`

```csharp
public class UrlSource(string url)
{
    public string? Url { get; set; } = url;
}
```

`Deepgram.Models.Listen.v1.REST.PreRecordedSchema`

```csharp
public class PreRecordedSchema
{
    public int? Alternatives { get; set; }
    public string? CallBack { get; set; }
    public bool? CallbackMethod { get; set; }
    public List<string>? CustomIntent { get; set; }
    public List<string>? CustomTopic { get; set; }
    public bool? DetectEntities { get; set; }
    public bool? DetectLanguage { get; set; }
    public bool? DetectTopics { get; set; }
    public bool? Diarize { get; set; }
    public string? Encoding { get; set; }
    public Dictionary<string, string>? Extra { get; set; }
    public bool? Intents { get; set; }
    public List<string>? Keywords { get; set; }
    public List<string>? Keyterm { get; set; }
    public string? Language { get; set; }
    public string? Model { get; set; }
    public bool? MultiChannel { get; set; }
    public bool? Numerals { get; set; }
    public bool? Paragraphs { get; set; }
    public bool? Punctuate { get; set; }
    public List<string>? Redact { get; set; }
    public List<string>? Replace { get; set; }
    public int? SampleRate { get; set; }
    public List<string>? Search { get; set; }
    public bool? Sentiment { get; set; }
    public bool? SmartFormat { get; set; }
    public bool? Summarize { get; set; }
    public List<string>? Tag { get; set; }
    public bool? Topics { get; set; }
}
```

`Deepgram.Models.Listen.v2.WebSocket.LiveSchema`

```csharp
public class LiveSchema
{
    public int? Alternatives { get; set; }
    public string? CallBack { get; set; }
    public int? Channels { get; set; }
    public bool? Diarize { get; set; }
    public string? Encoding { get; set; }
    public string? EndPointing { get; set; }
    public bool? InterimResults { get; set; }
    public List<string>? Keywords { get; set; }
    public List<string>? Keyterm { get; set; }
    public string? Language { get; set; }
    public string? Model { get; set; }
    public bool? NoDelay { get; set; }
    public bool? Punctuate { get; set; }
    public int? SampleRate { get; set; }
    public bool? SmartFormat { get; set; }
    public string? UtteranceEnd { get; set; }
    public bool? VadEvents { get; set; }
}
```

## Text and speech models

`Deepgram.Models.Speak.v1.REST.TextSource`

```csharp
public class TextSource(string text)
{
    public string? Text { get; set; } = text;
}
```

`Deepgram.Models.Speak.v1.REST.SpeakSchema`

```csharp
public class SpeakSchema
{
    public string? Model { get; set; }
    public string? BitRate { get; set; }
    public string? CallBack { get; set; }
    public string? CallBackMethod { get; set; }
    public string? Container { get; set; }
    public string? Encoding { get; set; }
    public string? SampleRate { get; set; }
}
```

`Deepgram.Models.Speak.v2.WebSocket.SpeakSchema`

```csharp
public class SpeakSchema
{
    public string? Model { get; set; } = "aura-asteria-en";
    public int? BitRate { get; set; }
    public string? Encoding { get; set; }
    public int? SampleRate { get; set; }
}
```

## Analyze and management models

`Deepgram.Models.Analyze.v1.AnalyzeSchema`

```csharp
public class AnalyzeSchema
{
    public string? CallBack { get; set; }
    public string? CallbackMethod { get; set; }
    public List<string>? CustomIntent { get; set; }
    public List<string>? CustomTopic { get; set; }
    public bool? Intents { get; set; }
    public string? Language { get; set; }
    public bool? Sentiment { get; set; }
    public bool? Summarize { get; set; }
    public bool? Topics { get; set; }
}
```

`Deepgram.Models.Auth.v1.GrantTokenSchema`

```csharp
public record GrantTokenSchema
{
    public int? TtlSeconds { get; set; }
}
```

`Deepgram.Models.Manage.v1.ProjectSchema`

```csharp
public class ProjectSchema
{
    public string? Name { get; set; }
    public string? Company { get; set; }
}
```

`Deepgram.Models.Manage.v1.KeySchema`

```csharp
public class KeySchema
{
    public string? Comment { get; set; }
    public List<string>? Scopes { get; set; }
    public List<string>? Tags { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public int? TimeToLiveInSeconds { get; set; }
}
```

`Deepgram.Models.SelfHosted.v1.CredentialsSchema`

```csharp
public class CredentialsSchema
{
    public string? Comment { get; set; }
    public List<string>? Scopes { get; set; }
    public string? Provider { get; set; }
}
```

## Agent models

`Deepgram.Models.Agent.v2.WebSocket.SettingsSchema`

```csharp
public class SettingsSchema
{
    public string? Type { get; } = AgentClientTypes.Settings;
    public bool? Experimental { get; set; }
    public List<string>? Tags { get; set; }
    public bool? MipOptOut { get; set; } = false;
    public Audio Audio { get; set; } = new Audio();
    public Agent Agent { get; set; } = new Agent();
}
```

`Deepgram.Models.Agent.v2.WebSocket.InjectUserMessageSchema`

```csharp
public class InjectUserMessageSchema
{
    public string? Type { get; } = AgentClientTypes.InjectUserMessage;
    public string? Content { get; set; }
}
```

`Deepgram.Models.Agent.v2.WebSocket.FunctionCallResponseSchema`

```csharp
public class FunctionCallResponseSchema
{
    public string? Type { get; } = AgentClientTypes.FunctionCallResponse;
    public string? FunctionCallId { get; set; }
    public string? Output { get; set; }
}
```

These agent models are notable because they mix strongly typed structure with dynamic provider payloads. `Provider` in `Deepgram/Models/Agent/v2/WebSocket/Provider.cs` stores extra JSON properties through `JsonExtensionData`, which lets the SDK support provider-specific fields without a separate class for every vendor.
