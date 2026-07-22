// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Flux.Speak.REST;

namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Initialize Library with default logging (Info level)
            Library.Initialize();

            // Use the client factory with an API key set via the "DEEPGRAM_API_KEY"
            // environment variable. Flux TTS also accepts an access token.
            var speakClient = ClientFactory.CreateFluxSpeakRESTClient();

            // Defaults to production (api.deepgram.com), which serves /v2/speak. To target a
            // non-prod endpoint (e.g. staging), pass a base-address override:
            //
            //   var options = new DeepgramHttpClientOptions(baseAddress: "https://api.staging.deepgram.com");
            //   var speakClient = ClientFactory.CreateFluxSpeakRESTClient("", options);

            // Synthesize a complete block of text into a single audio file. Model is required and
            // must be a flux-* voice.
            var mp3 = await speakClient.ToFile(
                new TextSource("Your appointment is confirmed for 3pm tomorrow."),
                "output.mp3",
                new SpeakSchema()
                {
                    Model = "flux-alexis-en",
                    Encoding = "mp3",
                    BitRate = 48000,
                });
            Console.WriteLine($"Wrote output.mp3 (request {mp3.RequestId}, {mp3.Characters} chars)");

            // Uncompressed linear16 WAV at 44100 Hz. sample_rate is an integer on the wire.
            var wav = await speakClient.ToFile(
                new TextSource("Your appointment is confirmed for 3pm tomorrow."),
                "output.wav",
                new SpeakSchema()
                {
                    Model = "flux-alexis-en",
                    Encoding = "linear16",
                    Container = "wav",
                    SampleRate = 44100,
                });
            Console.WriteLine($"Wrote output.wav (request {wav.RequestId})");

            // Callback (asynchronous) mode: the request returns an ack with a request_id and the
            // audio is delivered to your callback URL. Uncomment and supply a reachable URL:
            //
            //   var ack = await speakClient.StreamCallBack(
            //       new TextSource("..."), "https://example.com/webhook",
            //       new SpeakSchema() { Model = "flux-alexis-en", Encoding = "mp3" });
            //   Console.WriteLine($"Accepted async request: {ack.RequestId}");

            Library.Terminate();
        }
    }
}
