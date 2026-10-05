# Changelog

## [7.1.3](https://github.com/deepgram/deepgram-dotnet-sdk/compare/7.1.2...7.1.3) (2026-10-05)


### Bug Fixes

* **listen:** allow Connect without a model on the v2 websocket client ([4fa3fe1](https://github.com/deepgram/deepgram-dotnet-sdk/commit/4fa3fe135148741cafce7f1c8ec768108fc6737c))

## [7.1.2](https://github.com/deepgram/deepgram-dotnet-sdk/compare/7.1.1...7.1.2) (2026-09-30)


### Bug Fixes

* **websocket:** Surface unknown, malformed, and non-object server frames through the existing `Unhandled` event instead of dropping them or treating them as fatal ([829305a](https://github.com/deepgram/deepgram-dotnet-sdk/commit/829305a07e1f5c223aad400f2ebcd091e0e9313e), [3fb72e8](https://github.com/deepgram/deepgram-dotnet-sdk/commit/3fb72e851afea78cf05e84df1de9e6ae92a32842), [668a3ff](https://github.com/deepgram/deepgram-dotnet-sdk/commit/668a3ff69c5bd216a9b5bfe8667f0a9bd78ab1b5), [26f5910](https://github.com/deepgram/deepgram-dotnet-sdk/commit/26f59104e5c4dd725d4f612a953784f4d16cf96a), [ed49d27](https://github.com/deepgram/deepgram-dotnet-sdk/commit/ed49d277980d82d722986a558e322598047fee6a))
* **websocket:** Add opt-in `DeepgramWebSocketException` errors for rejected WebSocket upgrades, including an inspectable HTTP status code; raw `WebSocketException` remains the default behavior in 7.x ([f581f81](https://github.com/deepgram/deepgram-dotnet-sdk/commit/f581f815cc6069c526b07f79862d138c82e2555b), [9e981b9](https://github.com/deepgram/deepgram-dotnet-sdk/commit/9e981b9907a9f027d446ead1b8b6941d3f8f1ab1), [3dee0cd](https://github.com/deepgram/deepgram-dotnet-sdk/commit/3dee0cd874d0792bb1bed55ecf00f72e6ec44529))
