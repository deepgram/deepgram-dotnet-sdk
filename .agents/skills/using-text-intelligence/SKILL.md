---
name: using-text-intelligence
description: Use when writing or reviewing C# code in this repo that calls Deepgram Text Intelligence / Read (`/read`) for sentiment, summarization, topic detection, and intent recognition on text or hosted text URLs. Covers `ClientFactory.CreateAnalyzeClient()` with `AnalyzeText`, `AnalyzeUrl`, and `AnalyzeFile`. Use `using-audio-intelligence` when the source is audio instead of text.
---

# Using Deepgram Text Intelligence (.NET SDK)

Analyze text input for sentiment, summary, topics, and intents.

## When to use this product

- You already have text: transcript, email, document, chat log, etc.
- You want REST-style analysis, not streaming.

**Use a different skill when:**
- The source is audio and you want analytics during transcription → `using-audio-intelligence`.

## Authentication

```bash
dotnet add package Deepgram
```

```csharp
using Deepgram;

Library.Initialize();
var client = ClientFactory.CreateAnalyzeClient();
```

## Quick start — text

```csharp
using Deepgram;
using Deepgram.Models.Analyze.v1;

Library.Initialize();

var client = ClientFactory.CreateAnalyzeClient();
var response = await client.AnalyzeText(
    new TextSource("Hello, world! This is a sample text for analysis."),
    new AnalyzeSchema()
    {
        Language = "en",
        Sentiment = true,
        Summarize = true,
        Topics = true,
        Intents = true,
    });

Console.WriteLine(response);
```

## Quick start — file / URL

```csharp
var client = ClientFactory.CreateAnalyzeClient();

var fileBytes = File.ReadAllBytes("conversation.txt");
var fileResponse = await client.AnalyzeFile(
    fileBytes,
    new AnalyzeSchema() { Language = "en", Summarize = true });

var urlResponse = await client.AnalyzeUrl(
    new UrlSource("https://example.com/conversation.txt"),
    new AnalyzeSchema() { Language = "en", Sentiment = true });
```

## Key params

`AnalyzeSchema`: `Language`, `Sentiment`, `Summarize`, `Topics`, `Intents`, `CustomTopic`, `CustomTopicMode`, `CustomIntent`, `CustomIntentMode`, `CallBack`, `CallbackMethod`.

Input types:
- `TextSource`
- `UrlSource`
- `byte[]`
- `Stream`

## API reference (layered)

1. **In-repo source of truth**:
   - `Deepgram/ClientFactory.cs`
   - `Deepgram/Clients/Analyze/v1/Client.cs`
   - `Deepgram/Models/Analyze/v1/AnalyzeSchema.cs`
   - `Deepgram/Models/Analyze/v1/*.cs`
2. **Canonical OpenAPI (REST)**: https://developers.deepgram.com/openapi.yaml
3. **AsyncAPI**: not applicable for this product in this repo
4. **Context7**:
   - repo mirror: `https://context7.com/deepgram/deepgram-dotnet-sdk`
   - docs corpus: `/llmstxt/developers_deepgram_llms_txt`
5. **Product docs**:
   - https://developers.deepgram.com/reference/text-intelligence/analyze-text
   - https://developers.deepgram.com/docs/text-intelligence

## Gotchas

1. **`AnalyzeClient` maps to `/read`.** The repo uses `Analyze*` method names, not `Read*` names.
2. **Use `AnalyzeText(...)` for raw strings.** `AnalyzeFile(...)` is for bytes/streams, even if the file is plain text.
3. **`Summarize` is a boolean here.** Do not use the STT-style `"v2"` string from `/listen` summarization.
4. **Language is effectively English-only for the gated analytics.** The schema docs say `en` only today.
5. **Callback flows are separate methods.** Use `AnalyzeTextCallBack`, `AnalyzeUrlCallBack`, or `AnalyzeFileCallBack`.

## Example files in this repo

- `examples/analyze/summary/Program.cs`
- `examples/analyze/sentiment/Program.cs`
- `examples/analyze/topic/Program.cs`
- `examples/analyze/intent/Program.cs`

## Central product skills

For cross-language Deepgram product knowledge — the consolidated API reference, documentation finder, focused runnable recipes, third-party integration examples, and MCP setup — install the central skills:

```bash
npx skills add deepgram/skills
```

This SDK ships language-idiomatic code skills; `deepgram/skills` ships cross-language product knowledge (see `api`, `docs`, `recipes`, `examples`, `starters`, `setup-mcp`).
