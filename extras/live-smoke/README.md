# Live-API smoke harness (PR #426)

Dockerized end-to-end checks that compile the SDK from source together with three of its
runnable examples and exercise them against `api.deepgram.com`:

| Service | Example | What it proves |
| --- | --- | --- |
| `flux-smoke` | `examples/speech-to-text/websocket/flux-force-end-turn` | `SendForceEndTurn` yields an `EndOfTurn` with `trigger: "manual"` and the full final-turn transcript (queued audio is flushed before the control frame). |
| `agent-configurations-smoke` | `examples/agent/manage/configurations` | Agent configuration CRUD: create variable (bare `DG_*` token) -> create config referencing it -> list -> get (uninterpolated) -> update metadata -> delete both. |
| `agent-variables-smoke` | `examples/agent/manage/variables` | Agent variable CRUD: create -> list -> get -> update (PATCH, empty-body success) -> delete. |

`run-smokes.sh` builds the image, runs the three services in order, asserts on their output and
exits non-zero if any assertion fails. Per-run logs land next to the script (`*.log`, git-ignored).

## Required environment

Both variables are read from the host environment at run time and are never baked into the image.

| Variable | Used by | Purpose |
| --- | --- | --- |
| `DEEPGRAM_API_KEY` | all three services | A valid production API key. |
| `DEEPGRAM_PROJECT_ID` | the two agent services only | The project in which the agent smokes create, update and delete resources. **It must identify a disposable test project.** |

`run-smokes.sh` refuses to start if either variable is missing. There is no fallback to "the first
project on the account", in the script or in the examples it runs.

### Why the project must be disposable

The agent smokes delete everything they create, but the live API currently **reserves a deleted
template variable's name forever within its project** (`This project already has a variable with
that name`). Every run therefore leaves permanent, invisible state behind in whichever project it
targets, even when cleanup succeeds. The examples suffix each variable key with a per-run
timestamp (`DG_GREETING_<yyyyMMddHHmmss>`) so repeated runs do not collide, but those reserved
names still accumulate. Never point `DEEPGRAM_PROJECT_ID` at a production project.

### Preflight

Before the image is built, the script issues
`GET https://api.deepgram.com/v1/projects/$DEEPGRAM_PROJECT_ID` with the key and aborts on any
non-200 response. On success it prints the project's **name** alongside the id so the operator
sees exactly which project is about to be mutated.

The Flux smoke is project-agnostic; the harness deliberately does not pass the project id to it.

## What each run creates and deletes

All resources are created inside `DEEPGRAM_PROJECT_ID` and deleted in a `finally` block, so
cleanup runs even when a later step fails.

- `agent-configurations-smoke`: one template variable `DG_GREETING_<run>` and one agent
  configuration that references it. Deletes the configuration, then the variable.
- `agent-variables-smoke`: one template variable `DG_GREETING_<run>`. Deletes it.
- `flux-smoke`: nothing (a single WebSocket session over the bundled WAV fixture).

After a successful run the project's agent and agent-variable lists are back to what they were
before, minus the reserved names described above.

## Running

```bash
export DEEPGRAM_API_KEY=...          # production key
export DEEPGRAM_PROJECT_ID=...       # DISPOSABLE test project id
./extras/live-smoke/run-smokes.sh
```

Requires Docker with Compose v2. The fixture phrase the Flux assertion looks for can be overridden
with `FLUX_TAIL_PHRASE` if the bundled audio changes.
