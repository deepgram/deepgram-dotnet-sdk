# Deepgram .NET SDK

[![Nuget](https://img.shields.io/nuget/v/deepgram)](https://www.nuget.org/packages/Deepgram) [![Build Status](https://github.com/deepgram-devs/deepgram-dotnet-sdk/workflows/CI/badge.svg)](https://github.com/deepgram-devs/deepgram-dotnet-sdk/actions?query=CI) [![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-v2.0%20adopted-ff69b4.svg)](./.github/CODE_OF_CONDUCT.md) [![Discord](https://dcbadge.vercel.app/api/server/xWRaCDBtW4?style=flat)](https://discord.gg/xWRaCDBtW4)

Official .NET SDK for [Deepgram](https://www.deepgram.com/). Power your apps with world-class speech and Language AI models.

> This SDK only supports hosted usage of api.deepgram.com.

- [Deepgram .NET SDK](#deepgram-net-sdk)
- [Getting an API Key](#getting-an-api-key)
- [Documentation](#documentation)
- [Installation](#installation)
- [Targeted Frameworks](#targeted-frameworks)
- [Creating A Rest Client](#creating-a-rest-client)
  - [Default Client Creation](#default-client-creation)   
  - [DeepgramClientOptions Creation](#deepgramclientoptions-creation)   
  - [Setting Proxy for CORS](#setting-proxy-for-cors)   
- [Examples](#examples)
- [Transcription](#transcription)
  - [Remote File](#remote-file) 
    - [UrlSource](#urlsource)
  - [Local files](#local-files)    
    - [PrerecordedSchema](#prerecordedschema)
  - [CallBackAsync Methods](#callbackasync-methods) 
  - [Live Audio](#live-audio)
    - [LiveSchema](#liveSchema)
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

# Creating a Client

To create rest clients to communitcate with the deepgram apis, instantiate them directly.
When creating a restclient you need to pass in the apikey and a HttpClientFactory

## Default Client Creation
>If you dont need to customize the url or set optional headers then you can create a client -
```csharp
var manageClient = new ManageClient(apiKey,httpClientFactory);
```

## DeepgramClientOptions Creation
>In order to point the SDK at a different API endpoint (e.g., for on-prem deployments), you can pass in an object setting the `API_URL` when initializing the Deepgram client.
```csharp
var deepgramClientOptions = new DeepgramClientOptions(){
 Url = "urlstring", // any protocol will be stripped out and replaced with https://
 Headers = new Dictonary<string,string>(){}       
};
var client = new ManageClient(apiKey,oppions,HttpClientFactory)
```

## Setting Proxy for CORS
>The Deepgram api will not accept CORS requests. Some apps, Such as Blazor Wasm, may throw Cross Origin Resource Sharing Errors,if your
>app throw this exception as you do not have access to the api endpoints you will need to create a proxy server and assign it to the IHttpClientFactory. To do this when
>adding AddHttpClient to you services you need to add the following- 
```csharp
  services.AddHttpClient("ProxyClient").ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            Proxy = new WebProxy("http://proxyserver:port", true),
            UseProxy = true
        };
    });
``` 
> If you set this up as a NamedClient as shown above you need to let the SDK know the name of the NamedClient
> you cna do this by setting the NamedClientName property of the DeepgramClientOptions.


# Examples

To quickly get started with examples for prerecorded and streaming, run the files in the example folder. See the README in that folder for more information on getting started.

# Transcription

## Remote File
> for available options see PrerecordedSchema
```csharp
var client = new PrerecordedClient(apiKey,HttpClientFactory);
var response = await client.TranscribeUrlAsync(
    new UrlSource("https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav"),
    new PrerecordedSchema()
    {
        Punctuate = true
    });
```

#### UrlSource

| Property | Value  |          Description          |
| -------- | :----- | :---------------------------: |
| Url      | string | Url of the file to transcribe |

## Local files
>There are 2 overloads for local files you can pass either a byte[] or a stream 

```csharp
var client = new PrerecordedClient(apiKey,HttpClientFactory);
var response = await client.TranscribeFileAsync(
    stream,
     new PrerecordedSchema()
    {
        Punctuate = true
    });
```

### CallBackAsync Methods
>TranscibeFileCallBackAsync and TranscibeUrlCallBackAsync are the methods to use if you want to use a CallBack url
>you can either pass the the CallBack in the method or by setting  the CallBack proeprty in the PrerecordedSchema, but NOT in both


#### PrerecordedSchema

| Property               | Value Type |                                                      reason for                                                      |       Possible values       |
| ---------------------- | :--------- | :------------------------------------------------------------------------------------------------------------------: | :-------------------------: |
| Model                  | string     |                                       AI model used to process submitted audio                                       |
| Version                | string     |                                             Version of the model to use                                              |
| Language               | string     |                            BCP-47 language tag that hints at the primary spoken language                             |
| Tier                   | string     |                                 Level of model you would like to use in your request                                 |
| Punctuate              | bool       |                      Indicates whether to add punctuation and capitalization to the transcript                       |
| ProfanityFilter        | bool       |                              Indicates whether to remove profanity from the transcript                               |
| Redact                 | string[]   |                                  Indicates whether to redact sensitive information                                   |      pci, numbers, ssn      |
| Diarize                | bool       |                                    Indicates whether to recognize speaker changes                                    |
| MultiChannel           | bool       |                           Indicates whether to transcribe each audio channel independently                           |
| Alternatives           | int        |                                 Maximum number of transcript alternatives to return                                  |
| Numerals               | bool       |                               Indicates whether to convert numbers from written format                               |
| SmartFormat            | bool       |                               Indicates whether to use Smart Format on the transcript                                |
| Search                 | string[]   |                                Terms or phrases to search for in the submitted audio                                 |
| Replace                | string[]   |                          Terms or phrases to search for in the submitted audio and replace                           |
| Callback               | string     |            Callback URL to provide if you would like your submitted audio to be processed asynchronously             |
| Keywords               | string[]   | Keywords to which the model should pay particular attention to boosting or suppressing to help it understand context |
| Utterances             | bool       |                    Indicates whether Deepgram will segment speech into meaningful semantic units                     |
| DetectLanguage         | bool       |                            Indicates whether to detect the language of the provided audio                            |
| Paragraphs             | bool       |                             Indicates whether Deepgram will split audio into paragraphs                              |
| UtteranceSplit         | decimal    |              Length of time in seconds of silence between words that Deepgram will use when determining              |
| Summarize              | object     |              Indicates whether Deepgram should provide summarizations of sections of the provided audio              |
| DetectEntities         | bool       |                     Indicates whether Deepgram should detect entities within the provided audio                      |
| DetectTopics           | bool       |                      Indicates whether Deepgram should detect topics within the provided audio                       |
| Tag                    | string[]   |                                                                                                                      |


## Live Audio

> The example below demonstrates sending a pre-recorded audio to simulate a real-time
> stream of audio. In a real application, this type of audio is better handled using the
> pre-recorded transcription.

```csharp
using Deepgram.CustomEventArgs;
using Deepgram.Models;
using System.Net.WebSockets;



var deepgramClient = new LiveClient(apiKey);

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

#### LiveSchema

| Property               | Type     |                                                               Description                                                               |             Possible values |
| ---------------------- | :------- | :-------------------------------------------------------------------------------------------------------------------------------------: | --------------------------: |
| Model                  | string   |                                                AI model used to process submitted audio                                                 |
| Version                | string   |                                                       Version of the model to use                                                       |
| Language               | string   |                                      BCP-47 language tag that hints at the primary spoken language                                      |
| Tier                   | string   |                                          Level of model you would like to use in your request                                           |
| Punctuate              | bool     |                                Indicates whether to add punctuation and capitalization to the transcript                                |
| ProfanityFilter        | bool     |                                        Indicates whether to remove profanity from the transcript                                        |
| Redact                 | string[] |                                            Indicates whether to redact sensitive information                                            |           pci, numbers, ssn |
| Diarize                | bool     |                                             Indicates whether to recognize speaker changes                                              |
| MultiChannel           | bool     |                                    Indicates whether to transcribe each audio channel independently                                     |
| Numerals               | bool     |                                        Indicates whether to convert numbers from written format                                         |
| SmartFormat            | bool     |                                         Indicates whether to use Smart Format on the transcript                                         |
| Search                 | string[] |                                          Terms or phrases to search for in the submitted audio                                          |
| Replace                | string[] |                                    Terms or phrases to search for in the submitted audio and replace                                    |
| Callback               | string   |                      Callback URL to provide if you would like your submitted audio to be processed asynchronously                      |
| Keywords               | string[] |          Keywords to which the model should pay particular attention to boosting or suppressing to help it understand context           |
| InterimResults         | bool     |          Indicates whether the streaming endpoint should send you updates to its transcription as more audio becomes available          |
| EndPointing            | string   |                             Indicates whether Deepgram will detect whether a speaker has finished speaking                              |
| Channels               | int      |                               Number of independent audio channels contained in submitted streaming audio                               |
| SampleRate             | int      |                Sample rate of submitted streaming audio. Required (and only read) when a value is provided for encoding                 |
| Tag                    | string[]   |                                                                                                                      |

#NEEDS TO BE REVIESED FROM HERE DOWN
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
