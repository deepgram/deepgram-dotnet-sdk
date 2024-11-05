// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Speak.v1.WebSocket;
using SpeakV1 = Deepgram.Clients.Speak.v1.WebSocket;
using Deepgram.Logger;


namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Initialize Library with default logging
            // Normal logging is "Info" level
            //Library.Initialize();
            // OR very chatty logging
            Library.Initialize(LogLevel.Verbose); // LogLevel.Default, LogLevel.Debug, LogLevel.Verbose

            //// use the client factory with a API Key set with the "DEEPGRAM_API_KEY" environment variable
            //DeepgramWsClientOptions options = new DeepgramWsClientOptions();
            //options.AutoFlushSpeakDelta = 1000;
            //var speakClient = ClientFactory.CreateSpeakWebSocketClient("", options);
            var speakClient = ClientFactory.CreateSpeakWebSocketClient(1) as SpeakV1.Client;
            if (speakClient == null)
            {
                Console.WriteLine("Failed to create SpeakWebSocketClient");
                return;
            }

            // append wav header only once
            bool appendWavHeader = true;

            // Subscribe to the EventResponseReceived event
            speakClient.Subscribe(new EventHandler<OpenResponse>((sender, e) =>
            {
                Console.WriteLine($"\n\n----> {e.Type} received");
            }));
            speakClient.Subscribe(new EventHandler<MetadataResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");
                Console.WriteLine($"----> RequestId: {e.RequestId}");
            }));
            speakClient.Subscribe(new EventHandler<AudioResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");

                // add a wav header
                if (appendWavHeader)
                {
                    using (BinaryWriter writer = new BinaryWriter(File.Open("output.wav", FileMode.Append)))
                    {
                        Console.WriteLine("Adding WAV header to output.wav");
                        byte[] wavHeader = new byte[44];
                        int sampleRate = 48000;
                        short bitsPerSample = 16;
                        short channels = 1;
                        int byteRate = sampleRate * channels * (bitsPerSample / 8);
                        short blockAlign = (short)(channels * (bitsPerSample / 8));

                        wavHeader[0] = 0x52; // R
                        wavHeader[1] = 0x49; // I
                        wavHeader[2] = 0x46; // F
                        wavHeader[3] = 0x46; // F
                        wavHeader[4] = 0x00; // Placeholder for file size (will be updated later)
                        wavHeader[5] = 0x00; // Placeholder for file size (will be updated later)
                        wavHeader[6] = 0x00; // Placeholder for file size (will be updated later)
                        wavHeader[7] = 0x00; // Placeholder for file size (will be updated later)
                        wavHeader[8] = 0x57; // W
                        wavHeader[9] = 0x41; // A
                        wavHeader[10] = 0x56; // V
                        wavHeader[11] = 0x45; // E
                        wavHeader[12] = 0x66; // f
                        wavHeader[13] = 0x6D; // m
                        wavHeader[14] = 0x74; // t
                        wavHeader[15] = 0x20; // Space
                        wavHeader[16] = 0x10; // Subchunk1Size (16 for PCM)
                        wavHeader[17] = 0x00; // Subchunk1Size
                        wavHeader[18] = 0x00; // Subchunk1Size
                        wavHeader[19] = 0x00; // Subchunk1Size
                        wavHeader[20] = 0x01; // AudioFormat (1 for PCM)
                        wavHeader[21] = 0x00; // AudioFormat
                        wavHeader[22] = (byte)channels; // NumChannels
                        wavHeader[23] = 0x00; // NumChannels
                        wavHeader[24] = (byte)(sampleRate & 0xFF); // SampleRate
                        wavHeader[25] = (byte)((sampleRate >> 8) & 0xFF); // SampleRate
                        wavHeader[26] = (byte)((sampleRate >> 16) & 0xFF); // SampleRate
                        wavHeader[27] = (byte)((sampleRate >> 24) & 0xFF); // SampleRate
                        wavHeader[28] = (byte)(byteRate & 0xFF); // ByteRate
                        wavHeader[29] = (byte)((byteRate >> 8) & 0xFF); // ByteRate
                        wavHeader[30] = (byte)((byteRate >> 16) & 0xFF); // ByteRate
                        wavHeader[31] = (byte)((byteRate >> 24) & 0xFF); // ByteRate
                        wavHeader[32] = (byte)blockAlign; // BlockAlign
                        wavHeader[33] = 0x00; // BlockAlign
                        wavHeader[34] = (byte)bitsPerSample; // BitsPerSample
                        wavHeader[35] = 0x00; // BitsPerSample
                        wavHeader[36] = 0x64; // d
                        wavHeader[37] = 0x61; // a
                        wavHeader[38] = 0x74; // t
                        wavHeader[39] = 0x61; // a
                        wavHeader[40] = 0x00; // Placeholder for data chunk size (will be updated later)
                        wavHeader[41] = 0x00; // Placeholder for data chunk size (will be updated later)
                        wavHeader[42] = 0x00; // Placeholder for data chunk size (will be updated later)
                        wavHeader[43] = 0x00; // Placeholder for data chunk size (will be updated later)

                        writer.Write(wavHeader);
                        appendWavHeader = false;
                    }
                }

                if (e.Stream != null)
                {
                    using (BinaryWriter writer = new BinaryWriter(File.Open("output.wav", FileMode.Append)))
                    {
                        writer.Write(e.Stream.ToArray());
                    }
                }
            }));
            speakClient.Subscribe(new EventHandler<FlushedResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");
            }));
            speakClient.Subscribe(new EventHandler<ClearedResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");
            }));
            speakClient.Subscribe(new EventHandler<CloseResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");
            }));
            speakClient.Subscribe(new EventHandler<UnhandledResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");
            }));
            speakClient.Subscribe(new EventHandler<WarningResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");
            }));
            speakClient.Subscribe(new EventHandler<ErrorResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received. Error: {e.Message}");
            }));

            // Start the connection
            var speakSchema = new SpeakSchema()
            {
                Encoding = "linear16",
                SampleRate = 48000,
            };
            await speakClient.Connect(speakSchema);

            // Send some Text to convert to audio
            speakClient.SpeakWithText("Hello World!");

            //Flush the audio
            speakClient.Flush();

            // Wait for the user to press a key
            Console.WriteLine("\n\nPress any key to stop and exit...\n\n\n");
            Console.ReadKey();

            // Stop the connection
            await speakClient.Stop();

            // Terminate Libraries
            Library.Terminate();
        }
    }
}
