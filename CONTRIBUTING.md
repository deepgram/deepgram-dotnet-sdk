# Contributing

Thanks for your interest in contributing to the Deepgram .NET SDK. The contribution policy, code of conduct, and GitHub workflow live under `.github/`:

- [Contributing Guidelines](./.github/CONTRIBUTING.md): contribution types, first issues, and how code contributions are reviewed
- [Development Guide](./.github/CODE_CONTRIBUTIONS_GUIDE.md): preparing macOS, Windows, or Linux and installing the .NET 8.0 SDK
- [GitHub Workflow](./.github/GITHUB_WORKFLOW.md): fork, branch, commit, rebase, and pull request steps
- [Branch and Release Process](./.github/BRANCH_AND_RELEASE_PROCESS.md): how `main`, `release-v*`, and tags relate
- [Code of Conduct](./.github/CODE_OF_CONDUCT.md)

This page holds the commands. [AGENTS.md](./AGENTS.md) holds the repository map, the client surfaces, and the conventions.

## Prerequisites

- .NET SDK 8.0 (CI pins `8.0.x`)
- A Deepgram API key from the [Deepgram Console](https://console.deepgram.com/signup?jump=keys), exported as `DEEPGRAM_API_KEY`, for the examples and live tests

## Build and test

```bash
# Restore packages for the SDK, the tests, and the microphone helper.
dotnet restore Deepgram.sln

# Build in Release, the way the CI build job does.
dotnet build Deepgram.sln --configuration Release --no-restore

# Run the unit tests (412 at 7.1.1; the 6 live tests skip without a key).
dotnet test Deepgram.sln

# Run one test class.
dotnet test Deepgram.Tests/Deepgram.Tests.csproj --filter FullyQualifiedName~FluxSpeakClientTests

# Compile every example and edge-case project as well.
dotnet build Deepgram.Dev.sln
```

## Run an example

```bash
export DEEPGRAM_API_KEY="<your key>"
dotnet run --project examples/speech-to-text/rest/url/PreRecorded.csproj
```

Examples end with `Console.ReadKey()`, so run them in an interactive terminal.

## Making changes

1. Fork the repository and create a branch from `main`.
2. Make the change, add or update a test under `Deepgram.Tests/UnitTests`, and update the affected example and `README.md` section.
3. Run `dotnet test Deepgram.sln` and `dotnet build Deepgram.Dev.sln`.
4. Commit with a [Conventional Commits](https://www.conventionalcommits.org/) message (`feat:`, `fix:`, `docs:`, `chore:`).
5. Open a pull request against `main` and fill in `.github/PULL_REQUEST_TEMPLATE.md`.

Do not reformat files you are not otherwise changing; `dotnet format --verify-no-changes` is not part of CI and reports pre-existing import ordering.

## License

By contributing, you agree that your contributions will be licensed under the MIT License.
