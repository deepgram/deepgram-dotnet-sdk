# Deepgram .NET SDK

[![Nuget](https://img.shields.io/nuget/v/deepgram)](https://www.nuget.org/packages/Deepgram) [![Build Status](https://github.com/deepgram-devs/deepgram-dotnet-sdk/workflows/CI/badge.svg)](https://github.com/deepgram-devs/deepgram-dotnet-sdk/actions?query=CI) [![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-v2.0%20adopted-ff69b4.svg)](./.github/CODE_OF_CONDUCT.md) [![Discord](https://dcbadge.vercel.app/api/server/xWRaCDBtW4?style=flat)](https://discord.gg/xWRaCDBtW4)

Official .NET SDK for [Deepgram](https://www.deepgram.com/). Power your apps with world-class speech and Language AI models.

> This SDK only supports hosted usage of api.deepgram.com.

- [Deepgram .NET SDK](#deepgram-net-sdk)
- [Getting an API Key](#getting-an-api-key)
- [Documentation](#documentation)
- [Installation](#installation)
- [Targeted Frameworks](#targeted-frameworks)
- [Configuration](#configuration)
  - [Default](#default) 
   - [Notes regarding Cors](#notes-regarding-cors)
  - [Examples](#examples)  
- [Creating A Rest Client](#creating-a-rest-client)
  - [Default Client Creation](#default-client-creation)    
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
  - [Leave Project](#leave-project)
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
- [Balances](#balances)
  - [Get Balances](#get-balances) 
  - [Get Balance](#get-balance) 
- [OnPrem](#onprem)
  - [List Credentials](#list-credentials)
  - [Get Credentials](#get-credentials)
  - [Remove Credentials](#remove-credentials)
  - [Create Credentials](#create-credentials)
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

```csharp
dotnet add package Deepgram
```

Or use the Nuget package Manager.

Right click on project and select manage nuget packages

# Targeted Frameworks

- 8.0.x
- 7.0.x
- 6.0.x
- NetStandard2.0

# Configuration
Add to you ServiceCollection class

## to use default values 
```csharp     
    services.AddDeepgram(apiKey):
```

### With modified values
```csharp
    var options = new DeepgramOptions("apiKey")
    {
        BaseAddress  = "https://AcmeNemesisServices.com"
    }
    services.AddDeepgram(options)
```

#### Notes Regarding CORS
    deepgram api does not support COR requests


### Examples


# Creating a Client
The Sdk is configure to support Dependency injection. But you can also creat client with new()

## Create client with new()
To create rest clients to communitcate with the deepgram apis, instantiate them directly.
y

## Default Client Creation
```csharp
   
var manageClient = new ManageClient(apikey,httpClient);

    or

    var options = new DeepgramClientOptions("yourapikey");
    var manageClient = new ManageClient(options,httpClient);
```


## Customized & OnPrem Client Creation
>If you  need to customize the urlmset optional headers or change the deault HttpClient timeout then you can when creating  a client 
>passing in a instance of DeepgramClientOptions. 
```csharp
 var options = new DeepgramOptions("apiKey")
    {
        BaseAddress  = "https://ScoobyDooDetectives.org"
    }
var manageClient = new ManageClient(options,HttpClient);
```

### DeepgramClientOptions
| Property         | Value                       |          Description                   |
| --------         | :-----                      | :---------------------------:          |
| BaseAddress      | string?                     | url of server, include protocol        |
| Headers          | Dictionary<string, string>? | any headers that you want to add       |
| HttpTimeout      | TimeSpan?                   | Optional Timeout setting for HttpClient|
| ApiKey           | string                      | Deepgram API Key|

#### Notes on client Creaton
Options and predefined HttpCLients take presedence over values passed in by throught the ServiceCollection
if a HttpClient has a base address - the SDK will not check it is properly formatted with protocols attached

if you use the new() approach you need to pass in the HttpClient instance- it is recommend you use the name client type
as it has Polly wait and retry polices set on it.


**Default.HTTPCLIENT_NAME** is a const string included in the SDK
```csharp
    var httpClient = httpClientFactory.CreateClient(Default.HTTPCLIENT_NAME);
```

if you decided on creating a HttpClient without using the name type included in the SDK. 
the HttpClient and HttpClientMessageHandler Lifetime and resource management are your responsiblity
the only thing that the SDK can change at this point is the Timeout

if you are using a console app and dont have access to the service collection create a local service collection.
An example of doing this  -- 
```csharp
    var services = new ServiceCollection();
    var httpClientFactory = services.BuildServiceProvider();
    var client = _serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(Defaults.HTTPCLIENT_NAME);
    var deepgramClientOptions = new DeepgramClientOptions("yourapikey");

    var manageClient = new ManageClient(deepgramClientOptions,client);
    
```

>UserAgent & Authorization headers are added internally

>Timeout can also be set by callling the RestClients SetTimeout(Timespan)


# Examples

To quickly get started with examples for prerecorded and streaming, run the files in the example folder. See the README in that folder for more information on getting started.

# Transcription

## Remote File
> for available options see PrerecordedSchema
```csharp
var client = new PrerecordedClient(apiKey);
var response = await client.TranscribeUrlAsync(
    new UrlSource("https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav"),
    new PrerecordedSchema()
    {
        Punctuate = true
    });
```

### UrlSource

| Property | Value  |          Description          |
| -------- | :----- | :---------------------------: |
| Url      | string | Url of the file to transcribe |

## Local files
>There are 2 overloads for local files you can pass either a byte[] or a stream 

```csharp
var client = new PrerecordedClient(apiKey);
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
also see [TranscribeSchema] which prerecored Schema is derived from for more options

| Property          | Value Type |                                                      reason for                                    |
| ---------------   | :--------- | :------------------------------------------------------------------------------------------------: | 
|DetectEntities     | bool?      | Entity Detection identifies and extracts key entities from content in submitted audio              |
|DetectLanguage     | bool?      | Language Detection identifies the dominant language spoken in submitted audio.                     |
|DetectTopics       | bool?      |Topic Detection identifies and extracts key topics from content in submitted audio.                 |
|Dictation          | bool?      |Spoken dictation commands will be converted to their corresponding punctuation marks. e.g., comma to|
|Measurements       | bool?      | Spoken measurements will be converted to their corresponding abbreviations. e.g., milligram to mg  |
|Ner bool?          |||
|Paragraphs         | bool?      | Paragraphs splits audio into paragraphs to improve transcript readability.                         |
|Sentiment          | bool       ||
|SentimentThreshold | double     ||
|Summarize          | object?    |Summarizes content of submitted audio                                                               |
|Utterances         | bool       |Utterances segments speech into meaningful semantic units.                                          |
|UttSplit           | double?    |Utterance Split detects pauses between words in submitted audio.                                    |



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
    deepgramLive.MetaDataReceivedReceived += HandleMetadataReceived;

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

### LiveSchema
also see [TranscribeSchema] which LiveSchema is derived from for more options

| Property               | Type          |                                                               Description                                              |
| ---------------------- | :-------      | :--------------------------------------------------------------------------------------------------------------------- |
| Channels               | int?          | Number of independent audio channels contained in submitted streaming audio                                            |
| Encoding               | string?       | Encoding allows you to specify the expected encoding of your submitted audio.                                          |
| EndPointing            | string?       | Indicates whether Deepgram will detect whether a speaker has finished speaking                                         |
| InterimResults         | bool?         | Indicates whether the streaming endpoint should send you updates to its transcription as more audio becomes available  |
| SampleRate             | int?          | Sample rate of submitted streaming audio. Required (and only read) when a value is provided for encoding               |
| UtteranceEnd           | string?       | Indicates how long Deepgram will wait to send a {"type": "UtteranceEnd"} message after a word has been transcribed     |


### TranscribeSchema
| Property               | Type          |                                                               Description                                              |
| ---------------------- | :-------      | :--------------------------------------------------------------------------------------------------------------------- |
| Alternatives           | int?          | Number of transcripts to return per request                                                                            |
| Callback               | string?       | Callback URL to provide if you would like your submitted audio to be processed asynchronously                          |
| Diarize                | bool?         | Indicates whether to recognize speaker changes                                                                         |
| Diarize                | string?       |                                                                                                                        |
| FillerWords            | int?          | Whether to include words like "uh" and "um" in transcription output.                                                   |
| Keywords               | List<string>? | Keywords to which the model should pay particular attention to boosting or suppressing to help it understand context   |
| Language               | string?       | BCP-47 language tag that hints at the primary spoken language                                                          |
| Model                  | string?       | AI model used to process submitted audio                                                                               |
| MultiChannel           | bool?         | Indicates whether to transcribe each audio channel independently                                                       |
| Numerals               | bool?         | Indicates whether to convert numbers from written format                                                               |
| ProfanityFilter        | bool?         | Indicates whether to remove profanity from the transcript                                                              |
| Punctuate              | bool?         | Indicates whether to add punctuation and capitalization to the transcript                                              |
| Redact                 | List<string>? | Indicates whether to redact sensitive information                                                                      |
| Replace                | List<string>? | Terms or phrases to search for in the submitted audio and replace                                                      |
| Search                 | List<string>? | Terms or phrases to search for in the submitted audio                                                                  |
| SmartFormat            | bool?         | Indicates whether to use Smart Format on the transcript                                                                |
| Tag                    | List<string>? |                                                                                                                        |
| Tier                   | string?       | Level of model you would like to use in your request                                                                   |
| Version                | string?       | Version of the model to use                                                                                            |


# Projects

> projectId and memberId are of type `string`

## Get Projects

Returns all projects accessible by the API key.

```csharp
    var result = await manageClient.GetProjectsAsync();
```

[See our API reference for more info](https://developers.deepgram.com/reference/get-projects).

## Get Project

Retrieves a specific project based on the provided projectId.

```csharp
var result = await manageClient.GetProject(projectId);
```

[See our API reference for more info](https://developers.deepgram.com/reference/get-project).

## Update Project

Update a project.

```csharp
var updateProjectSchema = new UpdateProjectSchema()
{
    Company = "Acme",
    Name = "Mega Speech inc"
}
var result = await manageClient.UpdateProjectAsync(projectid,updateProjectSchema);

```

**UpdateProjectSchema Type**

| Property Name | Type   |                       Description                        |
| ------------- | :----- | :------------------------------------------------------: |
| Name          | string |                   Name of the project                    |
| Company       | string | Name of the company associated with the Deepgram project |

[See our API reference for more info](https://developers.deepgram.com/reference/update-project).

## Delete Project

Delete a project.

```csharp
manageClient.DeleteProject(projectId);
```

## Leave Project

Leave a project.

```csharp
var result = await manageClient.LeaveProjectAsync(projectId);
```

[See our API reference for more info](https://developers.deepgram.com/reference/leave-project).

# Keys

> projectId,keyId and comment are of type`string`

## List Keys

Retrieves all keys associated with the provided project_id.

```csharp
var result = await manageClient.GetProjectAsync(projectId);
```

[See our API reference for more info](https://developers.deepgram.com/reference/list-keys).

## Get Key

Retrieves a specific key associated with the provided project_id.

```csharp
var result = await manageClient.GetProjectKeyAsync(projectId,keyId);
```

[See our API reference for more info](https://developers.deepgram.com/reference/get-key).

## Create Key

Creates an API key with the provided scopes.

```csharp
var createProjectKeyWithExpirationSchema = new createProjectKeyWithExpirationSchema
    {
        Scopes= new List<string>{"admin","member"},
        Comment = "Yay a new key",
        Tags = new List<string> {"boss"}
        Expiration = DateTime.Now.AddDays(7);
};
var result = await manageClient.CreateProjectKey(projectId,createProjectKeyWithExpirationSchema);
```
>Required - Scopes, Comment
> You can set ExpirationDate or TimeToLive or neither, but you cannot set both

[See our API reference for more info](https://developers.deepgram.com/reference/create-key).

### CreateProjectKeySchema
| Property              | Type         | Required |              Description        |
| -------------         | :-------     | :--------|:-------------------------------:|
| Scopes                | List<string> |  *       | scopes for key                  |
| Comment               | DateTime     |  *       | comment description of key      |
| Tags                  | List<string> |          | Tag for key                     |
| ExpirationDate        | string       |          | Specfic data for key to expire  |
| TimeToLiveInSeconds   | string       |          | time to live in seconds         |



## Delete Key

Deletes a specific key associated with the provided project_id.

```csharp
manageClient.DeleteKey(projectId, keyId);
```

[See our API reference for more info](https://developers.deepgram.com/reference/delete-key).

# Members

> projectId and memberId are of type`string`

## Get Members

Retrieves account objects for all of the accounts in the specified project_id.

```csharp
var result = await manageClient.GetMembersAsync(projectId);
```

[See our API reference for more info](https://developers.deepgram.com/reference/get-members).

## Remove Member

Removes member account for specified member_id.

```csharp
var result = manageClient.RemoveProjectMember(projectId,memberId);
```

[See our API reference for more info](https://developers.deepgram.com/reference/remove-member).

# Scopes

> projectId and memberId are of type`string`


## Get Member Scopes

Retrieves scopes of the specified member in the specified project.

```csharp
var result = await manageClient.GetProjectMemberScopesAsync(projectId,memberId);
```

[See our API reference for more info](https://developers.deepgram.com/reference/get-member-scopes).

## Update Scope

Updates the scope for the specified member in the specified project.

```csharp
var scopeOptions = new UpdateProjectMemeberScopeSchema(){Scope = "admin"};
var result = await manageClient.UpdateProjectMemberScopeAsync(projectId,memberId,scopeOptions);
```

[See our API reference for more info](https://developers.deepgram.com/reference/update-scope).

# Invitations

## List Invites

Retrieves all invitations associated with the provided project_id.

```csharp
var result = await manageClient.GetProjectInvitesAsync(projectId);
```

[See our API reference for more info](https://developers.deepgram.com/reference/list-invites).

## Send Invite

Sends an invitation to the provided email address.

```csharp
var sendProjectInviteSchema = new SendProjectInviteSchema()
{
    Email = "awesome@person.com",
    Scope = "fab"
}
var result = manageClient.SendProjectInviteAsync(projectId,sendProjectInviteSchema)
```

[See our API reference for more info](https://developers.deepgram.com/reference/send-invites).

## Delete Invite

Removes the specified invitation from the project.

```csharp
 manageClient.DeleteProjectInvite(projectId,emailOfInvite)
```

[See our API reference for more info](https://developers.deepgram.com/reference/delete-invite).


# Usage

> projectId and requestId type`string`

## Get All Requests

Retrieves all requests associated with the provided projectId based on the provided options.

```csharp
var getProjectUsageRequestsSchema = new GetProjectUsageRequestsSchema ()
{
     Start = DateTime.Now.AddDays(-7);
};
var result = await manageClient.ListAllRequestsAsync(projectId,getProjectUsageRequestsSchema);
```

### GetProjectUsageRequestsSchema

| Property      | Type     |              Description               |
| ------------- | :------- | :------------------------------------: |
| Start         | DateTime | Start date of the requested date range |
| End           | DateTime |  End date of the requested date range  | required
| Limit         | int      |       number of results per page       |
| Status        | string   |     status of requests to search for   |

[See our API reference for more info](https://developers.deepgram.com/reference/get-all-requests).

## Get Request

Retrieves a specific request associated with the provided projectId.

```csharp
var result = await manageClient.GetProjectUsageRequestAsync(projectId,requestId);
```

[See our API reference for more info](https://developers.deepgram.com/reference/get-request).

## Summarize Usage

Retrieves usage associated with the provided project_id based on the provided options.

```csharp
var getProjectUsageSummarySchema = new GetProjectUsageSummarySchema ()
{
    StartDateTime = DateTime.Now
}
var result = await manageClient.GetProjectUsageSummaryAsync(projectId,getProjectUsageSummarySchema);
```

### GetProjectUsageSummarySchema

| Property        | Value    |              Description               |
| -------------   | :------- | :------------------------------------: |
| Start           | DateTime | Start date of the requested date range |
| End             | DateTime |  End date of the requested date range  |
| Accessor        | string   ||
| Model           | string   ||
| MultiChannel    | bool     ||
| InterimResults  | bool     ||
| Punctuate       | bool     ||
| Ner             | bool     ||
| Utterances      | bool     ||
| Replace         | bool     ||
| ProfanityFilter | bool     ||
| Keywords        | bool     ||
| DetectTopics    | bool     ||
| Diarize         | bool     ||
| Search          | bool     ||
| Redact          | bool     ||
| Alternatives    | bool     ||
| Numerals        | bool     ||
| SmartFormat     | bool     ||


[See our API reference for more info](https://developers.deepgram.com/reference/summarize-usage).

## Get Fields

Lists the features, models, tags, languages, and processing method used for requests in the specified project.

```csharp
var getProjectUsageFieldsSchema = new GetProjectUsageFieldsSchema()
{
    Start = Datetime.Now
}
var result = await manageClient.GetProjectUsageFieldsAsync(projectId,getProjectUsageFieldsSchema);
```

### GetProjectUsageFieldsSchema

| Property      | Value    |              Description               |
| ------------- | :------- | :------------------------------------: |
| Start         | DateTime | Start date of the requested date range |
| End           | DateTime |  End date of the requested date range  |

[See our API reference for more info](https://developers.deepgram.com/reference/get-fields).


# Balances

## Get Balances
Get balances associated with project
```csharp
var result = await manageClient.GetProjectBalancesAsync(projectId)
```
## Get Balance
Get Balance associated with id
```csharp
var result = await manageClient.GetProjectBalanceAsync(projectId,balanceId)
```

# OnPrem
OnPremClient methods

## List Credentials
list credenetials 
```csharp
var result = onPremClient.ListCredentialsAsync(projectId);
```

## Get Credentials
get the credentials associated with the credentials id
```csharp
var result = onPremClient.GetCredentialsASync(projectId,credentialsId);
```

## Remove Credentials
remove credentials associated with the credentials id
```csharp
var result = onPremClient.DeleteCredentialsASync(projectId,credentialsId);
```

## Create Credentials
```csharp
var createOnPremCredentialsSchema = new CreateOnPremCredentialsSchema()
 {
    Comment = "my new credentials",
    Scopes = new  List<string>{"team fab"},
    Provider = "Acme credential provider"
 }
var result = onPremClientCreateCredentialsAsync(string projectId,  createOnPremCredentialsSchema)
```

### CreateOnPremCredentialsSchema

| Property      | Value     |              Description               |
| ------------- | :-------  | :------------------------------------: |
| Comment       | string?   | comment to associate with credentials  |
| Scopes        | List<string>? | scopes for the credentials             |
| Provider      | string?   | provider for the credentials             |


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
The sdk will generate loggers with the cateroryName of the client being used for example 
 to get the logger for the ManageClient you would call

```csharp
LogProvider.GetLogger(nameof(ManageClient));
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
