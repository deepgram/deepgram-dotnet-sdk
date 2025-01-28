# Deepgram .NET SDK

This is just to see if CI passes.

[![NuGet](https://img.shields.io/nuget/v/deepgram)](https://www.nuget.org/packages/Deepgram) [![Build Status](https://github.com/deepgram-devs/deepgram-dotnet-sdk/workflows/CI/badge.svg)](https://github.com/deepgram-devs/deepgram-dotnet-sdk/actions?query=CI) [![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-v2.0%20adopted-ff69b4.svg)](./.github/CODE_OF_CONDUCT.md) [![Discord](https://dcbadge.vercel.app/api/server/xWRaCDBtW4?style=flat)](https://discord.gg/xWRaCDBtW4)

Official .NET SDK for [Deepgram](https://www.deepgram.com/). Power your apps with world-class speech and Language AI models.

- [Deepgram .NET SDK](#deepgram-net-sdk)
- [Documentation](#documentation)
- [Getting an API Key](#getting-an-api-key)
- [Requirements](#requirements)
- [Installation](#installation)
- [Quickstarts](#quickstarts)
  - [PreRecorded Audio Transcription Quickstart](#prerecorded-audio-transcription-quickstart)
  - [Live Audio Transcription Quickstart](#live-audio-transcription-quickstart)
- [Example Code](#example-code)
- [Logging](#logging)
- [Backwards Compatability](#backwards-compatibility)
- [Development and Contributing](#development-and-contributing)
- [Getting Help](#getting-help)
- [Backwards Compatibility](#backwards-compatibility)

# Documentation

Complete documentation of the .NET SDK can be found on the
[Deepgram Docs](https://developers.deepgram.com/docs/dotnet-sdk).

You can learn more about the full Deepgram API at [https://developers.deepgram.com](https://developers.deepgram.com).

# Getting an API Key

🔑 To access the Deepgram API, you will need a [free Deepgram API Key](https://console.deepgram.com/signup?jump=keys).

# Requirements

This SDK supports the following versions:

- .NET 8.0
- .NET 7.0
- .NET 6.0

# Installation

To install the latest version of the C# SDK using NuGet (latest means this version will guarantee change over time), run the following command from your terminal in your project's directory:

```bash
dotnet add package Deepgram
```

Or use the NuGet package Manager. Right click on project and select manage NuGet packages.

### Installing the Previous Version

We guarantee that major interfaces will not break in a given major semver (ie, `4.*` release). However, all bets are off moving from a `3.*` to `4.*` major release. This follows standard semver best-practices.

To install the previous major version of the .NET SDK, run the following command from your terminal in your project's directory:

```bash
dotnet add package Deepgram --version 3.4.2
```

# Quickstarts

This SDK aims to reduce complexity and abstract/hide some internal Deepgram details that clients shouldn't need to know about.  However, you can still tweak options and settings if you need.

## PreRecorded Audio Transcription Quickstart

You can find a [walkthrough](https://developers.deepgram.com/docs/pre-recorded-audio-transcription) on our documentation site. Transcribing Pre-Recorded Audio can be done using the following sample code:

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var deepgramClient = ClientFactory.CreateListenRESTClient();

var response = await deepgramClient.TranscribeUrl(
  new UrlSource("https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav"),
  new PreRecordedSchema()
  {
    Model = "nova-2",
  });

Console.WriteLine(response);
```

## Live Audio Transcription Quickstart

You can find a [walkthrough](https://developers.deepgram.com/docs/live-streaming-audio-transcription) on our documentation site. Transcribing Live Audio can be done using the following sample code:

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var liveClient = ClientFactory.CreateListenWebSocketClient();

// Subscribe to the EventResponseReceived event
liveClient.Subscribe(new EventHandler<OpenResponse>((sender, e) =>
{
    Console.WriteLine($"\n\n----> {e.Type} received");
}));
liveClient.Subscribe(new EventHandler<MetadataResponse>((sender, e) =>
{
    Console.WriteLine($"----> {e.Type} received");
}));
liveClient.Subscribe(new EventHandler<ResultResponse>((sender, e) =>
{
    Console.WriteLine($"----> Speaker: {e.Channel.Alternatives[0].Transcript}");
}));
liveClient.Subscribe(new EventHandler<SpeechStartedResponse>((sender, e) =>
{
    Console.WriteLine($"----> {e.Type} received");
}));
liveClient.Subscribe(new EventHandler<UtteranceEndResponse>((sender, e) =>
{
    Console.WriteLine($"----> {e.Type} received");
}));
liveClient.Subscribe(new EventHandler<CloseResponse>((sender, e) =>
{
    Console.WriteLine($"----> {e.Type} received");
}));
liveClient.Subscribe(new EventHandler<UnhandledResponse>((sender, e) =>
{
    Console.WriteLine($"----> {e.Type} received");
}));
liveClient.Subscribe(new EventHandler<ErrorResponse>((sender, e) =>
{
    Console.WriteLine($"----> { e.Type} received. Error: {e.Message}");
}));

// Start the connection
var liveSchema = new LiveSchema()
{
    Model = "nova-2",
    Encoding = "linear16",
    SampleRate = 16000,
    Punctuate = true,
    SmartFormat = true,
    InterimResults = true,
    UtteranceEnd = "1000",
    VadEvents = true,
};
await liveClient.Connect(liveSchema);

// Microphone streaming
var microphone = new Microphone(liveClient.Send);
microphone.Start();

// Wait for the user to press a key
Console.WriteLine("Press ENTER to exit...");
Console.ReadKey();

// Stop the microphone
microphone.Stop();

// Stop the connection
await liveClient.Stop();
```

# Example Code

There are examples for **every** API call in this SDK. You can find all of these examples in the [examples folder](https://github.com/deepgram/deepgram-dotnet-sdk/tree/main/examples) at the root of this repo.

These examples provide:

- Text to Speech - REST:

    - Hello World - [examples/text-to-speech/rest/file](https://github.com/deepgram/deepgram-dotnet-sdk/blob/main/examples/text-to-speech/rest/file/hello-world/Program.cs)

- Text to Speech - WebSocket:

    - Simple - [example/speak/websocket/simple](https://github.com/deepgram/deepgram-dotnet-sdk/blob/main/examples/text-to-speech/websocket/simple/Program.cs)

- Analyze Text:

    - Intent Recognition - [examples/analyze/intent](https://github.com/deepgram/deepgram-dotnet-sdk/blob/main/examples/analyze/intent/Program.cs)
    - Sentiment Analysis - [examples/analyze/sentiment](https://github.com/deepgram/deepgram-dotnet-sdk/blob/main/examples/analyze/sentiment/Program.cs)
    - Summarization - [examples/analyze/summary](https://github.com/deepgram/deepgram-dotnet-sdk/blob/main/examples/analyze/summary/Program.cs)
    - Topic Detection - [examples/analyze/topic](https://github.com/deepgram/deepgram-dotnet-sdk/blob/main/examples/analyze/topic/Program.cs)

- PreRecorded Audio:

    - Transcription From an Audio File - [examples/speech-to-text/rest/file](https://github.com/deepgram/deepgram-dotnet-sdk/blob/main/examples/speech-to-text/rest/file/Program.cs)
    - Transcription From a URL - [examples/speech-to-text/rest/url](https://github.com/deepgram/deepgram-dotnet-sdk/blob/main/examples/speech-to-text/rest/url/Program.cs)
    - Intent Recognition - [examples/speech-to-text/rest/intent](https://github.com/deepgram/deepgram-dotnet-sdk/blob/main/examples/speech-to-text/rest/intent/Program.cs)
    - Sentiment Analysis - [examples/speech-to-text/rest/sentiment](https://github.com/deepgram/deepgram-dotnet-sdk/blob/main/examples/speech-to-text/rest/sentiment/Program.cs)
    - Summarization - [examples/speech-to-text/rest/intent](https://github.com/deepgram/deepgram-dotnet-sdk/blob/main/examples/speech-to-text/rest/summary/Program.cs)
    - Topic Detection - [examples/speech-to-text/rest/topic](https://github.com/deepgram/deepgram-dotnet-sdk/blob/main/examples/speech-to-text/rest/topic/Program.cs)

- Live Audio Transcription:

    - From a Microphone - [examples/speech-to-text/websocket/microphone](https://github.com/deepgram/deepgram-dotnet-sdk/blob/main/examples/speech-to-text/websocket/microphone/Program.cs)
    - From an HTTP stream - [examples/speech-to-text/websocket/http](https://github.com/deepgram/deepgram-dotnet-sdk/blob/main/examples/speech-to-text/websocket/http/Program.cs)
    - From a File - [examples/speech-to-text/websocket/file](https://github.com/deepgram/deepgram-dotnet-sdk/blob/main/examples/speech-to-text/websocket/file/Program.cs)

- Management API exercise the full [CRUD](https://en.wikipedia.org/wiki/Create,_read,_update_and_delete) operations for:

    - Balances - [examples/manage/balances](https://github.com/deepgram/deepgram-dotnet-sdk/blob/main/examples/manage/balances/Program.cs)
    - Invitations - [examples/manage/invitations](https://github.com/deepgram/deepgram-dotnet-sdk/blob/main/examples/manage/invitations/Program.cs)
    - Models - [examples/manage/models](https://github.com/deepgram/deepgram-dotnet-sdk/blob/main/examples/manage/models/Program.cs)
    - Keys - [examples/manage/keys](https://github.com/deepgram/deepgram-dotnet-sdk/blob/main/examples/manage/keys/Program.cs)
    - Members - [examples/manage/members](https://github.com/deepgram/deepgram-dotnet-sdk/blob/main/examples/manage/members/Program.cs)
    - Projects - [examples/manage/projects](https://github.com/deepgram/deepgram-dotnet-sdk/blob/main/examples/manage/projects/Program.cs)
    - Scopes - [examples/manage/scopes](https://github.com/deepgram/deepgram-dotnet-sdk/blob/main/examples/manage/scopes/Program.cs)
    - Usage - [examples/manage/usage](https://github.com/deepgram/deepgram-dotnet-sdk/blob/main/examples/manage/usage/Program.cs)

To run each example, set the `DEEPGRAM_API_KEY` as an environment variable, then `cd` into each example folder and execute the example: `dotnet run <Project File>.csproj`.


# Logging

This SDK uses [Serilog](https://github.com/serilog/serilog) to perform all of its logging tasks. By default, this SDK will enable `Information` level messages and higher (ie `Warning`, `Error`, etc.) when you initialize the library as follows:

```csharp
// Default logging level is "Information"
Library.Initialize();
```

To increase the logging output/verbosity for debug or troubleshooting purposes, you can set the `Debug` level but using this code:

```csharp
Library.Initialize(LogLevel.Debug);
```

# Backwards Compatibility

Older SDK versions will receive Priority 1 (P1) bug support only. Security issues, both in our code and dependencies, are promptly addressed. Significant bugs without clear workarounds are also given priority attention.

# Development and Contributing

Interested in contributing? We ❤️ pull requests!

To make sure our community is safe for all, be sure to review and agree to our
[Code of Conduct](https://github.com/deepgram/deepgram-dotnet-sdk/blob/main/.github/CODE_OF_CONDUCT.md). Then see the
[Contribution](https://github.com/deepgram/deepgram-dotnet-sdk/blob/main/.github/CONTRIBUTING.md) guidelines for more information.

# Getting Help

We love to hear from you, so if you have questions, comments or find a bug in the project, please let us know! You can either:

- [Open an issue in this repository](https://github.com/deepgram/deepgram-dotnet-sdk/issues/new)
- [Join the Deepgram Github Discussions Community](https://github.com/orgs/deepgram/discussions)
- [Join the Deepgram Discord Community](https://discord.gg/xWRaCDBtW4)
