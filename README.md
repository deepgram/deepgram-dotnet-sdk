# Deepgram .NET SDK

[![NuGet](https://img.shields.io/nuget/v/deepgram)](https://www.nuget.org/packages/Deepgram)
[![Build Status](https://github.com/deepgram-devs/deepgram-dotnet-sdk/workflows/CI/badge.svg)](https://github.com/deepgram-devs/deepgram-dotnet-sdk/actions?query=CI)
[![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-v2.0%20adopted-ff69b4.svg)](./.github/CODE_OF_CONDUCT.md)
[![Discord](https://dcbadge.vercel.app/api/server/xWRaCDBtW4?style=flat)](https://discord.gg/xWRaCDBtW4)

Official .NET SDK for [Deepgram](https://www.deepgram.com/).
Power your apps with world-class speech and Language AI models.

- [Deepgram .NET SDK](#deepgram-net-sdk)
  - [Documentation](#documentation)
  - [Requirements](#requirements)
  - [Installation](#installation)
  - [Getting an API Key](#getting-an-api-key)
  - [Initialization](#initialization)
  - [Pre-Recorded (Synchronous)](#pre-recorded-synchronous)
    - [Remote Files (Synchronous)](#remote-files-synchronous)
    - [Local Files (Synchronous)](#local-files-synchronous)
  - [Pre-Recorded (Asynchronous / Callbacks)](#pre-recorded-asynchronous--callbacks)
    - [Remote Files (Asynchronous)](#remote-files-asynchronous)
    - [Local Files (Asynchronous)](#local-files-asynchronous)
  - [Streaming Audio](#streaming-audio)
  - [Voice Agent](#voice-agent)
  - [Text to Speech REST](#text-to-speech-rest)
  - [Text to Speech Streaming](#text-to-speech-streaming)
  - [Text Intelligence](#text-intelligence)
  - [Authentication](#authentication)
    - [Grant Token](#grant-token)
  - [Projects](#projects)
    - [Get Projects](#get-projects)
    - [Get Project](#get-project)
    - [Update Project](#update-project)
    - [Delete Project](#delete-project)
  - [Keys](#keys)
    - [List Keys](#list-keys)
    - [Get Key](#get-key)
    - [Create Key](#create-key)
    - [Delete Key](#delete-key)
  - [Members](#members)
    - [Get Members](#get-members)
    - [Remove Member](#remove-member)
  - [Scopes](#scopes)
    - [Get Member Scopes](#get-member-scopes)
    - [Update Scope](#update-scope)
  - [Invitations](#invitations)
    - [List Invites](#list-invites)
    - [Send Invite](#send-invite)
    - [Delete Invite](#delete-invite)
    - [Leave Project](#leave-project)
  - [Usage](#usage)
    - [Get All Requests](#get-all-requests)
    - [Get Request](#get-request)
    - [Get Fields](#get-fields)
    - [Summarize Usage](#summarize-usage)
  - [Billing](#billing)
    - [Get All Balances](#get-all-balances)
    - [Get Balance](#get-balance)
  - [Models](#models)
    - [Get All Project Models](#get-all-project-models)
    - [Get Model](#get-model)
  - [On-Prem APIs](#on-prem-apis)
    - [List On-Prem credentials](#list-on-prem-credentials)
    - [Get On-Prem credentials](#get-on-prem-credentials)
    - [Create On-Prem credentials](#create-on-prem-credentials)
    - [Delete On-Prem credentials](#delete-on-prem-credentials)
  - [Logging](#logging)
  - [Backwards Compatibility](#backwards-compatibility)
  - [Development and Contributing](#development-and-contributing)
    - [Getting Help](#getting-help)

## Documentation

You can learn more about the Deepgram API at [developers.deepgram.com](https://developers.deepgram.com/docs).

## Requirements

This SDK supports the following versions:

- .NET 8.0
- .NET Standard 2.0

## Installation

To install the latest version of the .NET SDK using [NuGet](https://www.nuget.org/),
run the following command from your terminal in your project's directory:

```bash
dotnet add package Deepgram
```

Or use the [NuGet package Manager](https://learn.microsoft.com/en-us/nuget/consume-packages/install-use-packages-visual-studio).
Right click on project and select manage NuGet packages.

## Getting an API Key

🔑 To access the Deepgram API, you will need a
[free Deepgram API Key](https://console.deepgram.com/signup?jump=keys).

## Initialization

All of the examples below will require initializing the Deepgram client
and inclusion of imports.

```csharp
using Deepgram;
using Deepgram.Models.Listen.v1.REST;
using Deepgram.Models.Speak.v1.REST;
using Deepgram.Models.Analyze.v1;
using Deepgram.Models.Manage.v1;
using Deepgram.Models.Authenticate.v1;

// Initialize Library with default logging
Library.Initialize();

// Create client using the client factory
var deepgramClient = ClientFactory.CreateListenRESTClient();
```

## Pre-Recorded (Synchronous)

### Remote Files (Synchronous)

Transcribe audio from a URL.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var deepgramClient = ClientFactory.CreateListenRESTClient();

// Define Deepgram Options
var response = await deepgramClient.TranscribeUrl(
    new UrlSource("https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav"),
    new PreRecordedSchema()
    {
        Model = "nova-3",
    });

// Writes to Console
Console.WriteLine($"Transcript: {response.Results.Channels[0].Alternatives[0].Transcript}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/speech-to-text-api/listen).

[See the Example for more info](./examples/speech-to-text/rest/url/).

### Local Files (Synchronous)

Transcribe audio from a file.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var deepgramClient = ClientFactory.CreateListenRESTClient();

// Check if file exists
if (!File.Exists("audio.wav"))
{
    Console.WriteLine("Error: File 'audio.wav' not found.");
    return;
}
// Define Deepgram Options
var audioData = File.ReadAllBytes("audio.wav");
var response = await deepgramClient.TranscribeFile(
    audioData,
    new PreRecordedSchema()
    {
        Model = "nova-3",
    });
// Writes to Console
Console.WriteLine($"Transcript: {response.Results.Channels[0].Alternatives[0].Transcript}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/speech-to-text-api/listen).

[See the Example for more info](./examples/speech-to-text/rest/file/).

## Pre-Recorded (Asynchronous / Callbacks)

### Remote Files (Asynchronous)

Transcribe audio from a URL with callback.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var deepgramClient = ClientFactory.CreateListenRESTClient();

// Define Deepgram Options
var response = await deepgramClient.TranscribeUrl(
    new UrlSource("https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav"),
    new PreRecordedSchema()
    {
        Model = "nova-3",
        Callback = "https://your-callback-url.com/webhook",
    });

// Writes to Console
Console.WriteLine($"Request ID: {response.RequestId}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/speech-to-text-api/listen).

### Local Files (Asynchronous)

Transcribe audio from a file with callback.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var deepgramClient = ClientFactory.CreateListenRESTClient();

var audioData = File.ReadAllBytes("audio.wav");
var response = await deepgramClient.TranscribeFile(
    audioData,
    new PreRecordedSchema()
    {
        Model = "nova-3",
        Callback = "https://your-callback-url.com/webhook",
    });

Console.WriteLine($"Request ID: {response.RequestId}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/speech-to-text-api/listen).

## Streaming Audio

Transcribe streaming audio.

```csharp
using Deepgram;
using Deepgram.Models.Listen.v2.WebSocket;
using Deepgram.Models.Speak.v2.WebSocket;
using Deepgram.Models.Agent.v2.WebSocket;

// Initialize Library with default logging
Library.Initialize();

// Create WebSocket client
var liveClient = ClientFactory.CreateListenWebSocketClient();
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key

// Subscribe to transcription results
await liveClient.Subscribe(new EventHandler<ResultResponse>((sender, e) =>
{
    if (!string.IsNullOrEmpty(e.Channel.Alternatives[0].Transcript))
    {
        Console.WriteLine($"Transcript: {e.Channel.Alternatives[0].Transcript}");
    }
}));

// Connect to Deepgram
var liveSchema = new LiveSchema()
{
    Model = "nova-3",
};
await liveClient.Connect(liveSchema);

// Stream audio data to Deepgram
byte[] audioData = GetAudioData(); // Your audio source
liveClient.Send(audioData);

// Keep connection alive while streaming
await Task.Delay(TimeSpan.FromSeconds(30));

// Stop the connection
await liveClient.Stop();
```

[See our API reference for more info](https://developers.deepgram.com/reference/speech-to-text-api/listen-streaming).

[See the Examples for more info](./examples/speech-to-text/websocket/).

## Voice Agent

Configure a Voice Agent.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var agentClient = ClientFactory.CreateAgentWebSocketClient();

// Subscribe to key events
await agentClient.Subscribe(new EventHandler<ConversationTextResponse>
((sender, e) =>
{
    Console.WriteLine($"Agent: {e.Text}");
}));
await agentClient.Subscribe(new EventHandler<AudioResponse>((sender, e) =>
{
    // Handle agent's audio response
    Console.WriteLine("Agent speaking...");
}));

// Configure agent settings
var settings = new SettingsSchema()
{
    Language = "en",
    Agent = new AgentSchema()
    {
        Think = new ThinkSchema()
        {
            Provider = new ProviderSchema()
            {
                Type = "open_ai",
                Model = "gpt-4o-mini",
            },
            Prompt = "You are a helpful AI assistant.",
        },
        Listen = new ListenSchema()
        {
            Provider = new ProviderSchema()
            {
                Type = "deepgram",
                Model = "nova-3",
            },
        },
        Speak = new SpeakSchema()
        {
            Provider = new ProviderSchema()
            {
                Type = "deepgram",
                Model = "aura-2-thalia-en",
            },
        },
    },
    Greeting = "Hello, I'm your AI assistant.",
};

// Connect to Deepgram Voice Agent
await agentClient.Connect(settings);

// Keep connection alive
await Task.Delay(TimeSpan.FromSeconds(30));

// Cleanup
await agentClient.Stop();
```

This example demonstrates:

- Setting up a WebSocket connection for Voice Agent
- Configuring the agent with speech, language, and audio settings
- Handling various agent events (speech, transcripts, audio)
- Sending audio data and keeping the connection alive

For a complete implementation, you would need to:

1. Add your audio input source (e.g., microphone)
2. Implement audio playback for the agent's responses
3. Handle any function calls if your agent uses them
4. Add proper error handling and connection management

## Text to Speech REST

Convert text into speech using the REST API.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var speakClient = ClientFactory.CreateSpeakRESTClient();

var response = await speakClient.ToFile(
    new TextSource("Hello world!"),
    "output.wav",
    new SpeakSchema()
    {
        Model = "aura-2-thalia-en",
    });

Console.WriteLine($"Audio saved to: output.wav");
```

[See our API reference for more info](https://developers.deepgram.com/reference/text-to-speech-api/speak).

[See the Example for more info](./examples/text-to-speech/rest/).

## Text to Speech Streaming

Convert streaming text into speech using a WebSocket.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var speakClient = ClientFactory.CreateSpeakWebSocketClient();

// Subscribe to audio responses
await speakClient.Subscribe(new EventHandler<AudioResponse>((sender, e) =>
{
    // Handle the generated audio data
    Console.WriteLine("Received audio data");
}));

// Configure speak options
var speakSchema = new SpeakSchema()
{
    Model = "aura-2-thalia-en",
    Encoding = "linear16",
    SampleRate = 16000,
};

// Connect to Deepgram
await speakClient.Connect(speakSchema);

// Send text to convert to speech
await speakClient.SendText("Hello, this is a text to speech example.");
await speakClient.Flush();

// Wait for completion and cleanup
await speakClient.WaitForComplete();
await speakClient.Stop();
```

[See our API reference for more info](https://developers.deepgram.com/reference/text-to-speech-api/speak-streaming).

[See the Examples for more info](./examples/text-to-speech/websocket/).

## Text Intelligence

Analyze text.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var analyzeClient = ClientFactory.CreateAnalyzeClient();

// Check if file exists
if (!File.Exists("text_to_analyze.txt"))
{
    Console.WriteLine("Error: File 'text_to_analyze.txt' not found.");
    return;
}

var textData = File.ReadAllBytes("text_to_analyze.txt");
var response = await analyzeClient.AnalyzeFile(
    textData,
    new AnalyzeSchema()
    {
        Model = "nova-3"
        // Configure Read Options
    });

Console.WriteLine($"Analysis Results: {response}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/text-intelligence-api/text-read).

[See the Examples for more info](./examples/analyze/).

## Authentication

### Grant Token

Creates a temporary token with a 30-second TTL.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var authClient = ClientFactory.CreateAuthClient();

var response = await authClient.GrantToken();

Console.WriteLine($"Token: {response.AccessToken}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/token-based-auth-api/grant-token).

[See the Examples for more info](./examples/auth/grant-token).

## Projects

### Get Projects

Returns all projects accessible by the API key.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var manageClient = ClientFactory.CreateManageClient();

var response = await manageClient.GetProjects();

Console.WriteLine($"Projects: {response.Projects}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/management-api/projects/list).

[See the Example for more info](./examples/manage/projects/).

### Get Project

Retrieves a specific project based on the provided project_id.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var manageClient = ClientFactory.CreateManageClient();

var response = await manageClient.GetProject(projectId);

Console.WriteLine($"Project: {response.Project}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/management-api/projects/get).

[See the Example for more info](./examples/manage/projects/).

### Update Project

Update a project.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var manageClient = ClientFactory.CreateManageClient();

var updateOptions = new ProjectSchema()
{
    Name = "Updated Project Name",
};

var response = await manageClient.UpdateProject(projectId, updateOptions);

Console.WriteLine($"Update result: {response.Message}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/management-api/projects/update).

[See the Example for more info](./examples/manage/projects/).

### Delete Project

Delete a project.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var manageClient = ClientFactory.CreateManageClient();

var response = await manageClient.DeleteProject(projectId);

Console.WriteLine($"Delete result: {response.Message}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/management-api/projects/delete).

[See the Example for more info](./examples/manage/projects/).

## Keys

### List Keys

Retrieves all keys associated with the provided project_id.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var manageClient = ClientFactory.CreateManageClient();

var response = await manageClient.GetKeys(projectId);

Console.WriteLine($"Keys: {response.APIKeys}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/management-api/keys/list).

[See the Example for more info](./examples/manage/keys/).

### Get Key

Retrieves a specific key associated with the provided project_id.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var manageClient = ClientFactory.CreateManageClient();

var response = await manageClient.GetKey(projectId, keyId);

Console.WriteLine($"Key: {response.APIKey}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/management-api/keys/get).

[See the Example for more info](./examples/manage/keys/).

### Create Key

Creates an API key with the provided scopes.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var manageClient = ClientFactory.CreateManageClient();

var createOptions = new KeySchema()
{
    Comment = "My API Key",
    Scopes = new List<string> { "admin" },
};

var response = await manageClient.CreateKey(projectId, createOptions);

Console.WriteLine($"Created key: {response.APIKeyID}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/management-api/keys/create).

[See the Example for more info](./examples/manage/keys/).

### Delete Key

Deletes a specific key associated with the provided project_id.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var manageClient = ClientFactory.CreateManageClient();

var response = await manageClient.DeleteKey(projectId, keyId);

Console.WriteLine($"Delete result: {response.Message}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/management-api/keys/delete).

[See the Example for more info](./examples/manage/keys/).

## Members

### Get Members

Retrieves account objects for all of the accounts in the specified project_id.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var manageClient = ClientFactory.CreateManageClient();

var response = await manageClient.GetMembers(projectId);

Console.WriteLine($"Members: {response.Members}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/management-api/members/list).

[See the Example for more info](./examples/manage/members/).

### Remove Member

Removes member account for specified member_id.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var manageClient = ClientFactory.CreateManageClient();

var response = await manageClient.RemoveMember(projectId, memberId);

Console.WriteLine($"Remove result: {response.Message}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/management-api/members/delete).

[See the Example for more info](./examples/manage/members/).

## Scopes

### Get Member Scopes

Retrieves scopes of the specified member in the specified project.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var manageClient = ClientFactory.CreateManageClient();

var response = await manageClient.GetMemberScopes(projectId, memberId);

Console.WriteLine($"Scopes: {response.Scopes}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/management-api/scopes/list).

[See the Example for more info](./examples/manage/scopes/).

### Update Scope

Updates the scope for the specified member in the specified project.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var manageClient = ClientFactory.CreateManageClient();

var updateOptions = new ScopeSchema()
{
    Scope = "admin",
};

var response = await manageClient.UpdateMemberScopes(projectId, memberId, updateOptions);

Console.WriteLine($"Update result: {response.Message}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/management-api/scopes/update).

[See the Example for more info](./examples/manage/scopes/).

## Invitations

### List Invites

Retrieves all invitations associated with the provided project_id.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var manageClient = ClientFactory.CreateManageClient();

var response = await manageClient.GetInvitations(projectId);

Console.WriteLine($"Invitations: {response.Invites}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/management-api/invitations/list).

[See the Example for more info](./examples/manage/invitations/).

### Send Invite

Sends an invitation to the provided email address.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var manageClient = ClientFactory.CreateManageClient();

var inviteOptions = new InvitationSchema()
{
    Email = "user@example.com",
    Scope = "admin",
};

var response = await manageClient.SendInvitation(projectId, inviteOptions);

Console.WriteLine($"Invitation sent: {response.Message}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/management-api/invitations/create).

[See the Example for more info](./examples/manage/invitations/).

### Delete Invite

Removes the specified invitation from the project.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var manageClient = ClientFactory.CreateManageClient();

var response = await manageClient.DeleteInvitation(projectId, email);

Console.WriteLine($"Delete result: {response.Message}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/management-api/invitations/delete).

[See the Example for more info](./examples/manage/invitations/).

### Leave Project

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var manageClient = ClientFactory.CreateManageClient();

var response = await manageClient.LeaveProject(projectId);

Console.WriteLine($"Leave result: {response.Message}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/management-api/invitations/leave).

[See the Example for more info](./examples/manage/invitations/).

## Usage

### Get All Requests

Retrieves all requests associated with the provided project_id
based on the provided options.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var manageClient = ClientFactory.CreateManageClient();

var response = await manageClient.GetUsageRequests(projectId);

Console.WriteLine($"Requests: {response.Requests}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/management-api/usage/list-requests).

[See the Example for more info](./examples/manage/usage/).

### Get Request

Retrieves a specific request associated with the provided project_id.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var manageClient = ClientFactory.CreateManageClient();

var response = await manageClient.GetUsageRequest(projectId, requestId);

Console.WriteLine($"Request: {response.Request}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/management-api/usage/get-request).

[See the Example for more info](./examples/manage/usage/).

### Get Fields

Lists the features, models, tags, languages, and processing method
used for requests in the specified project.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var manageClient = ClientFactory.CreateManageClient();

var response = await manageClient.GetUsageFields(projectId);

Console.WriteLine($"Fields: {response.Fields}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/management-api/usage/list-fields).

[See the Example for more info](./examples/manage/usage/).

### Summarize Usage

`Deprecated` Retrieves the usage for a specific project. Use Get Project Usage Breakdown
for a more comprehensive usage summary.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var manageClient = ClientFactory.CreateManageClient();

var response = await manageClient.GetUsageSummary(projectId);

Console.WriteLine($"Usage summary: {response.Usage}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/management-api/usage/get).

[See the Example for more info](./examples/manage/usage/).

## Billing

### Get All Balances

Retrieves the list of balance info for the specified project.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var manageClient = ClientFactory.CreateManageClient();

var response = await manageClient.GetBalances(projectId);

Console.WriteLine($"Balances: {response.Balances}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/management-api/balances/list).

[See the Example for more info](./examples/manage/balances/).

### Get Balance

Retrieves the balance info for the specified project and balance_id.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var manageClient = ClientFactory.CreateManageClient();

var response = await manageClient.GetBalance(projectId, balanceId);

Console.WriteLine($"Balance: {response.Balance}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/management-api/balances/get).

[See the Example for more info](./examples/manage/balances/).

## Models

### Get All Project Models

Retrieves all models available for a given project.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var manageClient = ClientFactory.CreateManageClient();

var response = await manageClient.GetProjectModels(projectId);

Console.WriteLine($"Models: {response}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/management-api/projects/list-models).

[See the Example for more info](./examples/manage/models/).

### Get Model

Retrieves details of a specific model.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var manageClient = ClientFactory.CreateManageClient();

var response = await manageClient.GetProjectModel(projectId, modelId);

Console.WriteLine($"Model: {response.Model}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/management-api/projects/get-model).

[See the Example for more info](./examples/manage/models/).

## On-Prem APIs

### List On-Prem credentials

Lists sets of distribution credentials for the specified project.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var selfHostedClient = ClientFactory.CreateSelfHostedClient();

var response = await selfHostedClient.ListSelfhostedCredentials(projectId);

Console.WriteLine($"Credentials: {response.Credentials}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/self-hosted-api/list-credentials).

### Get On-Prem credentials

Returns a set of distribution credentials for the specified project.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var selfHostedClient = ClientFactory.CreateSelfHostedClient();

var response = await selfHostedClient.GetSelfhostedCredentials(projectId, distributionCredentialsId);

Console.WriteLine($"Credentials: {response.Credentials}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/self-hosted-api/get-credentials).

### Create On-Prem credentials

Creates a set of distribution credentials for the specified project.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var selfHostedClient = ClientFactory.CreateSelfHostedClient();

var createOptions = new SelfhostedCredentialsSchema()
{
    Comment = "My on-prem credentials",
};

var response = await selfHostedClient.CreateSelfhostedCredentials(projectId, createOptions);

Console.WriteLine($"Created credentials: {response.CredentialsID}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/self-hosted-api/create-credentials).

### Delete On-Prem credentials

Deletes a set of distribution credentials for the specified project.

```csharp
// Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
var selfHostedClient = ClientFactory.CreateSelfHostedClient();

var response = await selfHostedClient.DeleteSelfhostedCredentials(projectId, distributionCredentialId);

Console.WriteLine($"Delete result: {response.Message}");
```

[See our API reference for more info](https://developers.deepgram.com/reference/self-hosted-api/delete-credentials).

## Logging

This SDK uses [Serilog](https://github.com/serilog/serilog) to perform all of its
logging tasks. By default, this SDK will enable `Information` level messages and
higher (ie `Warning`, `Error`, etc.) when you initialize the library as follows:

```csharp
// Default logging level is "Information"
Library.Initialize();
```

To increase the logging output/verbosity for debug or troubleshooting purposes,
you can set the `Debug` level but using this code:

```csharp
Library.Initialize(LogLevel.Debug);
```

## Backwards Compatibility

We follow semantic versioning (semver) to ensure a smooth upgrade experience.
Within a major version (like `6.*`), we will maintain backward compatibility
so your code will continue to work without breaking changes.
When we release a new major version (like moving from `5.*` to `6.*`),
we may introduce breaking changes to improve the SDK.
We'll always document these changes clearly in our release notes to help you
upgrade smoothly.

Older SDK versions will receive Priority 1 (P1) bug support only.
Security issues, both in our code and dependencies, are promptly addressed.
Significant bugs without clear workarounds are also given priority attention.

## Development and Contributing

Interested in contributing? We ❤️ pull requests!

To make sure our community is safe for all, be sure to review and agree to our
[Code of Conduct](.github/CODE_OF_CONDUCT.md).
Then see the [Contribution](.github/CONTRIBUTING.md) guidelines for more information.

### Getting Help

We love to hear from you, so if you have questions, comments or find a bug in
the project, please let us know! You can either:

- [Open an issue in this repository](https://github.com/deepgram/deepgram-dotnet-sdk/issues/new)
- [Join the Deepgram Github Discussions Community](https://github.com/orgs/deepgram/discussions)
- [Join the Deepgram Discord Community](https://discord.gg/xWRaCDBtW4)
