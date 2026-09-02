// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Flux.WebSocket;

namespace SampleApp
{
    /// <summary>
    /// Bring Your Own Turn Detection: suppress Flux's native end-of-turn detection with
    /// EotThreshold = 1.0 and end turns yourself with SendForceEndTurn(). The resulting
    /// EndOfTurn carries Trigger = "manual" ("timeout" when the EotTimeoutMs backstop fired).
    /// Success for this example means exactly that: an EndOfTurn with trigger "manual".
    /// Timeout, a fatal error, or any other trigger exits non-zero.
    /// https://developers.deepgram.com/docs/flux/own-turn-detection
    /// </summary>
    class Program
    {
        // Flux works best with ~80ms audio chunks: 16000 Hz * 2 bytes * 0.080s = 2560 bytes.
        const int ChunkBytes = 2560;
        const int WavHeaderBytes = 44;

        static async Task<int> Main(string[] args)
        {
            // Initialize Library with default logging
            // Normal logging is "Info" level
            Library.Initialize();

            // Resolve the audio fixture relative to the compiled output, not the caller's
            // working directory, so `dotnet run --project ...` works from anywhere. Do this
            // before connecting so a missing file never leaves a socket behind.
            var audioPath = Path.Combine(AppContext.BaseDirectory, "preamble-16k.wav");
            if (!File.Exists(audioPath))
            {
                Console.WriteLine($"Audio file not found: {audioPath}");
                Library.Terminate();
                return 1;
            }
            var audioData = File.ReadAllBytes(audioPath);

            // The concrete FluxWebSocketClient (rather than ClientFactory, which returns
            // IFluxWebSocketClient) is used because SendForceEndTurn lives on the concrete
            // client only until the 8.0 interface revision. The API key comes from the
            // "DEEPGRAM_API_KEY" environment variable.
            var fluxClient = new FluxWebSocketClient();
            var connected = false;

            try
            {
                // Completes with the manually-ended turn's TurnInfo, or faults on a fatal error.
                var endOfTurnReceived = new TaskCompletionSource<TurnInfoResponse>(TaskCreationOptions.RunContinuationsAsynchronously);

                // Subscribe to the Connected event
                await fluxClient.Subscribe(new EventHandler<ConnectedResponse>((sender, e) =>
                {
                    Console.WriteLine($"Connected. Request ID: {e.RequestId}");
                }));

                // Subscribe to TurnInfo events. With EotThreshold = 1.0 no EndOfTurn fires on the
                // model's own confidence - only ForceEndTurn ("manual") or the EotTimeoutMs
                // backstop ("timeout") end a turn.
                await fluxClient.Subscribe(new EventHandler<TurnInfoResponse>((sender, e) =>
                {
                    switch (e.EventType)
                    {
                        case TurnEvent.StartOfTurn:
                            Console.WriteLine($"\n[Turn {e.TurnIndex}] started");
                            break;
                        case TurnEvent.Update:
                            Console.WriteLine($"[Turn {e.TurnIndex}] ... {e.Transcript}");
                            break;
                        case TurnEvent.EndOfTurn:
                            // Trigger states why the turn ended: "manual" for ForceEndTurn,
                            // "timeout" for the EotTimeoutMs backstop, "model" for native
                            // detection. It is an open set - handle unknown values gracefully.
                            Console.WriteLine($"[Turn {e.TurnIndex}] FINAL (trigger: {e.Trigger}): {e.Transcript}");
                            endOfTurnReceived.TrySetResult(e);
                            break;
                    }
                }));

                // Subscribe to the Error event. Flux fatal errors close the connection, so a
                // fatal error means the manual EndOfTurn can never arrive - fail fast.
                await fluxClient.Subscribe(new EventHandler<ErrorResponse>((sender, e) =>
                {
                    Console.WriteLine($"Error: {e.Code} - {e.Description}");
                    endOfTurnReceived.TrySetException(
                        new Exception($"Flux fatal error before EndOfTurn: {e.Code} - {e.Description}"));
                }));

                // A ForceEndTurn sent when no turn is active is ignored with a Warning message
                // (code FORCE_END_TURN_NO_ACTIVE_TURN), which surfaces via the Unhandled event.
                await fluxClient.Subscribe(new EventHandler<UnhandledResponse>((sender, e) =>
                {
                    Console.WriteLine($"Unhandled message: {e.Raw}");
                }));

                // Subscribe to the Close event
                await fluxClient.Subscribe(new EventHandler<CloseResponse>((sender, e) =>
                {
                    Console.WriteLine("Connection closed");
                }));

                // Take full ownership of turn endings:
                // - EotThreshold = 1.0 suppresses natural EndOfTurn events entirely.
                // - EagerEotThreshold stays unset so no EagerEndOfTurn events fire.
                // - EotTimeoutMs is a high safety net that only fires if your detection fails.
                var fluxSchema = new FluxSchema()
                {
                    Model = "flux-general-en",
                    Encoding = "linear16",
                    SampleRate = 16000,
                    EotThreshold = 1.0,
                    EotTimeoutMs = 30000,
                };
                connected = await fluxClient.Connect(fluxSchema);
                if (!connected)
                {
                    Console.WriteLine("Failed to connect to the server");
                    return 1;
                }

                // Stream the raw PCM (skipping the 44-byte WAV header) in ~80ms chunks,
                // paced like a live microphone.
                for (var offset = WavHeaderBytes; offset < audioData.Length; offset += ChunkBytes)
                {
                    var length = Math.Min(ChunkBytes, audioData.Length - offset);
                    var chunk = new byte[length];
                    Array.Copy(audioData, offset, chunk, 0, length);
                    fluxClient.Send(chunk);
                    await Task.Delay(80);
                }

                // In a real application this is where your own turn detector fires: a
                // push-to-talk release, a DTMF tone, a VAD silence event, a UI action.
                // Here the end of the audio file plays that role. SendForceEndTurn flushes any
                // audio still queued by Send() before the control message goes out.
                Console.WriteLine("\nExternal detector fired - sending ForceEndTurn...");
                await fluxClient.SendForceEndTurn();

                // Wait for the manual EndOfTurn. Success requires trigger "manual"; a timeout
                // or a fatal error is a failure, not a normal exit.
                var timeout = Task.Delay(TimeSpan.FromSeconds(10));
                var winner = await Task.WhenAny(endOfTurnReceived.Task, timeout);
                if (winner == timeout)
                {
                    Console.WriteLine("FAILED: no EndOfTurn arrived within 10 seconds of ForceEndTurn.");
                    return 1;
                }

                // Faults (from the Error handler) propagate here as an exception.
                var endOfTurn = await endOfTurnReceived.Task;
                if (endOfTurn.Trigger != "manual")
                {
                    Console.WriteLine($"FAILED: expected EndOfTurn trigger \"manual\" but got \"{endOfTurn.Trigger}\".");
                    return 1;
                }

                Console.WriteLine($"\nSUCCESS: manual EndOfTurn received. Transcript: {endOfTurn.Transcript}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return 1;
            }
            finally
            {
                // Clean shutdown on every path: Stop() sends {"type":"CloseStream"} and waits
                // briefly so the server can flush any remaining results before teardown.
                if (connected)
                {
                    try
                    {
                        await fluxClient.Stop();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception during Stop: {ex.Message}");
                    }
                }

                // Teardown Library
                Library.Terminate();
            }
        }
    }
}
