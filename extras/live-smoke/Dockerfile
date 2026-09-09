# Live-API smoke harness for PR #426: compiles the SDK from source together with the three
# runnable examples it ships, so the containers exercise exactly the code under review against
# api.deepgram.com.
#
# SECURITY: the Deepgram API key is NEVER baked into this image. It is injected at run time
# via the DEEPGRAM_API_KEY environment variable (see docker-compose.yml / run-smokes.sh).
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Deepgram/ Deepgram/
COPY examples/speech-to-text/websocket/flux-force-end-turn/ examples/speech-to-text/websocket/flux-force-end-turn/
COPY examples/agent/manage/ examples/agent/manage/

RUN dotnet publish examples/speech-to-text/websocket/flux-force-end-turn -c Release -f net8.0 -o /app/flux-force-end-turn \
 && dotnet publish examples/agent/manage/configurations -c Release -f net8.0 -o /app/agent-configurations \
 && dotnet publish examples/agent/manage/variables -c Release -f net8.0 -o /app/agent-variables

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/ /app/
