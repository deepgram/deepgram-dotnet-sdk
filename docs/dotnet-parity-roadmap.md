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

## Phase 2: Voice Agent Protocol Parity

### Goal

Bring Voice Agent control messages and events to current protocol coverage, with particular focus on safe function execution and Flux-backed turn control.

### Work

1. Add `DeferUntilEot` to the Agent `Function` model.
2. Add typed request models and extension methods for `UpdateListen`, `UpdateThink`, `UpdateSpeak`, `UpdatePrompt`, `InjectAgentMessage`, `FunctionCallResponse`, and `ForceEndTurn`.
3. Add new current-protocol request models rather than changing dormant schemas. The new InjectAgentMessage model serializes `message`, plus optional `behavior` (`default`, `queue`, or `interrupt`); the new FunctionCallResponse model serializes `id`, `name`, and `content`.
4. Add typed inbound models, dispatch entries, and subscriptions for `FunctionCallCancelled`, `ListenUpdated`, `ThinkUpdated`, `LatencyReport`, `Warning`, and `History`.
5. Add the Agent REST client for `GET /v1/agent/settings/think/models`.
6. Add a Voice Agent example that demonstrates deferred client-side functions and cancellation handling.

### Sequencing

Implement models and dispatcher support, then add extension methods over `IAgentWebSocketClient`. The extensions should serialize the typed control message and use the existing immediate-send path, so clients created through `ClientFactory.CreateAgentWebSocketClient()` retain a typed, discoverable surface without modifying the interface. Concrete-client convenience methods may forward to the extensions.

### Acceptance Criteria

- Serialized Settings tests prove `defer_until_eot` is emitted only when specified.
- One wire test exists for each outbound control message and covers required properties.
- Recorded fixtures exercise every new inbound event, including a cancellation event containing multiple function IDs.
- Dispatcher tests prove all known frames select their typed handler and unsupported frames continue to reach `UnhandledResponse`.
- A cancellation test documents the required application behavior: never send a function response for a cancelled call ID.

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
| Next 7.x minor release (`feat:`) | Phase 0 route and request-shape corrections with no public type changes, plus Phase 1 Flux STT options. |
| Next 7.x feature release | Phase 2 Voice Agent extension methods and models, plus Phase 3 reporting through a new narrow client and factory. |
| Follow-up releases | Phase 4 additive model completion and Phase 5 resiliency/escape-hatch work. |
| Future major, only if explicitly approved | Any public-interface expansion or callback/numeric-option type correction deferred by this roadmap. |

## Out of Scope

- Adding features to deprecated clients.
- Preserving undocumented route fallbacks without verified production need.
- Treating peer SDK naming or synchronous/asynchronous style differences as missing API functionality.
- Broad formatting or architecture rewrites unrelated to the affected client surfaces.
