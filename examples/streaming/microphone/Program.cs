using Deepgram.Models.Live.v1;
using Deepgram.Microphone;
using System.Text.Json;

namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Library.Initialize();

            // Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
            var liveClient = new LiveClient();

            // Subscribe to the EventResponseReceived event
            liveClient.EventResponseReceived += (sender, e) =>
            {
                if (e.Response.Transcription != null)
                {
                    if (e.Response.Transcription.Channel.Alternatives[0].Transcript == "")
                    {
                        Console.WriteLine("Empty transcription received.");
                        return;
                    }

                    // Console.WriteLine("Transcription received: " + JsonSerializer.Serialize(e.Response.Transcription));
                    Console.WriteLine($"Speaker: {e.Response.Transcription.Channel.Alternatives[0].Transcript}");
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
                Encoding = "linear16",
                SampleRate = 16000,
                InterimResults = true,
                UtteranceEnd = "1000",
                VadEvents = true,
            };
            await liveClient.Connect(liveSchema);

            // Microphone streaming
            var microphone = new Microphone(liveClient.Send);
            microphone.Start();

            Thread.Sleep(3600000);

            // Stop the connection
            await liveClient.Stop();

            // Dispose the client
            liveClient.Dispose();

            // Terminate PortAudio
            Library.Terminate();
        }
    }
}
