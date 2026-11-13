# API-spec parity

This SDK is hand-written. It does not have a Fern generator target, so generated
SDK changes do not automatically reach C#. Use this document to keep the public
.NET surface aligned with the Deepgram API definitions.

## Baseline

- **API definitions:** `deepgram/deepgram-docs`
- **Reviewed spec snapshot:** `229defd4c32f220af07902c24efca5e94b4c47a4`
  (`2026-11-10`)
- **.NET SDK baseline:** `7.5.1`
- **Generated SDKs reviewed against the same snapshot:** Java `0.11.5`,
  JavaScript `5.20.0`, and Python `7.19.0`

`deepgram-docs` can receive content-only changes after this commit. Before
treating an update as an SDK change, inspect the diff under
`api/specs/openapi/` and `api/specs/asyncapi/`, including both `overrides.yml`
files.

## Current parity matrix

| API surface | SDK model or client | Status | Required regression coverage |
| --- | --- | --- | --- |
| `/v1/listen` REST `language_hint` | `PreRecordedSchema` | Pending | Repeated query values preserve order and serialize as `language_hint=en&language_hint=es`. |
| `/v1/listen` REST `redact_usage` | `PreRecordedSchema` | Pending | The optional boolean serializes lowercase and is omitted when unset. |
| `/v1/listen` WebSocket `language_hint` | `Models/Listen/v2/WebSocket/LiveSchema` | Pending | The connection URL repeats language hints. |
| `/v1/listen` WebSocket `redact_usage` | `Models/Listen/v2/WebSocket/LiveSchema` | Pending | The connection URL emits a lowercase boolean. |
| Flux STT connection `redact_usage` | `FluxSchema` | Pending | The `/v2/listen` URL emits a lowercase boolean. |
| Flux STT Configure `redact_usage` | `ConfigureSchema` | Pending | The outgoing `Configure` frame has a boolean field; `ConfigureSuccess` already deserializes it. |
| Flux TTS REST `normalize_volume` | `Models/Flux/Speak/REST/SpeakSchema` | Pending | The `/v2/speak` URL emits a lowercase boolean. |
| Flux TTS WebSocket `normalize_volume` | `Models/Flux/Speak/WebSocket/SpeakSchema` | Pending | The `/v2/speak` WebSocket URL emits a lowercase boolean. |
| Voice Agent Think Live settings | `Agent`, typed provider models | Pending | Settings serialization preserves the complete provider payload. |
| Voice Agent `UpdateThink_Live` | protocol schema, client, and interface | Pending | The exact control frame is sent and `ThinkLiveUpdated` dispatches to a typed subscriber. |
| Voice Agent `FrustrationDetected` | protocol response, client, and interface | Pending | The typed payload dispatches; unknown frames remain non-fatal. |
| Think Live function fillers | `Function` | Pending | All three filler options serialize with their documented wire names. |
| Voice Agent `conversational_behavior` | `ConversationTextResponse` | Pending | The value deserializes and missing legacy/self-hosted values safely default to `default`. |

The expanded Flux TTS voice roster is not listed as a gap. The .NET SDK accepts
model names as strings, so newly listed server models work without a generated
enum update.

## Implementation sequence

1. **Speech-to-text controls:** add Nova `language_hint` and `redact_usage`,
   then Flux STT connection and Configure `redact_usage`.
2. **Flux TTS volume normalization:** add `normalize_volume` to both batch and
   streaming schemas.
3. **Voice Agent Think Live:** add settings, control and event types, function
   filler options, and conversational behavior.

Each implementation PR must update this matrix from `Pending` to `Supported`
and link its focused unit tests. Use additive public models and omit unset
properties with `JsonIgnoreCondition.WhenWritingNull`.

## Review workflow

For an API-spec change:

1. Inspect the OpenAPI or AsyncAPI change and the matching override.
2. Compare the generator output in Java, JavaScript, and Python when it is
   available. Treat it as a shape and wire-behavior oracle, not as .NET code to
   copy mechanically.
3. Map the change to the current .NET REST schema, WebSocket connection schema,
   control model, response model, and public interface.
4. Add exact query-string or frame assertions, plus response dispatch coverage
   for new server events.
5. Update this matrix, examples, README sections, and the applicable
   code-documentation page.

Do not add CI that fetches a mutable remote specification. Keep this reviewed
local baseline and update its snapshot in the same change that implements new
API surface.
