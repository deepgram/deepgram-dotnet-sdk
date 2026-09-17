# Agents

Instructions for AI coding agents (Claude Code, Cursor, Codex, Copilot) and for humans working with them in this repository. `CLAUDE.md` includes this file.

## Repository purpose

This is the official .NET SDK for the Deepgram API, published to NuGet as `Deepgram` (the SDK) and `Deepgram.Microphone` (a capture helper). The latest release tag is `7.1.1`. Both packages target `net8.0` and `netstandard2.0`. The SDK is hand-written: there is no code generator, no `fern/` folder, and no `.fernignore`. Edit the source directly.

The package version is not stored in the repository. `Deepgram/Deepgram.csproj` has no `<Version>` element; the CD workflow passes the git tag as `-p:Version=<tag>` at pack time.

Never hardcode API keys or access tokens. Every client constructor and every `ClientFactory.Create*` method takes an optional `apiKey` and falls back to the `DEEPGRAM_API_KEY` environment variable (`DEEPGRAM_ACCESS_TOKEN` for a bearer token). Examples and tests rely on that fallback.

## Repository map

| Path | What lives there |
| --- | --- |
| `Deepgram/` | The SDK project. Top-level `*Client.cs` files are thin public entry points; the implementations live under `Clients/` |
| `Deepgram/Clients/<Product>/<clientVersion>/{REST,WebSocket}` | Client implementations. The folder version is the SDK client version, which is distinct from the API version (see the surfaces table) |
| `Deepgram/Clients/Flux/` | Flux STT (`WebSocket/`, `/v2/listen`) and Flux TTS (`Speak/REST`, `Speak/WebSocket`, `/v2/speak`) clients |
| `Deepgram/Clients/Interfaces/{v1,v2}` | Public client interfaces (`IListenRESTClient`, `IFluxSpeakWebSocketClient`, and so on) |
| `Deepgram/Models/<Product>/<version>/...` | Request schemas and response types; namespaces mirror the path (`Deepgram.Models.Listen.v1.REST`, `Deepgram.Models.Flux.Speak.WebSocket`) |
| `Deepgram/Abstractions/` | `AbstractRestClient` and `AbstractWebSocketClient`, the shared HTTP and WebSocket plumbing |
| `Deepgram/ClientFactory.cs` | Static factory that returns the current client for each product; `Library.cs` configures logging (`Library.Initialize()`) |
| `Deepgram/Constants/Defaults.cs` | `api.deepgram.com`, API version `v1`, and the environment variable names |
| `Deepgram.Tests/` | NUnit 4 unit tests (`UnitTests/`), fakes, and recorded fixtures (`Fixtures/Flux`, `Fixtures/FluxSpeak`) |
| `Deepgram.Microphone/` | The `Deepgram.Microphone` helper package (PortAudio) |
| `examples/` | One console project per scenario, grouped by product |
| `tests/edge_cases/`, `tests/expected_failures/` | Console programs run by hand against the live API to reproduce reconnect, keepalive, timeout, and error paths |
| `extras/live-smoke/` | Dockerized live smokes for Flux STT `ForceEndTurn` and the Voice Agent management endpoints; see its `README.md` |
| `.github/workflows/` | `CI.yml`, `tests-daily.yml`, `CD.yml`, `CD-dev.yml`, `context7.yml` |
| `.github/` | `CONTRIBUTING.md`, `CODE_CONTRIBUTIONS_GUIDE.md`, `GITHUB_WORKFLOW.md`, `BRANCH_AND_RELEASE_PROCESS.md`, `PULL_REQUEST_TEMPLATE.md` |
| `.agents/skills/` | Agent-agnostic skills for using this SDK (speech-to-text, conversational STT, text-to-speech, voice agent, audio intelligence, text intelligence, management API) |

Three solution files exist. `Deepgram.sln` holds only `Deepgram`, `Deepgram.Tests`, and `Deepgram.Microphone`; CI builds and tests it. `Deepgram.DevBuild.sln` holds the two packages and packs them as `Deepgram.Unstable.SDK.Builds` for pre-release tags. `Deepgram.Dev.sln` adds every example and edge-case project (103 projects) for Visual Studio.

## Client surfaces

Every row below was checked against `Deepgram/ClientFactory.cs` and the `UriSegments.cs` files on 2026-09-13.

| Product | Endpoint | Factory method | Implementation | Status |
| --- | --- | --- | --- | --- |
| Speech-to-text, pre-recorded | `POST /v1/listen` | `CreateListenRESTClient` | `Clients/Listen/v1/REST` | Shipped |
| Speech-to-text, streaming (Nova) | `wss /v1/listen` | `CreateListenWebSocketClient` | `Clients/Listen/v2/WebSocket` (SDK client v2 of the v1 API) | Shipped |
| Flux STT (conversational speech-to-text) | `wss /v2/listen` | `CreateFluxWebSocketClient` | `Clients/Flux/WebSocket` | Shipped; `SendConfigure`, `SendForceEndTurn` |
| Text-to-speech, batch (Aura) | `POST /v1/speak` | `CreateSpeakRESTClient` | `Clients/Speak/v1/REST` | Shipped |
| Text-to-speech, streaming (Aura) | `wss /v1/speak` | `CreateSpeakWebSocketClient` | `Clients/Speak/v2/WebSocket` (SDK client v2 of the v1 API) | Shipped |
| Flux TTS, batch | `POST /v2/speak` | `CreateFluxSpeakRESTClient` | `Clients/Flux/Speak/REST` | Shipped |
| Flux TTS, streaming | `wss /v2/speak` | `CreateFluxSpeakWebSocketClient` | `Clients/Flux/Speak/WebSocket` | Shipped; `SendText`, `SendFlush`, `SendInterrupt`, `SendConfigure`, `Stop` |
| Voice Agent | `wss agent.deepgram.com/v1/agent/converse` | `CreateAgentWebSocketClient` | `Clients/Agent/v2/Websocket` | Shipped |
| Voice Agent management | `/v1/projects/{id}/agents`, `/agent-variables` | `CreateAgentManageClient` | `Clients/AgentManage/v1` | Shipped |
| Text intelligence | `POST /v1/read` | `CreateAnalyzeClient` | `Clients/Analyze/v1` | Shipped |
| Management API | `/v1/projects/...` | `CreateManageClient` | `Clients/Manage/v1` | Shipped |
| Self-hosted credentials | `/v1/projects/{id}/onprem/...` | `CreateSelfHostedClient` | `Clients/SelfHosted/v1` | Shipped |
| Auth (grant token) | `POST /v1/auth/grant` | `CreateAuthClient` | `Clients/Auth/v1` | Shipped |

`LiveClient`, `PreRecordedClient`, `SpeakClient`, and `OnPremClient` (and `Clients/Live`, `Clients/PreRecorded`, `Clients/OnPrem`) are deprecated and frozen. Point new code at `ListenWebSocketClient`, `ListenRESTClient`, `SpeakRESTClient`, and `SelfHostedClient`.

## Prerequisites

- .NET SDK 8.0. Every workflow pins `dotnet-version: "8.0.x"`. The library also targets `netstandard2.0`, so keep the source compatible with C# features that compile for both targets (`LangVersion` is `latest`, `Nullable` and `ImplicitUsings` are enabled).
- No native dependencies for the SDK or its tests. `Deepgram.Microphone` and the microphone examples need an audio device at run time.
- Docker, for `extras/live-smoke`.

## Build, test, lint, format

Every command in this table was run on 2026-09-13 inside a `mcr.microsoft.com/dotnet/sdk:8.0` container (SDK 8.0.424) with the repository mounted at `/work`; the exit codes are recorded in the pull request that added this file.

| Task | Command | Notes |
| --- | --- | --- |
| Restore | `dotnet restore Deepgram.sln` | First step of every CI job |
| Build | `dotnet build Deepgram.sln --configuration Release --no-restore` | The CI `build` job. `EnforceCodeStyleInBuild` and `AnalysisLevel` `latest` are on; at `7.1.1` the build emits analyzer warnings and zero errors. Do not add warning suppressions to get a build green |
| Tests, CI command | `dotnet test Deepgram.sln` | The CI `test` job on every pull request and push to `main` and `release-v*`. At `7.1.1`: 412 tests, 406 pass, 6 skipped without a key, about 20 seconds |
| Tests, one project | `dotnet test Deepgram.Tests/Deepgram.Tests.csproj` | Same tests; `Deepgram.sln` contains no other test project |
| One test class | `dotnet test Deepgram.Tests/Deepgram.Tests.csproj --filter FullyQualifiedName~FluxSpeakClientTests` | NUnit filter syntax |
| Live tests | `DEEPGRAM_API_KEY=<key> dotnet test Deepgram.sln --configuration Release --no-restore` | What `tests-daily.yml` runs at 09:00 UTC. The six `Live_*` tests call `Assert.Ignore` when the key is unset; `GlobalTestEnvironment` snapshots the key at startup because `BearerTokenTests` clears it |
| Pre-release pack | `dotnet pack Deepgram.DevBuild.sln --configuration Release --no-restore --output ./dist -p:Version=0.0.0-dev.1` | What `CD-dev.yml` runs; produces `Deepgram.Unstable.SDK.Builds` and `Deepgram.Unstable.Microphone.Builds` |
| Format | `dotnet format Deepgram/Deepgram.csproj --verify-no-changes` | Not run by CI, and it fails at `7.1.1` on pre-existing `IMPORTS` ordering in about a dozen files. Do not reformat files you are not otherwise changing |

There is no lint step beyond the Roslyn analyzers that run during `dotnet build`.

## Run an example against the live API

Every example calls `Library.Initialize()` and constructs a client with no key argument, so the SDK reads `DEEPGRAM_API_KEY` from the environment.

```bash
# The SDK resolves DEEPGRAM_API_KEY from the environment.
export DEEPGRAM_API_KEY="<your key>"

# Transcribe a hosted file with Nova-3 and print the JSON response.
dotnet run --project examples/speech-to-text/rest/url/PreRecorded.csproj

# Synthesize a block of text with Flux TTS over REST and write output.mp3.
dotnet run --project examples/text-to-speech/rest/flux/FluxSpeakBatch.csproj

# Stream the bundled preamble-16k.wav through Flux STT, paced like a microphone, and print TurnInfo events.
cd examples/speech-to-text/websocket/flux && dotnet run --project Flux.csproj
```

Most examples end with `Console.ReadKey()`, so they need an interactive terminal; under a redirected stdin they print their result and then exit with an `InvalidOperationException` from `ReadKey`. The microphone examples (`speech-to-text/websocket/microphone`, `agent/websocket/simple`) need an audio device. The Voice Agent management examples create and delete resources in the project named by `DEEPGRAM_PROJECT_ID`, and the live API keeps a deleted variable's name reserved in that project, so point them only at a disposable project (details in `extras/live-smoke/README.md`).

## Implementation conventions

- Keep public changes additive. When you replace a client, leave the old one in place, mark it `[Obsolete]`, and add a "frozen" comment as the deprecated clients do.
- One product per client, one client version per folder. A new API version gets a new folder under `Clients/<Product>/` and new models under `Models/<Product>/`; do not widen an existing schema class to serve two wire formats.
- Schemas are classes with `[JsonPropertyName]` attributes and nullable properties, serialized with `System.Text.Json`. Match names and optionality to the wire contract, and omit unset properties (`JsonIgnoreCondition.WhenWritingNull`) so the server's strict field checks pass.
- WebSocket clients expose typed events through `Subscribe(EventHandler<TResponse>)`. Unknown message types must stay non-fatal: log and continue, so a new server frame cannot break a deployed client (`AgentUnknownMessageTests` and `FluxParityTests` guard this).
- Errors from REST calls surface as `DeepgramException` subclasses (`Deepgram/Models/Exceptions`); do not swallow them, and never log the `Authorization` header or the key.
- Logging goes through `Microsoft.Extensions.Logging` via `Library.Initialize()` or `Library.Configure(ILoggerFactory)`; do not write to `Console` from library code.
- Every `.cs` file starts with the MIT license header:

  ```csharp
  // Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
  // Use of this source code is governed by a MIT license that can be found in the LICENSE file.
  // SPDX-License-Identifier: MIT
  ```

- Tests are NUnit 4 with NSubstitute and FluentAssertions. Unit tests must pass without a key; anything that needs the live API calls `Assert.Ignore` when `DEEPGRAM_API_KEY` is unset and is named `Live_*`. Recorded server payloads go under `Deepgram.Tests/Fixtures`.
- Write "Flux STT" or "Flux TTS" in prose and comments; never bare "Flux". Identifiers such as `FluxWebSocketClient` and the `flux-general-en` model name stay as they are.
- Keep every example compiling. `Deepgram.Dev.sln` is the quickest way to check all 103 projects at once.

## Example: add a Flux TTS query parameter

1. Add the property with its `[JsonPropertyName]` to `Deepgram/Models/Flux/Speak/WebSocket/SpeakSchema.cs` (and the REST schema under `Models/Flux/Speak/REST` if the parameter applies to both transports).
2. If the parameter also changes a server event, extend the matching response class and add a recorded payload under `Deepgram.Tests/Fixtures/FluxSpeak`.
3. Add a unit test in `Deepgram.Tests/UnitTests/ClientTests/FluxSpeakClientTests.cs` that asserts the query string the client builds.
4. Update `examples/text-to-speech/websocket/flux` and the Flux TTS section of `README.md`.
5. Run `dotnet test Deepgram.Tests/Deepgram.Tests.csproj --filter FullyQualifiedName~FluxSpeak`, then `dotnet test Deepgram.sln`, then `dotnet build Deepgram.Dev.sln`.

## Release process

Releases are git tags on `main`; there is no release-please. The full process is in `.github/BRANCH_AND_RELEASE_PROCESS.md`.

1. `main` must stay releasable. Consumers install a tagged version from NuGet (`dotnet add package Deepgram --version 7.1.1`).
2. A maintainer tags with plain semver and no `v` prefix (`git tag -m 7.2.0 7.2.0 && git push upstream 7.2.0`). `CD.yml` matches `[0-9]+.[0-9]+.[0-9]+`, restores and builds `Deepgram.sln` in Release, packs with `-p:Version=<tag>`, and pushes both packages to nuget.org with the `NUGET_API_KEY` secret.
3. Pre-release tags (`7.2.0-dev.1`, `-alpha.N`, `-beta.N`, `-rc.N`) run `CD-dev.yml`, which packs `Deepgram.DevBuild.sln` as `Deepgram.Unstable.SDK.Builds`.
4. The maintainer then publishes a GitHub release from the tag, titled with the version and a short summary of the headline changes (the 7.1.1 release names the Agent `FunctionCallRequest` functions and the error code work); `context7.yml` refreshes the Context7 index when the release is published.
5. A breaking change bumps the major version and gets a `release-v<N>` branch for patches to the previous major.

## Pull requests

- Keep diffs focused on one issue or behavior change.
- Add regression coverage for bug fixes, including the exact query string or WebSocket frame when the bug is a serialization or protocol mismatch.
- State the commands you ran and link the related issue. Use `Fixes #<issue>` only when the pull request fully resolves it.
- Follow `.github/PULL_REQUEST_TEMPLATE.md`. Commit messages follow Conventional Commits (`feat:`, `fix:`, `docs:`, `ci:`, `chore:`).

## Documentation

- API reference and product guides: <https://developers.deepgram.com/docs>
- C# build pages: <https://developers.deepgram.com/docs/speech-to-text/build/csharp>, <https://developers.deepgram.com/docs/speech-to-text/streaming/build/csharp>, <https://developers.deepgram.com/docs/speech-to-text/flux/build/csharp>, <https://developers.deepgram.com/docs/text-to-speech/build/csharp>, <https://developers.deepgram.com/docs/text-to-speech/streaming/build/csharp>, <https://developers.deepgram.com/docs/text-to-speech/flux/build/csharp>, <https://developers.deepgram.com/docs/voice-agent/build/csharp>
- SDK feature matrix: <https://developers.deepgram.com/docs/sdks/sdk-features>
- NuGet: <https://www.nuget.org/packages/Deepgram>
- Agent skills that teach this SDK: `.agents/skills/` (install with `npx skills add deepgram/deepgram-dotnet-sdk`)

## Do not

- Do not commit keys, `.env` files, `bin/`, or `obj/` (`clean-up.sh` removes build output).
- Do not run `extras/live-smoke` or the `examples/agent/manage` programs against a production project.
- Do not add new code to the deprecated `Live`, `PreRecorded`, `Speak` (v1 wrapper), or `OnPrem` clients.
- Do not reformat files you are not otherwise changing, and do not suppress analyzer warnings to silence the build.
- Do not add a `<Version>` to a `.csproj`; the tag is the version.
