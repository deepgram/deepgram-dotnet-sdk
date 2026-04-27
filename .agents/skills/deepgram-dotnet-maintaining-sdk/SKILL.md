---
name: deepgram-dotnet-maintaining-sdk
description: Use when maintaining this hand-written Deepgram .NET SDK: editing client code, models, examples, tests, solution files, CI/CD packaging, or NuGet release flows. This repo is not Fern-generated. Covers `dotnet build`, `dotnet test`, `dotnet pack`, solution-file selection (`Deepgram.sln`, `Deepgram.Dev.sln`, `Deepgram.DevBuild.sln`), `clean-up.sh`, and NuGet publishing conventions from GitHub Actions.
---

# Maintaining the Deepgram .NET SDK

This SDK is hand-maintained C#/.NET code. It is **not** Fern-generated. Do not use Fern regeneration instructions from other Deepgram SDKs here.

## Repo shape

- `Deepgram/` — main SDK package (`Deepgram.csproj`)
- `Deepgram.Microphone/` — microphone helper package (`Deepgram.Microphone.csproj`)
- `Deepgram.Tests/` — unit tests
- `examples/` — runnable local examples
- `tests/` — edge cases / expected failure projects
- `clean-up.sh` — removes `.vs`, `dist`, `obj`, `bin`
- `extras/dg_logo.png` — package icon packed into NuGet packages

## Solution files

- `Deepgram.sln`
  - main CI/CD solution
  - contains `Deepgram`, `Deepgram.Tests`, `Deepgram.Microphone`
  - used by CI, release build, and stable NuGet packaging
- `Deepgram.Dev.sln`
  - broad developer solution
  - includes examples, edge cases, expected failures, and test/debug projects
- `Deepgram.DevBuild.sln`
  - packaging-only developer-build solution
  - contains `Deepgram` and `Deepgram.Microphone`
  - used for unstable/dev NuGet packages

## Build, test, pack

Stable validation:

```bash
dotnet restore Deepgram.sln
dotnet test Deepgram.sln
dotnet build Deepgram.sln --configuration Release --no-restore
```

Stable pack:

```bash
dotnet restore Deepgram.sln
dotnet build Deepgram.sln --configuration Release --no-restore
dotnet pack Deepgram.sln --configuration Release --no-restore --output ./dist -p:Version=<VERSION>
```

Developer / prerelease pack:

```bash
dotnet restore Deepgram.DevBuild.sln
dotnet build Deepgram.DevBuild.sln --configuration Release --no-restore
dotnet pack Deepgram.DevBuild.sln --configuration Release --no-restore --output ./dist -p:Version=<VERSION>
```

Local cleanup:

```bash
bash ./clean-up.sh
```

## NuGet package behavior

`Deepgram/Deepgram.csproj`:
- stable package ID on `Deepgram.sln`: `Deepgram`
- unstable package ID on `Deepgram.DevBuild.sln`: `Deepgram.Unstable.SDK.Builds`

`Deepgram.Microphone/Deepgram.Microphone.csproj`:
- stable package ID on `Deepgram.sln`: `Deepgram.Microphone`
- unstable package ID on `Deepgram.DevBuild.sln`: `Deepgram.Unstable.Microphone.Builds`

Both projects target:
- `net8.0`
- `netstandard2.0`

Both enable:
- nullable reference types
- implicit usings
- latest language version

## Release flow from GitHub Actions

From `.github/workflows/CD.yml`:
- restore `Deepgram.sln`
- build `Deepgram.sln`
- pack `Deepgram.sln` into `./dist`
- publish with `dotnet nuget push ./dist/**.nupkg --source nuget.org --api-key ${{secrets.NUGET_API_KEY}}`

From `.github/workflows/CD-dev.yml`:
- same flow, but against `Deepgram.DevBuild.sln`
- tag patterns include `-dev`, `-alpha`, `-beta`, `-rc`

## Contribution guidance

- Read `.github/CONTRIBUTING.md` first.
- This repo welcomes docs, tests, bug fixes, and feature work.
- For broad manual validation, use the example projects under `examples/`.
- Keep examples resettable; `examples/README.md` explicitly asks contributors to undo local test edits.

## Codebase-specific maintenance notes

1. **No Fern.** Do not add `.fernignore`, regen scripts, or “frozen file” workflows.
2. **Prefer the non-deprecated client surface.** Use `CreateListenRESTClient`, `CreateListenWebSocketClient`, `CreateSpeakRESTClient`, `CreateSpeakWebSocketClient`, `CreateManageClient`, `CreateAuthClient`, `CreateAgentWebSocketClient`.
3. **Deprecated classes still exist for compatibility.** Examples: `PreRecordedClient`, `LiveClient`, `SpeakClient`, `OnPremClient`. Avoid extending them for new features.
4. **Authentication supports both API keys and bearer access tokens.** `DeepgramHttpClientOptions` and `DeepgramWsClientOptions` implement priority-based credential selection.
5. **The microphone helper is a separate package.** Changes in streaming examples may affect both `Deepgram/` and `Deepgram.Microphone/`.
6. **Packaging metadata depends on the solution name.** If pack output looks wrong, confirm which `.sln` you used.

## Source-of-truth files for maintainers

- `README.md`
- `.github/CONTRIBUTING.md`
- `.github/workflows/CI.yml`
- `.github/workflows/CD.yml`
- `.github/workflows/CD-dev.yml`
- `Deepgram/Deepgram.csproj`
- `Deepgram.Microphone/Deepgram.Microphone.csproj`
- `clean-up.sh`
