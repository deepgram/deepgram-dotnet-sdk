// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Flux.Speak.WebSocket;

namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // Initialize Library with default logging (Info level)
                Library.Initialize();

                // Use the client factory with an API key set via the "DEEPGRAM_API_KEY"
                // environment variable. Flux TTS also accepts an access token.
                var speakClient = ClientFactory.CreateFluxSpeakWebSocketClient();

                // Defaults to production (api.deepgram.com), which serves /v2/speak. To target a
                // non-prod endpoint (e.g. staging), construct the client with a base-address
                // override instead:
                //
                //   var options = new DeepgramWsClientOptions(baseAddress: "wss://api.staging.deepgram.com");
                //   var speakClient = ClientFactory.CreateFluxSpeakWebSocketClient("", options);

                // Collect the synthesized audio as it streams in. Raw linear16 PCM at the sample
                // rate requested below; write it to a file you can play back.
                var audioOut = new FileStream("output.raw", FileMode.Create);

                // Signals the turn's audio is complete (SpeechMetadata fires after all of a turn's
                // audio has been sent). Wait on this before closing so audio is not truncated.
                var turnComplete = new TaskCompletionSource();
                var secondTurnStarted = new TaskCompletionSource();
                var configureAcked = new TaskCompletionSource();
                var interruptAcked = new TaskCompletionSource();

                // PLAYBACK OFFSET — the one thing to get right about barge-in:
                // this example counts bytes RECEIVED (48 bytes/ms at linear16/24kHz) as a
                // stand-in for playback position, so it stays self-contained. Audio arrives
                // much faster than realtime, so a real client MUST report its audio player's
                // actual position instead — the offset is what makes TextSpoken/TextRemaining
                // reflect what the user really heard. It is cumulative across the session
                // (not per turn), and each interrupt must advance past the previous one.
                const long BytesPerMs = 48;
                long audioBytesReceived = 0;
                var speechStartedCount = 0;

                await speakClient.Subscribe(new EventHandler<ConnectedResponse>((sender, e) =>
                {
                    Console.WriteLine($"Connected. Request ID: {e.RequestId} (model {e.ModelName})");
                }));

                await speakClient.Subscribe(new EventHandler<SpeechStartedResponse>((sender, e) =>
                {
                    Console.WriteLine($"Speech started: {e.SpeechId}");
                    if (Interlocked.Increment(ref speechStartedCount) == 2)
                    {
                        secondTurnStarted.TrySetResult();
                    }
                }));

                // Audio chunks arrive interleaved with the control messages.
                // NOTE: keep handlers fast — they run on the receive loop, and a slow handler
                // delays delivery of the next chunk.
                await speakClient.Subscribe(new EventHandler<AudioResponse>((sender, e) =>
                {
                    if (e.Stream is not null)
                    {
                        e.Stream.CopyTo(audioOut);
                        Interlocked.Add(ref audioBytesReceived, e.Stream.Length);
                    }
                }));

                await speakClient.Subscribe(new EventHandler<FlushedResponse>((sender, e) =>
                {
                    Console.WriteLine($"Flushed: {e.SpeechId}");
                }));

                await speakClient.Subscribe(new EventHandler<SpeechMetadataResponse>((sender, e) =>
                {
                    Console.WriteLine($"Turn metadata: {e.AudioDurationMs}ms audio, {e.BillableCharacterCount} billable chars");
                    turnComplete.TrySetResult();
                }));

                // GA: barge-in. The server cancels the active turn and reports exactly what the
                // user heard, so it can be fed back into your LLM context.
                await speakClient.Subscribe(new EventHandler<SpeechInterruptedResponse>((sender, e) =>
                {
                    Console.WriteLine($"Interrupted after {e.AudioPlayedMs}ms.");
                    Console.WriteLine($"  Heard:  {e.TextSpoken}");
                    Console.WriteLine($"  Unsaid: {e.TextRemaining}");
                    interruptAcked.TrySetResult();
                }));

                // GA: mid-stream speed changes, acknowledged by ConfigureSuccess/ConfigureFailure.
                await speakClient.Subscribe(new EventHandler<ConfigureSuccessResponse>((sender, e) =>
                {
                    Console.WriteLine($"Configure applied: speed={e.Applied?.Speed}");
                    configureAcked.TrySetResult();
                }));
                await speakClient.Subscribe(new EventHandler<ConfigureFailureResponse>((sender, e) =>
                {
                    Console.WriteLine($"Configure rejected: {e.Code} - {e.Description}");
                    configureAcked.TrySetResult();
                }));

                await speakClient.Subscribe(new EventHandler<SessionMetadataResponse>((sender, e) =>
                {
                    Console.WriteLine($"Session totals: {e.TotalAudioDurationMs}ms audio, {e.TotalBillableCharacterCount} billable chars");
                }));

                await speakClient.Subscribe(new EventHandler<WarningResponse>((sender, e) =>
                {
                    Console.WriteLine($"Warning: {e.Code} - {e.Description}");
                }));

                await speakClient.Subscribe(new EventHandler<ErrorResponse>((sender, e) =>
                {
                    Console.WriteLine($"Error: {e.Code} - {e.Description}");
                }));

                await speakClient.Subscribe(new EventHandler<CloseResponse>((sender, e) =>
                {
                    Console.WriteLine("Connection closed");
                }));

                // Model is required and must be a flux-* voice. Speed and Expressivity are
                // optional GA connect params (see the mid-stream Configure below for changing
                // speed without reconnecting).
                var schema = new SpeakSchema()
                {
                    Model = "flux-alexis-en",
                    Encoding = "linear16",
                    SampleRate = 24000,
                };
                bool bConnected = await speakClient.Connect(schema);
                if (!bConnected)
                {
                    Console.WriteLine("Failed to connect to the server");
                    return;
                }

                // Stream text into the active turn — send LLM tokens here as they arrive.
                await speakClient.SendText("Sure, I can help you cancel your subscription.");

                // Flush ends the turn and generates the remaining audio; the server replies with
                // Flushed, then SpeechMetadata once the turn's audio has been sent.
                await speakClient.SendFlush();

                // Wait for the turn's audio to finish arriving before closing, so it isn't
                // truncated. (Closing mid-turn only drains up to the client's close grace period.)
                await Task.WhenAny(turnComplete.Task, Task.Delay(30000));

                // GA: change the speaking rate mid-session, without reconnecting.
                await speakClient.SendConfigure(new ConfigureSchema { Speed = 1.1 });
                await Task.WhenAny(configureAcked.Task, Task.Delay(10000));

                // GA: barge-in. Start a deliberately long second turn, then interrupt it partway
                // through — in a real app, Interrupt fires the moment you detect the user
                // speaking over the agent (e.g. Flux STT's StartOfTurn), not on a fixed delay.
                await speakClient.SendText(
                    "This sentence is deliberately long so that audio is still streaming when the " +
                    "interrupt is sent, which is what makes the barge-in observable.");
                await Task.WhenAny(secondTurnStarted.Task, Task.Delay(10000));
                await Task.Delay(500); // let some of the second turn's audio arrive before cutting it off
                // The offset sent to SendInterrupt is the SESSION total, not this turn's — it must
                // keep growing across turns, or a later interrupt will fail the server's "must
                // advance past the previous interrupt" check. Do not subtract a per-turn baseline.
                var playbackOffsetMs = Interlocked.Read(ref audioBytesReceived) / BytesPerMs;
                await speakClient.SendInterrupt(Math.Max(playbackOffsetMs, 1));
                await Task.WhenAny(interruptAcked.Task, Task.Delay(10000));

                // Clean shutdown: sends {"type":"Close"} and waits briefly for the server to drain
                // any remaining/queued audio and emit a final SessionMetadata before teardown.
                await speakClient.Stop();

                audioOut.Flush();
                audioOut.Dispose();
                Console.WriteLine("Wrote output.raw (raw linear16 PCM @ 24000 Hz)");

                Library.Terminate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }
    }
}
