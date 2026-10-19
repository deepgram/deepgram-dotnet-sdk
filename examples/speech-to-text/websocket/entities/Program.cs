// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Listen.v2.WebSocket;

namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // Initialize Library with default logging
                // Normal logging is "Info" level
                Library.Initialize();

                // use the client factory with a API Key set with the "DEEPGRAM_API_KEY" environment variable
                var liveClient = ClientFactory.CreateListenWebSocketClient();

                // Entities only arrive on final results, so this handler ignores interim ones.
                await liveClient.Subscribe(new EventHandler<ResultResponse>((sender, e) =>
                {
                    var transcript = e.Channel?.Alternatives?[0].Transcript;
                    if (string.IsNullOrEmpty(transcript))
                    {
                        return;
                    }

                    Console.WriteLine($"Transcript: {transcript}");

                    if (e.Entities is null)
                    {
                        // detect_entities was not enabled, or this is an interim result.
                        return;
                    }

                    foreach (var entity in e.Entities)
                    {
                        // RawValue is the original spoken form, present when formatting is enabled.
                        var spoken = entity.RawValue is null ? "" : $" (spoken: \"{entity.RawValue}\")";
                        Console.WriteLine(
                            $"  entity {entity.Label}: {entity.Value}{spoken} " +
                            $"confidence={entity.Confidence} words=[{entity.StartWord}, {entity.EndWord})");
                    }
                }));

                // Print every server Error so a rejected parameter is visible.
                await liveClient.Subscribe(new EventHandler<ErrorResponse>((sender, e) =>
                {
                    Console.Error.WriteLine($"{e.Variant} {e.Code}: {e.Description}");
                }));

                var liveSchema = new LiveSchema()
                {
                    Model = "nova-3",
                    // Entity Detection extracts names, phone numbers, organizations and more from
                    // the transcript. Enabling it also turns Punctuation on by default.
                    DetectEntities = true,
                    // DiarizeModel selects the diarization model and enables diarization on its own,
                    // so there is no need to also set Diarize. Streaming accepts "v1" and "latest".
                    DiarizeModel = "latest",
                    InterimResults = true,
                };

                bool bConnected = await liveClient.Connect(liveSchema);
                if (!bConnected)
                {
                    Console.WriteLine("Failed to connect to the server");
                    return;
                }

                // Send some audio data
                var audioData = File.ReadAllBytes(@"preamble.wav");
                liveClient.Send(audioData);

                // Wait for a while to receive responses
                await Task.Delay(45000);

                // Stop the connection
                await liveClient.Stop();

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
