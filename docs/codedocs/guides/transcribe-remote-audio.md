---
title: "Transcribe Remote Audio"
description: "Submit a remote audio URL to Deepgram and return a formatted transcript from a .NET console app."
---

This guide covers the most direct speech-to-text workflow in the SDK: submitting a remote audio URL to `ListenRESTClient`. It follows the same pattern used in `examples/speech-to-text/rest/url/Program.cs`, but trims it down to the practical pieces you need in an application.

<Steps>
<Step>
### Install the package

```bash
dotnet add package Deepgram
```

</Step>
<Step>
### Set your API key

```bash
export DEEPGRAM_API_KEY="your_api_key"
```

</Step>
<Step>
### Create and call the client

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
        Punctuate = true,
        Keyterm = new List<string> { "Bueller" }
    },
    addons: new Dictionary<string, string>
    {
        ["smart_format"] = "true"
    });

var transcript = response.Results?.Channels?[0].Alternatives?[0].Transcript;
Console.WriteLine(transcript);

Library.Terminate();
```

</Step>
</Steps>

Why this works:

- `ClientFactory.CreateListenRESTClient()` returns the current REST transcription client.
- `UrlSource` serializes to `{ "url": "..." }`.
- `PreRecordedSchema` becomes query parameters or request settings handled by the SDK's REST abstraction.
- The optional `addons` dictionary lets you attach request-level flags without mutating global client options.

Expected behavior:

- The SDK resolves credentials from `DEEPGRAM_API_KEY` if you do not pass an explicit key.
- `TranscribeUrl` sends a JSON body containing the remote URL instead of uploading media bytes from your process.
- The result comes back as `SyncResponse`, so you can immediately inspect `Results`, `Channels`, and transcript alternatives.

Common adjustments:

- Add `Diarize = true` when you need speaker attribution.
- Add `DetectEntities = true` or `DetectTopics = true` when you are feeding the transcript into downstream automation.
- Switch to `TranscribeUrlCallBack` if the audio is long enough that you would rather process results asynchronously through a webhook.

If you want to process local files instead of URLs, switch to `TranscribeFile(byte[] ...)` or `TranscribeFile(Stream ...)` as documented in [ListenRESTClient](/docs/api-reference/listen-rest-client).
