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

                // To target the staging endpoint (where /v2/speak is available today), construct
                // the client with a base-address override instead:
                //
                //   var options = new DeepgramWsClientOptions(baseAddress: "wss://api.staging.deepgram.com");
                //   var speakClient = ClientFactory.CreateFluxSpeakWebSocketClient("", options);

                // Collect the synthesized audio as it streams in. Raw linear16 PCM at the sample
                // rate requested below; write it to a file you can play back.
                var audioOut = new FileStream("output.raw", FileMode.Create);

                await speakClient.Subscribe(new EventHandler<ConnectedResponse>((sender, e) =>
                {
                    Console.WriteLine($"Connected. Request ID: {e.RequestId} (model {e.ModelName})");
                }));

                await speakClient.Subscribe(new EventHandler<SpeechStartedResponse>((sender, e) =>
                {
                    Console.WriteLine($"Speech started: {e.SpeechId}");
                }));

                // Audio chunks arrive interleaved with the control messages.
                // NOTE: keep handlers fast — they run on the receive loop, and a slow handler
                // delays delivery of the next chunk.
                await speakClient.Subscribe(new EventHandler<AudioResponse>((sender, e) =>
                {
                    if (e.Stream is not null)
                    {
                        e.Stream.CopyTo(audioOut);
                    }
                }));

                await speakClient.Subscribe(new EventHandler<FlushedResponse>((sender, e) =>
                {
                    Console.WriteLine($"Flushed: {e.SpeechId}");
                }));

                await speakClient.Subscribe(new EventHandler<SpeechMetadataResponse>((sender, e) =>
                {
                    Console.WriteLine($"Turn metadata: {e.AudioDurationMs}ms audio, {e.BillableCharacterCount} billable chars");
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

                // Model is required and must be a flux-* voice.
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

                // Give the audio a moment to arrive, then close.
                await Task.Delay(2000);

                // Clean shutdown: sends {"type":"Close"} and waits briefly for the server to drain
                // remaining/queued audio and emit a final SessionMetadata before teardown.
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
