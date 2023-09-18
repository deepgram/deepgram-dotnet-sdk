# Deepgram .NET SDK

[![Nuget](https://img.shields.io/nuget/v/deepgram)](https://www.nuget.org/packages/Deepgram) [![Build Status](https://github.com/deepgram-devs/deepgram-dotnet-sdk/workflows/CI/badge.svg)](https://github.com/deepgram-devs/deepgram-dotnet-sdk/actions?query=CI) [![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-v2.0%20adopted-ff69b4.svg)](./.github/CODE_OF_CONDUCT.md)

Official .NET SDK for [Deepgram](https://www.deepgram.com/). Power your apps with world-class speech and Language AI models.

> This SDK only supports hosted usage of api.deepgram.com.

- [Deepgram .NET SDK](#deepgram-net-sdk)
- [Getting an API Key](#getting-an-api-key)
- [Documentation](#documentation)
- [Installation](#installation)
- [Targeted Frameworks](#targeted-frameworks)
- [Configuration](#configuration)
- [Examples](#examples)
- [Transcription](#transcription)
  - [Remote Files](#remote-files)
    - [UrlSource](#urlsource)
  - [Local files](#local-files)
    - [StreamSource](#streamsource)
    - [PrerecordedTranscriptionOptions](#prerecordedtranscriptionoptions)
- [Generating Captions](#generating-captions)
  - [Live Audio](#live-audio)
    - [LiveTranscriptionOptions](#livetranscriptionoptions)
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
    - [ListAllRequestOptions](#listallrequestoptions)
  - [Get Request](#get-request)
  - [Summarize Usage](#summarize-usage)
    - [GetUsageSummaryOptions](#getusagesummaryoptions)
  - [Get Fields](#get-fields)
    - [GetUsageFieldsOptions](#getusagefieldsoptions)
- [Logging](#logging)
- [Development and Contributing](#development-and-contributing)
- [Testing](#testing)
- [Getting Help](#getting-help)

# Getting an API Key

🔑 To access the Deepgram API you will need a [free Deepgram API Key](https://console.deepgram.com/signup?jump=keys).

# Documentation

Complete documentation of the .NET SDK can be found on the
[Deepgram Docs](https://developers.deepgram.com/docs/dotnet-sdk).

You can learn more about the full Deepgram API at [https://developers.deepgram.com](https://developers.deepgram.com).

# Installation

To install the C# SDK using NuGet:

Run the following command from your terminal in your projects directory:

```bash
dotnet add package Deepgram
```

Or use the Nuget package Manager.

Right click on project and select manage nuget packages

# Targeted Frameworks

- 7.0.x
- 6.0.x

# Configuration

To setup the configuration of the Deepgram Client you can do one of the following:

- Create a Deepgram Client instance and pass in credentials in the constructor.

```csharp
var credentials = new Credentials(YOUR_DEEPGRAM_API_KEY);
var deepgramClient = new DeepgramClient(credentials);
```

# Examples

To quickly get started with examples for prerecorded and streaming, run the files in the example folder. See the README in that folder for more information on getting started.

# Transcription

## Remote Files

```csharp
using Deepgram.Models;

var credentials = new Credentials(DEEPGRAM_API_KEY);

var deepgramClient = new DeepgramClient(credentials);

var response = await deepgramClient.Transcription.Prerecorded.GetTranscriptionAsync(
    new UrlSource("https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav"),
    new PrerecordedTranscriptionOptions()
    {
        Punctuate = true
    });
```

#### UrlSource

| Property | Value  |          Description          |
| -------- | :----- | :---------------------------: |
| Url      | string | Url of the file to transcribe |

## Local files

```csharp
using Deepgram.Models;

var credentials = new Credentials(DEEPGRAM_API_KEY);

var deepgramClient = new DeepgramClient(credentials);

using (FileStream fs = File.OpenRead("path\\to\\file"))
{
    var response = await deepgramClient.Transcription.Prerecorded.GetTranscriptionAsync(
        new StreamSource(
            fs,
            "audio/wav"),
        new PrerecordedTranscriptionOptions()
        {
            Punctuate = true
        });
}
```

#### StreamSource

| Property | Value Type |      reason for      |
| -------- | :--------- | :------------------: |
| Stream   | Stream     | stream to transcribe |
| MimeType | string     |  MIMETYPE of stream  |

#### PrerecordedTranscriptionOptions

| Property               | Value Type |                                                      reason for                                                      |       Possible values       |
| ---------------------- | :--------- | :------------------------------------------------------------------------------------------------------------------: | :-------------------------: |
| Model                  | string     |                                       AI model used to process submitted audio                                       |
| Version                | string     |                                             Version of the model to use                                              |
| Language               | string     |                            BCP-47 language tag that hints at the primary spoken language                             |
| Tier                   | string     |                                 Level of model you would like to use in your request                                 |
| Punctuate              | bool       |                      Indicates whether to add punctuation and capitalization to the transcript                       |
| ProfanityFilter        | bool       |                              Indicates whether to remove profanity from the transcript                               |
| Redaction              | string[]   |                                  Indicates whether to redact sensitive information                                   |      pci, numbers, ssn      |
| Diarize                | bool       |                                    Indicates whether to recognize speaker changes                                    |
| DiarizationVersion     | string     |                                    Indicates which version of the diarizer to use                                    |
| NamedEntityRecognition | bool       |                                 Indicates whether to recognize alphanumeric strings.                                 | **obselete** **deprecated** |
| MultiChannel           | bool       |                           Indicates whether to transcribe each audio channel independently                           |
| Alternatives           | int        |                                 Maximum number of transcript alternatives to return                                  |
| Numerals               | bool       |                               Indicates whether to convert numbers from written format                               |
| Numbers                | bool       |                               Indicates whether to convert numbers from written format                               |
| NumbersSpaces          | bool       |                                Indicates whether to add spaces between spoken numbers                                |
| Dates                  | bool       |                                Indicates whether to convert dates from written format                                |
| DateFormat             | string     |                                        Indicates the format to use for dates                                         |
| Times                  | bool       |                                Indicates whether to convert times from written format                                |
| Dictation              | bool       |                                         Option to format punctuated commands                                         |
| Measurements           | bool       |                                  Option to convert measurments to numerical format                                   |
| SmartFormat            | bool       |                               Indicates whether to use Smart Format on the transcript                                |
| SearchTerms            | string[]   |                                Terms or phrases to search for in the submitted audio                                 |
| Replace                | string[]   |                          Terms or phrases to search for in the submitted audio and replace                           |
| Callback               | string     |            Callback URL to provide if you would like your submitted audio to be processed asynchronously             |
| Keywords               | string[]   | Keywords to which the model should pay particular attention to boosting or suppressing to help it understand context |
| KeywordBoost           | string     |                                            Support for out-of-vocabulary                                             |
| Utterances             | bool       |                    Indicates whether Deepgram will segment speech into meaningful semantic units                     |
| DetectLanguage         | bool       |                            Indicates whether to detect the language of the provided audio                            |
| Paragraphs             | bool       |                             Indicates whether Deepgram will split audio into paragraphs                              |
| UtteranceSplit         | decimal    |              Length of time in seconds of silence between words that Deepgram will use when determining              |
| Summarize              | object     |              Indicates whether Deepgram should provide summarizations of sections of the provided audio              |
| DetectEntities         | bool       |                     Indicates whether Deepgram should detect entities within the provided audio                      |
| Translate              | string[]   |                              anguage codes to which transcripts should be translated to                              |
| DetectTopics           | bool       |                      Indicates whether Deepgram should detect topics within the provided audio                       |
| AnalyzeSentiment       | bool       |                         Indicates whether Deepgram will identify sentiment in the transcript                         |
| Sentiment              | bool       |                           Indicates whether Deepgram will identify sentiment in the audio                            |
| SentimentThreshold     | decimal    |                            Indicates the confidence requirement for non-neutral sentiment                            |

# Generating Captions

```
var preRecordedTranscription =  await deepgramClient.Transcription.Prerecorded.GetTranscriptionAsync(streamSource,prerecordedtranscriptionOptions);
var WebVTT = preRecordedTranscription.ToWebVTT();
var SRT =  preRecordedTranscription.ToSRT();
```

## Live Audio

> The example below demonstrates sending a pre-recorded audio to simulate a real-time
> stream of audio. In a real application, this type of audio is better handled using the
> pre-recorded transcription.

```csharp
using Deepgram.CustomEventArgs;
using Deepgram.Models;
using System.Net.WebSockets;

var credentials = new Credentials(DEEPGRAM_API_KEY);

var deepgramClient = new DeepgramClient(credentials);

using (var deepgramLive = deepgramClient.CreateLiveTranscriptionClient())
{
    deepgramLive.ConnectionOpened += HandleConnectionOpened;
    deepgramLive.ConnectionClosed += HandleConnectionClosed;
    deepgramLive.ConnectionError += HandleConnectionError;
    deepgramLive.TranscriptReceived += HandleTranscriptReceived;

    // Connection opened so start sending audio.
    async void HandleConnectionOpened(object? sender, ConnectionOpenEventArgs e)
    {
        byte[] buffer;

        using (FileStream fs = File.OpenRead("path\\to\\file"))
        {
            buffer = new byte[fs.Length];
            fs.Read(buffer, 0, (int)fs.Length);
        }

        var chunks = buffer.Chunk(1000);

        foreach (var chunk in chunks)
        {
            deepgramLive.SendData(chunk);
            await Task.Delay(50);
        }

        await deepgramLive.FinishAsync();
    }

    void HandleTranscriptReceived(object? sender, TranscriptReceivedEventArgs e)
    {
        if (e.Transcript.IsFinal && e.Transcript.Channel.Alternatives.First().Transcript.Length > 0) {
            var transcript = e.Transcript;
            Console.WriteLine($"[Speaker: {transcript.Channel.Alternatives.First().Words.First().Speaker}] {transcript.Channel.Alternatives.First().Transcript}");
        }
    }

    void HandleConnectionClosed(object? sender, ConnectionClosedEventArgs e)
    {
        Console.Write("Connection Closed");
    }

    void HandleConnectionError(object? sender, ConnectionErrorEventArgs e)
    {
        Console.WriteLine(e.Exception.Message);
    }

    var options = new LiveTranscriptionOptions() { Punctuate = true, Diarize = true, Encoding = Deepgram.Common.AudioEncoding.Linear16 };
    await deepgramLive.StartConnectionAsync(options);

    while (deepgramLive.State() == WebSocketState.Open) { }
}
```

#### LiveTranscriptionOptions

| Property               | Type     |                                                               Description                                                               |             Possible values |
| ---------------------- | :------- | :-------------------------------------------------------------------------------------------------------------------------------------: | --------------------------: |
| Model                  | string   |                                                AI model used to process submitted audio                                                 |
| Version                | string   |                                                       Version of the model to use                                                       |
| Language               | string   |                                      BCP-47 language tag that hints at the primary spoken language                                      |
| Tier                   | string   |                                          Level of model you would like to use in your request                                           |
| Punctuate              | bool     |                                Indicates whether to add punctuation and capitalization to the transcript                                |
| ProfanityFilter        | bool     |                                        Indicates whether to remove profanity from the transcript                                        |
| Redaction              | string[] |                                            Indicates whether to redact sensitive information                                            |           pci, numbers, ssn |
| Diarize                | bool     |                                             Indicates whether to recognize speaker changes                                              |
| DiarizationVersion     | string   |                                             Indicates which version of the diarizer to use                                              |
| NamedEntityRecognition | bool     |                                          Indicates whether to recognize alphanumeric strings.                                           | **obselete** **deprecated** |
| MultiChannel           | bool     |                                    Indicates whether to transcribe each audio channel independently                                     |
| Alternatives           | int      |                                           Maximum number of transcript alternatives to return                                           |
| Numerals               | bool     |                                        Indicates whether to convert numbers from written format                                         |
| Numbers                | bool     |                                        Indicates whether to convert numbers from written format                                         |
| NumbersSpaces          | bool     |                                         Indicates whether to add spaces between spoken numbers                                          |
| Dates                  | bool     |                                         Indicates whether to convert dates from written format                                          |
| DateFormat             | string   |                                                  Indicates the format to use for dates                                                  |
| Times                  | bool     |                                         Indicates whether to convert times from written format                                          |
| Dictation              | bool     |                                                  Option to format punctuated commands                                                   |
| Measurements           | bool     |                                            Option to convert measurments to numerical format                                            |
| SmartFormat            | bool     |                                         Indicates whether to use Smart Format on the transcript                                         |
| SearchTerms            | string[] |                                          Terms or phrases to search for in the submitted audio                                          |
| Replace                | string[] |                                    Terms or phrases to search for in the submitted audio and replace                                    |
| Callback               | string   |                      Callback URL to provide if you would like your submitted audio to be processed asynchronously                      |
| Keywords               | string[] |          Keywords to which the model should pay particular attention to boosting or suppressing to help it understand context           |
| KeywordBoost           | string   |                                                      Support for out-of-vocabulary                                                      |
| Utterances             | bool     |                              Indicates whether Deepgram will segment speech into meaningful semantic units                              |
| DetectLanguage         | bool     |                                     Indicates whether to detect the language of the provided audio                                      |
| Paragraphs             | bool     |                                       Indicates whether Deepgram will split audio into paragraphs                                       |
| InterimResults         | bool     |          Indicates whether the streaming endpoint should send you updates to its transcription as more audio becomes available          |
| EndPointing            | string   |                             Indicates whether Deepgram will detect whether a speaker has finished speaking                              |
| VADTurnOff             | int      | Length of time in milliseconds of silence that voice activation detection (VAD) will use to detect that a speaker has finished speaking |
| Encoding               | string   |                                           Expected encoding of the submitted streaming audio                                            |
| Channels               | int      |                               Number of independent audio channels contained in submitted streaming audio                               |
| SampleRate             | int      |                Sample rate of submitted streaming audio. Required (and only read) when a value is provided for encoding                 |

# Projects

> projectId and memberId are of type `string`

## Get Projects

Returns all projects accessible by the API key.

```csharp
var result = await deepgramClient.Projects.ListProjectsAsync();
```

[See our API reference for more info](https://developers.deepgram.com/reference/get-projects).

## Get Project

Retrieves a specific project based on the provided projectId.

```csharp
var result = await deepgramClient.Projects.GetProjectAsync(projectId);
```

[See our API reference for more info](https://developers.deepgram.com/reference/get-project).

## Update Project

Update a project.

```csharp
var project = new Project()
{
    Project = "projectId string",
    Name = "New name for Project"
}
var result = await deepgramClient.Projects.UpdateProjectAsync(project);

```

**Project Type**

| Property Name | Type   |                       Description                        |
| ------------- | :----- | :------------------------------------------------------: |
| Id            | string |        Unique identifier of the Deepgram project         |
| Name          | string |                   Name of the project                    |
| Company       | string | Name of the company associated with the Deepgram project |

[See our API reference for more info](https://developers.deepgram.com/reference/update-project).

## Delete Project

Delete a project.

```csharp
var result = await deepgramClient.Projects.DeleteProjectAsync(projectId);
```

[See our API reference for more info](https://developers.deepgram.com/reference/delete-project).

# Keys

> projectId,keyId and comment are of type`string`

## List Keys

Retrieves all keys associated with the provided project_id.

```csharp
var result = await deepgramClient.Keys.ListKeysAsync(projectId);
```

[See our API reference for more info](https://developers.deepgram.com/reference/list-keys).

## Get Key

Retrieves a specific key associated with the provided project_id.

```csharp
var result = await deepgramClient.Keys.GetKeyAsync(projectId,keyId);
```

[See our API reference for more info](https://developers.deepgram.com/reference/get-key).

## Create Key

Creates an API key with the provided scopes.

```csharp
var scopes = new string[]{"admin","member"};
var result = await deepgramClient.Keys.CreateKeyAsync(projectId,comment,scopes);
```

[See our API reference for more info](https://developers.deepgram.com/reference/create-key).

## Delete Key

Deletes a specific key associated with the provided project_id.

```csharp
var result = await deepgramClient.Keys.DeleteKeyAsync(projectId, keyId);
```

[See our API reference for more info](https://developers.deepgram.com/reference/delete-key).

# Members

> projectId and memberId are of type`string`

## Get Members

Retrieves account objects for all of the accounts in the specified project_id.

```csharp
var result = await deepgramClient.Projects.GetMembersScopesAsync(projectId,memberId);
```

[See our API reference for more info](https://developers.deepgram.com/reference/get-members).

## Remove Member

Removes member account for specified member_id.

```csharp
var result = await deepgramClient.Projects.RemoveMemberAsync(projectId,memberId);
```

[See our API reference for more info](https://developers.deepgram.com/reference/remove-member).

# Scopes

> projectId and memberId are of type`string`

## Get Member Scopes

Retrieves scopes of the specified member in the specified project.

```csharp
var result = await deepgramClient.Keys. GetMemberScopesAsync(projectId,memberId);
```

[See our API reference for more info](https://developers.deepgram.com/reference/get-member-scopes).

## Update Scope

Updates the scope for the specified member in the specified project.

```csharp
var scopeOptions = new UpdateScopeOption(){Scope = "admin"};
var result = await deepgramClient.Keys.UpdateScopeAsync(projectId,memberId,scopeOptions);
```

[See our API reference for more info](https://developers.deepgram.com/reference/update-scope).

# Invitations

## List Invites

Retrieves all invitations associated with the provided project_id.

```csharp
to be implmented
```

[See our API reference for more info](https://developers.deepgram.com/reference/list-invites).

## Send Invite

Sends an invitation to the provided email address.

```csharp
to be implmentented
```

[See our API reference for more info](https://developers.deepgram.com/reference/send-invites).

## Delete Invite

Removes the specified invitation from the project.

```csharp
to be implemented
```

[See our API reference for more info](https://developers.deepgram.com/reference/delete-invite).

## Leave Project

Removes the authenticated user from the project.

```csharp
var result = await deepgramClient.Projects.LeaveProjectAsync(projectId);
```

[See our API reference for more info](https://developers.deepgram.com/reference/leave-project).

# Usage

> projectId and requestId type`string`

## Get All Requests

Retrieves all requests associated with the provided projectId based on the provided options.

```csharp
var listAllRequestOptions = new listAllRequestOptions()
{
     StartDateTime = DateTime.Now
};
var result = await deepgramClient.Usage.ListAllRequestsAsync(projectId,listAllRequestOptions);
```

#### ListAllRequestOptions

| Property      | Type     |              Description               |
| ------------- | :------- | :------------------------------------: |
| StartDateTime | DateTime | Start date of the requested date range |
| EndDateTime   | DateTime |  End date of the requested date range  |
| Limit         | int      |       number of results per page       |

[See our API reference for more info](https://developers.deepgram.com/reference/get-all-requests).

## Get Request

Retrieves a specific request associated with the provided projectId.

```csharp
var result = await deepgramClient.Usage.GetUsageRequestAsync(projectId,requestId);
```

[See our API reference for more info](https://developers.deepgram.com/reference/get-request).

## Summarize Usage

Retrieves usage associated with the provided project_id based on the provided options.

```csharp
var getUsageSummmaryOptions = new GetUsageSummmaryOptions()
{
    StartDateTime = DateTime.Now
}
var result = await deepgramClient.Usage.GetUsageSummaryAsync(projectId,getUsageSummmaryOptions);
```

#### GetUsageSummaryOptions

| Property      | Value    |              Description               |
| ------------- | :------- | :------------------------------------: |
| StartDateTime | DateTime | Start date of the requested date range |
| EndDateTime   | DateTime |  End date of the requested date range  |
| Limit         | int      |       number of results per page       |

[See our API reference for more info](https://developers.deepgram.com/reference/summarize-usage).

## Get Fields

Lists the features, models, tags, languages, and processing method used for requests in the specified project.

```csharp
var getUsageFieldsOptions = new getUsageFieldsOptions()
{
    StartDateTime = Datetime.Now
}
var result = await deepgramClient.Usage.GetUsageFieldsAsync(projectId,getUsageFieldsOptions);
```

#### GetUsageFieldsOptions

| Property      | Value    |              Description               |
| ------------- | :------- | :------------------------------------: |
| StartDateTime | DateTime | Start date of the requested date range |
| EndDateTime   | DateTime |  End date of the requested date range  |

[See our API reference for more info](https://developers.deepgram.com/reference/get-fields).

# Logging

The Library uses Microsoft.Extensions.Logging to preform all of its logging tasks. To configure
logging for your app simply create a new `ILoggerFactory` and call the `LogProvider.SetLogFactory()`
method to tell the Deepgram library how to log. For example, to log to the console with Serilog, you'd need to install the Serilog package with `dotnet add package Serilog` and then do the following:

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Deepgram.Logger;
using Serilog;

var log = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm} [{Level}]: {Message}\n")
    .CreateLogger();
var factory = new LoggerFactory();
factory.AddSerilog(log);
LogProvider.SetLogFactory(factory);
```

# Development and Contributing

Interested in contributing? We ❤️ pull requests!

To make sure our community is safe for all, be sure to review and agree to our
[Code of Conduct](./.github/CODE_OF_CONDUCT.md). Then see the
[Contribution](./.github/CONTRIBUTING.md) guidelines for more information.

# Testing

The test suite is located within `Deepgram.Tests/`. Run all tests with the following command from the top-level repository:

```bash
dotnet test
```

Upon completion, a summary is printed:

```bash
Passed!  - Failed:     0, Passed:    69, Skipped:     0, Total:    69, Duration: 906 ms - Deepgram.Tests.dll (net7.0)
```

# Getting Help

We love to hear from you so if you have questions, comments or find a bug in the
project, let us know! You can either:

- [Open an issue in this repository](https://github.com/deepgram-devs/deepgram-dotnet-sdk/issues/new)
- [Join the Deepgram Github Discussions Community](https://github.com/orgs/deepgram/discussions)
- [Join the Deepgram Discord Community](https://dpgr.am/discord)

[license]: LICENSE.txt
