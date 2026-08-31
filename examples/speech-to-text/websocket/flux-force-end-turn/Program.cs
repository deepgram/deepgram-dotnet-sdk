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
    /// https://developers.deepgram.com/docs/speech-to-text/flux/guides/own-turn-detection
    /// </summary>
    class Program
    {
        // Flux works best with ~80ms audio chunks: 16000 Hz * 2 bytes * 0.080s = 2560 bytes.
        const int ChunkBytes = 2560;
        const int WavHeaderBytes = 44;

        static async Task Main(string[] args)
        {
            try
            {
                // Initialize Library with default logging
                // Normal logging is "Info" level
                Library.Initialize();

                // use the client factory with a API Key set with the "DEEPGRAM_API_KEY" environment variable
                var fluxClient = ClientFactory.CreateFluxWebSocketClient();

                // Signals that the manually-ended turn's EndOfTurn has arrived.
                var endOfTurnReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

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
                            endOfTurnReceived.TrySetResult(true);
                            break;
                    }
                }));

                // Subscribe to the Error event. Flux fatal errors close the connection.
                await fluxClient.Subscribe(new EventHandler<ErrorResponse>((sender, e) =>
                {
                    Console.WriteLine($"Error: {e.Code} - {e.Description}");
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
                bool bConnected = await fluxClient.Connect(fluxSchema);
                if (!bConnected)
                {
                    Console.WriteLine("Failed to connect to the server");
                    return;
                }

                // Stream the raw PCM (skipping the 44-byte WAV header) in ~80ms chunks,
                // paced like a live microphone.
                var audioData = File.ReadAllBytes(@"preamble-16k.wav");
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
                // Here the end of the audio file plays that role.
                Console.WriteLine("\nExternal detector fired - sending ForceEndTurn...");
                await fluxClient.SendForceEndTurn();

                // Wait for the manual EndOfTurn (trigger: "manual") to arrive.
                await Task.WhenAny(endOfTurnReceived.Task, Task.Delay(TimeSpan.FromSeconds(10)));

                // Clean shutdown: sends {"type":"CloseStream"} and waits briefly so the server
                // can flush any remaining results before the socket is torn down.
                await fluxClient.Stop();

                // Teardown Library
                Library.Terminate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }
    }
}
