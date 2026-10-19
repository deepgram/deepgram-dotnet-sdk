# Changelog

## [7.2.0](https://github.com/deepgram/deepgram-dotnet-sdk/compare/7.1.3...7.2.0) (2026-10-13)


### Features

* **self-hosted:** Add `CreateSelfHostedDistributionCredentialsClient()` for the current `/v1/projects/{project_id}/self-hosted/distribution/credentials` endpoints, including typed create options and responses, while retaining the legacy on-prem client. ([24adb8e](https://github.com/deepgram/deepgram-dotnet-sdk/commit/24adb8e8402b4d8cb975de777a4d2420370e5f9f))
* **agent:** Add `CreateAgentProtocolClient()` and typed Voice Agent protocol controls, events, acknowledgements, warnings, history, and latency reports without changing legacy `IAgentWebSocketClient` fallback behavior. ([80b4e6f](https://github.com/deepgram/deepgram-dotnet-sdk/commit/80b4e6f92dba8f7f5d1d70aa2761c1971b1f9b77))
* **agent:** Add `CreateAgentCatalogClient()` to list available Think models and support typed custom Think-provider frames with arbitrary JSON content. ([2eac499](https://github.com/deepgram/deepgram-dotnet-sdk/commit/2eac4999c228d2427e5d4168ec2efc7b59a08885))
* Add current API request and response fields across Aura REST and streaming TTS, Listen REST and WebSocket, Text Intelligence, Management, and Voice Agent models, including Model Improvement Program opt-out and metadata tags. ([971f8c9](https://github.com/deepgram/deepgram-dotnet-sdk/commit/971f8c99a48eb9af57c3ada7df9104fce87b7207))
* **flux:** Add Flux STT numeral formatting and number redaction options for connection setup and mid-session `Configure` updates. ([1cb7168](https://github.com/deepgram/deepgram-dotnet-sdk/commit/1cb7168e4351a53f64585934a4a74e3c44fb385b))
* **flux:** Add Flux TTS helpers for IPA pronunciation markers in REST and WebSocket text, plus REST-only pause markers. ([70db782](https://github.com/deepgram/deepgram-dotnet-sdk/commit/70db7821dc73edeaadc144f5efb91151181c31f6))
* **listen:** Add `SendConfigure` for Nova-3 streaming so callers can update keyterms and feature flags during a session, with typed server rejection details. ([95bd033](https://github.com/deepgram/deepgram-dotnet-sdk/commit/95bd033acfcd0136c476000d6d0794e44ee5ebf4))
* **manage:** Add `CreateManagementReportingClient()` with typed usage and billing breakdowns, billing fields, purchases, and reporting filters while preserving existing `IManageClient` APIs. ([d2faa81](https://github.com/deepgram/deepgram-dotnet-sdk/commit/d2faa81092f9e778009e02a14a959e09d2ea4171))
* **speak:** Add `MipOptOut` to the current Speak WebSocket options so Aura streaming callers can send `mip_opt_out=true`. ([99a3a5d](https://github.com/deepgram/deepgram-dotnet-sdk/commit/99a3a5d14dd27cd6313205cc41c423e045c8827b))


### Bug Fixes

* **manage:** Use `decimal` rather than `double` for billing and purchase amounts to preserve exact currency values in sums and comparisons. ([730100a](https://github.com/deepgram/deepgram-dotnet-sdk/commit/730100a7eccfacf5ca663c9adb2f5c209cb54263))
* Mark the legacy self-hosted client factory and concrete client types obsolete, and direct callers to `SelfHostedDistributionCredentialsClient` for the current self-hosted distribution credentials routes. ([db2bf04](https://github.com/deepgram/deepgram-dotnet-sdk/commit/db2bf04517444d5734c76866f2439fef31621b0c))

## [7.1.3](https://github.com/deepgram/deepgram-dotnet-sdk/compare/7.1.2...7.1.3) (2026-10-05)


### Bug Fixes

* **Listen v1 WebSocket:** `LiveSchema.Model` is optional. `Connect` no longer throws `NullReferenceException` when a caller omits the model, with or without keyterms. Keyterms with an explicitly non-Nova-3 model still fail validation with `DeepgramException`. ([4fa3fe1](https://github.com/deepgram/deepgram-dotnet-sdk/commit/4fa3fe135148741cafce7f1c8ec768108fc6737c))

## [7.1.2](https://github.com/deepgram/deepgram-dotnet-sdk/compare/7.1.1...7.1.2) (2026-09-30)


### Bug Fixes

* **websocket:** Surface unknown, malformed, and non-object server frames through the existing `Unhandled` event instead of dropping them or treating them as fatal ([829305a](https://github.com/deepgram/deepgram-dotnet-sdk/commit/829305a07e1f5c223aad400f2ebcd091e0e9313e), [3fb72e8](https://github.com/deepgram/deepgram-dotnet-sdk/commit/3fb72e851afea78cf05e84df1de9e6ae92a32842), [668a3ff](https://github.com/deepgram/deepgram-dotnet-sdk/commit/668a3ff69c5bd216a9b5bfe8667f0a9bd78ab1b5), [26f5910](https://github.com/deepgram/deepgram-dotnet-sdk/commit/26f59104e5c4dd725d4f612a953784f4d16cf96a), [ed49d27](https://github.com/deepgram/deepgram-dotnet-sdk/commit/ed49d277980d82d722986a558e322598047fee6a))
* **websocket:** Add opt-in `DeepgramWebSocketException` errors for rejected WebSocket upgrades, including an inspectable HTTP status code; raw `WebSocketException` remains the default behavior in 7.x ([f581f81](https://github.com/deepgram/deepgram-dotnet-sdk/commit/f581f815cc6069c526b07f79862d138c82e2555b), [9e981b9](https://github.com/deepgram/deepgram-dotnet-sdk/commit/9e981b9907a9f027d446ead1b8b6941d3f8f1ab1), [3dee0cd](https://github.com/deepgram/deepgram-dotnet-sdk/commit/3dee0cd874d0792bb1bed55ecf00f72e6ec44529))
