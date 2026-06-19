---
title: "Getting Started"
description: "Use the Deepgram .NET SDK to call Deepgram speech, text, management, auth, and agent APIs from .NET applications."
---

The Deepgram .NET SDK gives .NET applications a typed client for Deepgram REST and WebSocket APIs, including speech-to-text, text-to-speech, text analysis, management, auth, and voice agents.

## The Problem

- Building raw `HttpClient` and `ClientWebSocket` integrations for Deepgram means repeating authentication, URI construction, retries, and payload serialization.
- Speech workflows mix different transport styles: file upload, URL submission, live audio streaming, audio output streaming, and event-driven agent sessions.
- Authentication is easy to misconfigure when you need to support API keys, short-lived access tokens, environment variables, and self-hosted deployments.
- Production code needs more than one endpoint: most apps eventually touch transcription, TTS, analysis, or project management in the same codebase.

## The Solution

The SDK centralizes those concerns in `ClientFactory`, `DeepgramHttpClientOptions`, and `DeepgramWsClientOptions`, then exposes focused clients for each Deepgram product area. REST clients inherit common request behavior from `AbstractRestClient`, and WebSocket clients share connection, subscription, queueing, and keepalive behavior through `AbstractWebSocketClient`.

```csharp
using Deepgram;
using Deepgram.Models.Listen.v1.REST;

Library.Initialize();

var client = ClientFactory.CreateListenRESTClient();

var response = await client.TranscribeUrl(
    new UrlSource("https://dpgr.am/bueller.wav"),
    new PreRecordedSchema
    {
        Model = "nova-3",
        SmartFormat = true,
        Punctuate = true
    });

Console.WriteLine(response.Results?.Channels?[0].Alternatives?[0].Transcript);
Library.Terminate();
```

## Installation

<Tabs items={["dotnet CLI", "PackageReference", "Visual Studio", "Paket"]}>
<Tab value="dotnet CLI">

```bash
dotnet add package Deepgram
dotnet add package Deepgram.Microphone
```

</Tab>
<Tab value="PackageReference">

```xml
<ItemGroup>
  <PackageReference Include="Deepgram" Version="*" ></PackageReference>
  <PackageReference Include="Deepgram.Microphone" Version="*" ></PackageReference>
</ItemGroup>
```

</Tab>
<Tab value="Visual Studio">

```text
Right click the project -> Manage NuGet Packages -> Browse -> Deepgram
```

</Tab>
<Tab value="Paket">

```text
nuget Deepgram
nuget Deepgram.Microphone
```

</Tab>
</Tabs>

`Deepgram` targets `.NET 8.0` and `.NET Standard 2.0`. `Deepgram.Microphone` targets the same frameworks and adds a PortAudio-based microphone helper.

## Quick Start

Set `DEEPGRAM_API_KEY` first, then run the smallest working transcription example:

```csharp
using Deepgram;
using Deepgram.Models.Listen.v1.REST;

Library.Initialize();

var client = ClientFactory.CreateListenRESTClient();

var result = await client.TranscribeUrl(
    new UrlSource("https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav"),
    new PreRecordedSchema
    {
        Model = "nova-3"
    });

Console.WriteLine($"Transcript: {result.Results?.Channels?[0].Alternatives?[0].Transcript}");
Library.Terminate();
```

Expected output:

```text
Transcript: life moves pretty fast
```

## Key Features

- Typed REST clients for transcription, text analysis, auth, management, and self-hosted credential APIs
- Typed WebSocket clients for live transcription, streaming TTS, and agent conversations
- Credential resolution that prefers explicit access tokens, then explicit API keys, then environment variables
- Built-in retrying `HttpClient` configuration and shared request serialization
- Optional keepalive and autoflush behavior for long-running WebSocket sessions
- PortAudio microphone integration for live capture scenarios
- Backward-compatible wrappers for older `PreRecordedClient`, `LiveClient`, and `OnPremClient` entry points

<Cards>
  <Card title="Architecture" href="/docs/architecture">See how REST abstractions, WebSocket abstractions, and concrete clients fit together.</Card>
  <Card title="Core Concepts" href="/docs/client-factory-and-options">Understand factory creation, authentication resolution, schemas, and event subscriptions.</Card>
  <Card title="API Reference" href="/docs/api-reference/library-and-client-factory">Jump to constructors, signatures, parameters, and source-file references.</Card>
</Cards>
