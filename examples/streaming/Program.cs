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
            var apiKey = "REPLACE-WITH-YOUR-API-KEY";
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
