# .NET SDK Parity Roadmap

## Purpose

Bring the hand-written .NET SDK to parity with the current Deepgram API contract and the Java, JavaScript, and Python SDKs without widening deprecated clients or introducing breaking changes.

This roadmap was prepared against the current .NET 7.1.2 worktree, the current API OpenAPI and AsyncAPI specifications in `../deepgram-docs`, and Java 0.11.0, JavaScript 5.13.0, and Python 7.12.0.

## Delivery Rules

- Implement all new functionality on the current clients only. Do not add features to `LiveClient`, `PreRecordedClient`, `SpeakClient`, or `OnPremClient`.
- Treat the OpenAPI and AsyncAPI documents as the wire-contract source of truth. Peer SDK behavior is corroborating evidence, not the contract.
- Preserve unknown WebSocket frame handling. New Agent frames must remain non-fatal when no handler is registered.
- Add exact wire tests for every new query parameter, control frame, route, and response payload.
- Do not add members to existing public interfaces. Adding an interface member source-breaks consumer implementations. Expose new Agent controls through extension methods over the existing interface and new endpoint families through narrow sibling interfaces and factories.
- Do not change the type, name, or serialized behavior of an existing public option or method. Defer a correction requiring that change until a separately approved major-version plan exists. Expose a current wire contract through a new additive client or model when necessary.
- Keep options nullable and omit unset values so server defaults remain authoritative.

## Current Status

The first three delivery slices are implemented and awaiting merge in this order:

1. [Phase 0: Contract Corrections](https://github.com/deepgram/deepgram-dotnet-sdk/pull/445)
2. [Phase 1: Flux STT Parity](https://github.com/deepgram/deepgram-dotnet-sdk/pull/446)
3. [Phase 2A: Voice Agent WebSocket Protocol Parity](https://github.com/deepgram/deepgram-dotnet-sdk/pull/447)

Phase 2A is based on the Phase 1 branch. After Phase 1 merges, retarget Phase 2A to `main` if GitHub does not do so automatically. The Agent Think-model catalog and custom Think-provider frames were deliberately deferred to Phase 2B; they are not part of the Phase 2A pull request.

## Phase 0: Contract Corrections

### Goal

Expose corrected API contracts without changing existing public behavior before adding new feature area.

### Work

1. Add a narrow self-hosted distribution-credentials client targeting `/v1/projects/{project_id}/self-hosted/distribution/credentials`.
2. Give the new client typed query options for `scopes` and `provider`, and retain only `comment` in the JSON body.
3. Document the pre-recorded `callback_method` Boolean limitation and the current server behavior: leave it unset to use the server default because the service rejects an explicit method parameter.
4. Add a current contract test for Aura REST numeric query parameters (`sample_rate` and `bit_rate`) and document the current string serialization without changing public property types.
5. Validate the current self-hosted route against the hosted service before release. Keep the existing `SelfHostedClient` unchanged; it remains the legacy compatibility surface.

### Compatibility Decision

Do not change `PreRecordedSchema.CallbackMethod` from `bool?` in this roadmap. A new correctly typed property would map to the same wire name and needs custom serialization; changing the existing property would be source-breaking. The live service currently rejects explicit `callback_method` query values, so this SDK must document the server-default behavior rather than offer an add-on workaround. Revisit the API-contract divergence and a first-class callback-method redesign only in a separately approved major-version plan.

### Acceptance Criteria

- A request construction test asserts the new self-hosted client's route and the exact split between query string and JSON body.
- A callback test verifies the legacy property is omitted when unset, preserving the server-default behavior.
- Existing self-hosted, listen REST, and full-solution tests pass.
- No existing public interface, method signature, property name, property type, or wire behavior changes.

## Phase 1: Flux STT Parity

### Goal

Expose current Flux STT formatting and privacy controls as first-class options.

### Work

1. Add `Numerals` to `FluxSchema` as a nullable Boolean query option.
2. Add `Redact` to `FluxSchema` using the Flux-supported values: `numbers` and `aggressive_numbers`.
3. Add `Numerals` to Flux `ConfigureSchema` so it can be changed during an active session.
4. Add the active numerals value to `ConfigureSuccessResponse`.
5. Update the Flux example and README to show connection-time redaction and a numerals Configure message.

### Acceptance Criteria

- Connection tests assert `numerals=true` and each supported `redact` value in the query string.
- Control-frame tests assert the exact Configure JSON shape for numerals.
- A fixture verifies that ConfigureSuccess deserializes the numerals echo.
- Flux tests pass without a live key; a live test is added only if the current Flux smoke suite is suitable for this behavior.

### Release Shape

This is additive and belongs in the next 7.x feature release. No interface change is necessary because `SendConfigure` already exists.

## Phase 2A: Voice Agent WebSocket Protocol Parity

### Goal

Bring Voice Agent control messages and events to current protocol coverage, with particular focus on safe function execution and Flux-backed turn control.

### Work

1. Add `DeferUntilEot` to the Agent `Function` model.
2. Add typed request models and extension methods for `UpdateListen`, `UpdateThink`, `UpdateSpeak`, `UpdatePrompt`, `InjectAgentMessage`, `FunctionCallResponse`, and `ForceEndTurn`.
3. Add new current-protocol request models rather than changing dormant schemas. The new InjectAgentMessage model serializes `message`, plus optional `behavior` (`default`, `queue`, or `interrupt`); the new FunctionCallResponse model serializes `id`, `name`, and `content`.
4. Add typed inbound models, dispatch entries, and subscriptions for `FunctionCallCancelled`, `ListenUpdated`, `ThinkUpdated`, `LatencyReport`, `Warning`, and `History`.
5. Add `IAgentProtocolClient` and `ClientFactory.CreateAgentProtocolClient()` for the new typed subscriptions and ordered `ForceEndTurn`, while preserving `IAgentWebSocketClient` unchanged.
6. Update the no-microphone Voice Agent example to use the protocol client and handle cancellation, warning, and latency events.

### Sequencing

Implement models and dispatcher support, then add extension methods over `IAgentWebSocketClient` for typed control messages that use the existing immediate-send path. Expose new typed subscriptions and ordered `ForceEndTurn` through `IAgentProtocolClient` and `ClientFactory.CreateAgentProtocolClient()` without modifying the legacy interface or factory return type. Concrete-client convenience methods may forward to the extensions.

### Acceptance Criteria

- Serialized Settings tests prove `defer_until_eot` is emitted only when specified.
- One wire test exists for each outbound control message and covers required properties.
- Recorded fixtures exercise every new inbound event, including a cancellation event containing multiple function IDs.
- Dispatcher tests prove all known frames select their typed handler and unsupported frames continue to reach `UnhandledResponse`.
- A cancellation test documents the required application behavior: never send a function response for a cancelled call ID.

### Live Validation Note

The Phase 2A live smoke validated connection, Flux-backed `ForceEndTurn`, `UpdateListen`, `UpdateSpeak`, `UpdatePrompt`, injected-agent audio, history, and latency reports. The live service accepted a valid `UpdateThink` message without an error but did not emit the documented `ThinkUpdated` acknowledgement. Keep the typed `ThinkUpdated` model and dispatcher support covered by contract fixtures; treat the missing live acknowledgement as a service behavior gap rather than changing the client contract.

## Phase 2B: Voice Agent Catalog and Custom Think Frames

### Goal

Complete the Agent scope intentionally excluded from Phase 2A without expanding legacy interfaces.

### Work

1. Add a narrow Agent settings client for `GET /v1/agent/settings/think/models` with a dedicated factory method.
2. Add typed request and response models for `__customToThinkProvider` and `__customFromThinkProvider` only after reconfirming their current AsyncAPI contract and live behavior.
3. Add a runnable example or focused live smoke for the catalog and custom Think-provider frames that uses a disposable configuration.

### Acceptance Criteria

- The catalog client has route, authentication, and response-deserialization tests.
- Custom Think frames have exact wire tests and continue to fall back to `UnhandledResponse` when no typed subscriber is registered.
- Live validation is opt-in and reports unsupported service behavior without weakening the typed contract.

## Phase 3: Management Reporting and Billing

### Goal

Add the current reporting endpoints needed for cost reconciliation and modern usage analytics.

### Work

1. Add typed options, responses, and client methods for `GET /v1/projects/{project_id}/usage/breakdown`.
2. Add typed options, responses, and client methods for `GET /v1/projects/{project_id}/billing/breakdown`.
3. Add typed options, responses, and client methods for `GET /v1/projects/{project_id}/billing/fields`.
4. Add typed responses and a client method for `GET /v1/projects/{project_id}/purchases`.
5. Add missing filters to existing management operations: `GetProject` gets `limit` and `page`; `GetKeys` gets `status`; and `GetUsageRequests` gets `accessor`, `request_id`, `deployment`, `endpoint`, and `method`.
6. Document the deprecated `GetUsageSummary` as legacy and direct new consumers to usage breakdown.

### API Design

Use focused option objects for each endpoint rather than extending `UsageSummarySchema`. The new endpoints have different filter and grouping semantics, and reusing the legacy schema would preserve existing naming and type mistakes.

### Sequencing

Expose reporting through a new narrow `IManagementReportingClient` and `ClientFactory.CreateManagementReportingClient()` rather than adding members to `IManageClient`. The new client can share the existing authenticated REST abstraction while preserving every existing interface and factory return type.

### Acceptance Criteria

- Each endpoint has a route and query-string construction test.
- Fixtures cover usage totals, agent hours, input/output tokens, TTS characters, billing dollars, grouping, line items, and purchase orders.
- Each result model uses numeric and date/time types that match the OpenAPI schema.
- The management guide contains one usage-breakdown and one billing-breakdown example.

## Phase 4: Request and Response Model Completion

### Goal

Close documented typed-model gaps on otherwise supported endpoints.

### Work

1. Aura REST TTS: add `Speed`, `Tag`, and `MipOptOut`; retain the existing public string types for numeric output options unless a separately approved major-version plan authorizes a correction.
2. Pre-recorded STT: add `MipOptOut`.
3. Text Intelligence: add `Tag`.
4. Listen responses: add `Metadata.DiarizeInfo` with `ModelUuid` and `Arch`, and add `Metadata.Tags` if the current response contract confirms it.
5. Management responses: add project `MipOptOut`; add usage request `Code` and `Deployment`; and add a documented purchase-order property alongside the existing balance `Purchase` property after verifying the response payload.

### Acceptance Criteria

- Every field has a recorded deserialization fixture or a serialization assertion.
- No response field is added based solely on another SDK; its shape must be present in the OpenAPI specification or a captured service payload.
- Public documentation is updated only after the corresponding tests pass.

## Phase 5: Resiliency and Escape Hatches

### Goal

Improve .NET integration ergonomics where peer SDKs have capabilities beyond basic API parity.

### Work

1. Evaluate an opt-in WebSocket reconnect policy with bounded attempts, backoff, and an application-controlled reconnect predicate.
2. Evaluate a public WebSocket transport abstraction for proxies, tests, and alternate hosts.
3. Evaluate an authenticated, public low-level REST escape hatch for newly released endpoints not yet modeled by the SDK.
4. Evaluate Flux TTS text helpers for pause, IPA, and pronunciation controls. Prefer small composable helpers over an opinionated markup builder.

### Guardrails

- Reconnection must never replay queued audio or control frames unless its semantics are explicitly defined and tested.
- A low-level REST escape hatch must use the SDK's authentication, error conversion, timeout, and redacted logging paths.
- These are product decisions, not parity blockers; do not delay Phases 0 through 4 for them.

## Test Plan

Run focused tests while implementing each phase, then verify the repository baseline:

```bash
dotnet test Deepgram.Tests/Deepgram.Tests.csproj
dotnet test Deepgram.sln
dotnet build Deepgram.sln --configuration Release --no-restore
```

Run a live test only for endpoints where a deterministic unit test cannot prove server acceptance. Live tests must skip when `DEEPGRAM_API_KEY` is unavailable and must use a disposable project for Agent management and self-hosted credential operations.

## Proposed Release Plan

| Release | Scope |
| --- | --- |
| Current PR stack | Merge Phase 0 (#445), then Phase 1 (#446), then Phase 2A (#447). |
| Next 7.x feature slice | Phase 2B Agent catalog and custom Think-provider frames, then Phase 3 reporting through a new narrow client and factory. |
| Follow-up releases | Phase 4 additive model completion and Phase 5 resiliency/escape-hatch work. |
| Future major, only if explicitly approved | Any public-interface expansion or callback/numeric-option type correction deferred by this roadmap. |

## Out of Scope

- Adding features to deprecated clients.
- Preserving undocumented route fallbacks without verified production need.
- Treating peer SDK naming or synchronous/asynchronous style differences as missing API functionality.
- Broad formatting or architecture rewrites unrelated to the affected client surfaces.
