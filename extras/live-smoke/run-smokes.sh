#!/usr/bin/env bash
# Live-API smokes for PR #426. Two required runs, both must exit non-zero on failure:
#
#   1. Flux force-end-turn: passes only if EndOfTurn arrives with trigger "manual" and the
#      full final-turn transcript is present (this also exercises the SendForceEndTurn flush
#      fix over a real socket).
#   2. Agent management CRUD round-trip: create config -> create variable (bare DG_ token) ->
#      list -> get -> update -> delete for both resources, with cleanup guaranteed in finally.
#
# DEEPGRAM_API_KEY must be set in the host environment; it is passed into the containers at
# run time and never baked into the image.
set -uo pipefail
cd "$(dirname "$0")"

if [ -z "${DEEPGRAM_API_KEY:-}" ]; then
    echo "FATAL: DEEPGRAM_API_KEY is not set. The live smokes require a valid production key." >&2
    exit 1
fi

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
# The configurations smoke performs: create variable (bare DG_GREETING token) -> create
# config referencing it -> list -> get (uninterpolated) -> update metadata -> re-fetch ->
# delete config -> delete variable, all with cleanup in finally.
if run_smoke agent-configurations-smoke agent-configurations-smoke.log; then
    for needle in \
        "Created agent variable DG_GREETING:" \
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
    # The fetched, uninterpolated config must still carry the bare DG_GREETING token.
    if ! grep -q "DG_GREETING" agent-configurations-smoke.log; then
        echo "FAIL [agent-config]: bare DG_GREETING token not visible in the fetched config." >&2
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
        "Created agent variable:" \
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
