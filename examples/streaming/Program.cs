/*
using System.Net.WebSockets;
using Deepgram.Constants;
using Deepgram.DeepgramEventArgs;
using Deepgram.Models;

const string DEEPGRAM_API_KEY = "";

using (var deepgramLive = new LiveClient(DEEPGRAM_API_KEY))
{
    deepgramLive.ConnectionOpened += HandleConnectionOpened;
    deepgramLive.ConnectionClosed += HandleConnectionClosed;
    deepgramLive.LiveError += HandleConnectionError;
    deepgramLive.TranscriptReceived += HandleTranscriptReceived;

    // Connection opened so start sending audio.
    async void HandleConnectionOpened(object? sender, ConnectionOpenEventArgs e)
    {
        byte[] buffer;

        using (FileStream fs = File.OpenRead(@"\preamble.wav"))
        {
            buffer = new byte[fs.Length];
            fs.Read(buffer, 0, (int)fs.Length);
        }

        var chunks = buffer.Chunk(1000);

        foreach (var chunk in chunks)
        {
            deepgramLive.Send(chunk);
            await Task.Delay(50);
        }
    }

    void HandleTranscriptReceived(object? sender, TranscriptReceivedEventArgs e)
    {
        if (e.Transcript.IsFinal && e.Transcript.Channel.Alternatives.First().Transcript.Length > 0)
        {
            var transcript = e.Transcript;
            Console.WriteLine($"[Speaker: {transcript.Channel.Alternatives.First().Words.First()}] {transcript.Channel.Alternatives.First().Transcript}");
        }
    }

    void HandleConnectionClosed(object? sender, ConnectionClosedEventArgs e)
    {
        Console.Write("Connection Closed");
    }

    void HandleConnectionError(object? sender, LiveErrorEventArgs e)
    {
        Console.WriteLine(e.Exception.Message);
    }

    var options = new LiveSchema() { Punctuate = true, Diarize = true, Encoding = AudioEncoding.Linear16 };
    await deepgramLive.Connect(options);

    while (deepgramLive.State() == WebSocketState.Open) { }
}
*/


using Deepgram;
using Deepgram.Constants;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Live.v1;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Initialize the LiveClient with your API key and options
            var apiKey = "0b0c71c7f752646e8499ac72a0d72ae3267ca8fa";
            var options = new DeepgramClientOptions();
            var liveClient = new LiveClient(apiKey, options);

            // Subscribe to the EventResponseReceived event
            liveClient.EventResponseReceived += (sender, e) =>
            {
                if (e.Response.Transcription != null)
                {
                    Console.WriteLine("Transcription received: " + JsonSerializer.Serialize(e.Response.Transcription));
                }
                else if (e.Response.SpeechStarted != null)
                {
                    Console.WriteLine("SpeechStarted received: " + JsonSerializer.Serialize(e.Response.SpeechStarted));
                }
                else if (e.Response.UtteranceEnd != null)
                {
                    Console.WriteLine("UtteranceEnd received: " + JsonSerializer.Serialize(e.Response.UtteranceEnd));
                }
                else if (e.Response.MetaData != null)
                {
                    Console.WriteLine("Metadata received: " + JsonSerializer.Serialize(e.Response.MetaData));
                }
                else if (e.Response.Error != null)
                {
                    Console.WriteLine("Error: " + JsonSerializer.Serialize(e.Response.Error.Message));
                }
            };

            // Start the connection
            var liveSchema = new LiveSchema()
            {
                Model = "nova-2",
            };
            await liveClient.Connect(liveSchema);

            // Send some audio data
            var audioData = File.ReadAllBytes(@"preamble.wav");
            liveClient.Send(audioData);

            // Wait for a while to receive responses
            await Task.Delay(10000);

            // Stop the connection
            await liveClient.Stop();

            // Dispose the client
            liveClient.Dispose();
        }
    }
}
