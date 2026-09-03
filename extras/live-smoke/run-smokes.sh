#!/usr/bin/env bash
# Live-API smokes for PR #426. Three required runs, each must exit non-zero on failure:
#
#   1. Flux force-end-turn: passes only if EndOfTurn arrives with trigger "manual" and the
#      full final-turn transcript is present (this also exercises the SendForceEndTurn flush
#      fix over a real socket).
#   2. Agent configuration CRUD round-trip: create variable (bare DG_ token) -> create config ->
#      list -> get -> update metadata -> delete both, with cleanup guaranteed in finally.
#   3. Agent variable CRUD round-trip: create -> list -> get -> update -> delete, same guarantee.
#
# Both agent runs operate ONLY inside DEEPGRAM_PROJECT_ID and delete only what they created.
#
# Required host environment (passed into the containers at run time, never baked into the image):
#
#   DEEPGRAM_API_KEY      a valid production key.
#   DEEPGRAM_PROJECT_ID   the project the two agent smokes create/update/delete resources in.
#                         It MUST be a disposable test project: the live API reserves a deleted
#                         variable's name forever within its project, so every run leaves
#                         invisible permanent state behind even though cleanup succeeds. The
#                         script refuses to guess a project and never falls back to the account's
#                         first project.
#
# Before building anything the script GETs /v1/projects/$DEEPGRAM_PROJECT_ID with the key and
# aborts on a non-200, printing the project's name so the operator sees exactly what is about to
# be mutated. The Flux smoke is project-agnostic and does not receive the project id.
set -uo pipefail
cd "$(dirname "$0")"

if [ -z "${DEEPGRAM_API_KEY:-}" ]; then
    echo "FATAL: DEEPGRAM_API_KEY is not set. The live smokes require a valid production key." >&2
    exit 1
fi

if [ -z "${DEEPGRAM_PROJECT_ID:-}" ]; then
    cat >&2 <<'MSG'
FATAL: DEEPGRAM_PROJECT_ID is not set.

The agent smokes create, update and delete agent configurations and template variables in the
project you name here, and the live API reserves a deleted variable's name permanently within
that project. Point DEEPGRAM_PROJECT_ID at a DISPOSABLE test project you own - never a production
project. This harness will not pick a project for you.
MSG
    exit 1
fi

echo "==> Preflight: resolving project ${DEEPGRAM_PROJECT_ID}..."
preflight_body=$(mktemp)
preflight_code=$(curl -sS -o "$preflight_body" -w '%{http_code}' \
    -H "Authorization: Token ${DEEPGRAM_API_KEY}" \
    "https://api.deepgram.com/v1/projects/${DEEPGRAM_PROJECT_ID}")
if [ "$preflight_code" != "200" ]; then
    echo "FATAL: GET /v1/projects/${DEEPGRAM_PROJECT_ID} returned HTTP ${preflight_code}; refusing to run destructive smokes." >&2
    echo "       Check that the key can access the project and that the id is correct." >&2
    cat "$preflight_body" >&2; echo >&2
    rm -f "$preflight_body"
    exit 1
fi
project_name=$(sed -n 's/.*"name"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' "$preflight_body" | head -n1)
rm -f "$preflight_body"
echo "==> Agent smokes will create/update/delete resources in project: ${project_name:-<unnamed>} (${DEEPGRAM_PROJECT_ID})"

# A phrase from the tail of the spoken audio (the US Constitution preamble). Its presence in
# the final transcript proves the LAST audio chunks reached Flux before the turn ended — the
# exact truncation the ForceEndTurn flush fix prevents. Override if the fixture audio changes.
FLUX_TAIL_PHRASE="${FLUX_TAIL_PHRASE:-United States}"

fail=0

echo "==> Building smoke image (SDK + examples compiled from source)..."
docker compose build flux-smoke || exit 1

run_smoke() {
    local name="$1"; shift
    local logfile="$1"; shift
    echo ""
    echo "==> Running ${name}..."
    docker compose run --rm "$name" 2>&1 | tee "$logfile"
    local code=${PIPESTATUS[0]}
    echo "==> ${name} exit code: ${code}"
    return "$code"
}

# --- Run 1: Flux force-end-turn ---------------------------------------------------------
if run_smoke flux-smoke flux-smoke.log; then
    if ! grep -q "SUCCESS: manual EndOfTurn received" flux-smoke.log; then
        echo "FAIL [flux]: no manual EndOfTurn success line in output." >&2
        fail=1
    fi
    transcript=$(sed -n 's/.*SUCCESS: manual EndOfTurn received\. Transcript: //p' flux-smoke.log)
    words=$(echo "$transcript" | wc -w | tr -d ' ')
    if [ "${words:-0}" -lt 5 ]; then
        echo "FAIL [flux]: final-turn transcript missing or too short (${words} words): '$transcript'" >&2
        fail=1
    fi
    if ! echo "$transcript" | grep -qi "$FLUX_TAIL_PHRASE"; then
        echo "FAIL [flux]: final transcript does not contain the expected tail phrase '$FLUX_TAIL_PHRASE'" >&2
        echo "             (transcript: '$transcript')" >&2
        fail=1
    fi
else
    echo "FAIL [flux]: flux-smoke exited non-zero." >&2
    fail=1
fi

# --- Run 2: Agent management CRUD round-trip --------------------------------------------
# The configurations smoke performs: create variable (bare DG_GREETING_<run> token; the live
# API reserves deleted variable names forever, so the examples suffix each run) -> create
# config referencing it -> list -> get (uninterpolated) -> update metadata -> re-fetch ->
# delete config -> delete variable, all with cleanup in finally.
if run_smoke agent-configurations-smoke agent-configurations-smoke.log; then
    for needle in \
        "Created agent variable DG_GREETING_" \
        "Created agent configuration:" \
        "agent configuration(s):" \
        "Fetched agent configuration (uninterpolated):" \
        "Updated agent metadata:" \
        "Deleted agent configuration:" \
        "Deleted agent variable:"; do
        if ! grep -q "$needle" agent-configurations-smoke.log; then
            echo "FAIL [agent-config]: expected output '$needle' not found." >&2
            fail=1
        fi
    done
    # The fetched, uninterpolated config must still carry the bare token, unquoted and
    # unbraced, exactly as written: e.g.  \"greeting\": DG_GREETING_<run>
    if ! grep -Eq '\\"greeting\\": DG_GREETING_[0-9]+' agent-configurations-smoke.log; then
        echo "FAIL [agent-config]: bare DG_GREETING_<run> token not visible uninterpolated in the fetched config." >&2
        fail=1
    fi
else
    echo "FAIL [agent-config]: agent-configurations-smoke exited non-zero." >&2
    fail=1
fi

# The variables smoke performs the full variable CRUD: create -> list -> get -> update
# (PATCH, empty-body success) -> re-fetch -> delete, with cleanup in finally.
if run_smoke agent-variables-smoke agent-variables-smoke.log; then
    for needle in \
        "Created agent variable DG_GREETING_" \
        "agent variable(s):" \
        "Fetched agent variable:" \
        "Updated agent variable:" \
        "Deleted agent variable:"; do
        if ! grep -q "$needle" agent-variables-smoke.log; then
            echo "FAIL [agent-vars]: expected output '$needle' not found." >&2
            fail=1
        fi
    done
    if ! grep -q "Welcome back! What can I do for you?" agent-variables-smoke.log; then
        echo "FAIL [agent-vars]: updated variable value not visible after re-fetch." >&2
        fail=1
    fi
else
    echo "FAIL [agent-vars]: agent-variables-smoke exited non-zero." >&2
    fail=1
fi

echo ""
if [ "$fail" -ne 0 ]; then
    echo "LIVE SMOKES FAILED (see logs above: flux-smoke.log, agent-configurations-smoke.log, agent-variables-smoke.log)" >&2
    exit 1
fi
echo "LIVE SMOKES PASSED: manual EndOfTurn with full transcript, and both agent CRUD round-trips clean."
