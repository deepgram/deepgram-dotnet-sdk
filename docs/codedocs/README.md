# Code Documentation Corpus

This directory is the Deepgram-owned source corpus for Context7 code documentation. It is maintained alongside the SDK and verified against the 7.1.1 release surface.

Use the repository [README](../../README.md) and the product documentation at [developers.deepgram.com](https://developers.deepgram.com/docs) for the primary SDK and API documentation.

## Updating This Corpus

1. Update affected pages whenever a public client, model, lifecycle behavior, or example changes.
2. Verify names and signatures against `Deepgram/ClientFactory.cs`, `Deepgram/Clients/Interfaces/`, and runnable examples under `examples/`.
3. Run `dotnet build Deepgram.sln --configuration Release` and `dotnet test Deepgram.sln` when the local toolchain is available.
4. Keep `index.md` links current so new API surfaces are discoverable. The `Update Context7 Documentation` GitHub workflow refreshes the hosted index after published releases.
