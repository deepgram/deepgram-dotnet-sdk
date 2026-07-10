// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Flux.v2.WebSocket;

namespace SampleApp
{
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

                // Subscribe to the Connected event
                await fluxClient.Subscribe(new EventHandler<ConnectedResponse>((sender, e) =>
                {
                    Console.WriteLine($"Connected. Request ID: {e.RequestId}");
                }));

                // Subscribe to TurnInfo events. The turn lifecycle is carried in e.Event/e.EventType.
                // NOTE: keep handlers fast — they are invoked from the receive loop, and a slow
                // handler delays delivery of the next message (including EndOfTurn).
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
                        case TurnEvent.EagerEndOfTurn:
                            // Only sent when EagerEotThreshold is set: a good moment to start
                            // preparing an agent reply before the turn is final.
                            Console.WriteLine($"[Turn {e.TurnIndex}] (eager, confidence {e.EndOfTurnConfidence:F2}) {e.Transcript}");
                            break;
                        case TurnEvent.TurnResumed:
                            // The speaker kept talking after an EagerEndOfTurn: abandon the draft reply.
                            Console.WriteLine($"[Turn {e.TurnIndex}] resumed");
                            break;
                        case TurnEvent.EndOfTurn:
                            Console.WriteLine($"[Turn {e.TurnIndex}] FINAL: {e.Transcript}");
                            foreach (var word in e.Words ?? new List<Word>())
                            {
                                Console.WriteLine($"    {word.HeardWord} ({word.Start}s - {word.End}s, confidence {word.Confidence:F2})");
                            }
                            break;
                    }
                }));

                // Subscribe to the Error event. Flux fatal errors close the connection.
                await fluxClient.Subscribe(new EventHandler<ErrorResponse>((sender, e) =>
                {
                    Console.WriteLine($"Error: {e.Code} - {e.Description}");
                }));

                // Subscribe to the Close event
                await fluxClient.Subscribe(new EventHandler<CloseResponse>((sender, e) =>
                {
                    Console.WriteLine("Connection closed");
                }));

                // Start the connection. Model is required; eager end-of-turn events are enabled
                // by setting EagerEotThreshold.
                var fluxSchema = new FluxSchema()
                {
                    Model = "flux-general-en",
                    Encoding = "linear16",
                    SampleRate = 16000,
                    EotThreshold = 0.7,
                    EagerEotThreshold = 0.5,
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

                // Clean shutdown: this sends {"type":"CloseStream"} and waits briefly so the
                // server can flush the final turn results before the socket is torn down.
                // (Flux has no KeepAlive or Finalize messages - CloseStream is the only
                // type-only control message it accepts.)
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
