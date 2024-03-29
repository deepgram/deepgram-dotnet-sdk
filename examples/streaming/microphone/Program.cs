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
            liveClient._resultsReceived += (sender, e) =>
            {
                if (e.Channel.Alternatives[0].Transcript == "")
                {
                    Console.WriteLine("Empty transcription received.");
                    return;
                }

                // Console.WriteLine("Transcription received: " + JsonSerializer.Serialize(e.Response.Transcription));
                Console.WriteLine($"Speaker: {e.Channel.Alternatives[0].Transcript}");
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

            // Wait for the user to press a key
            Console.WriteLine("Press any key to stop the microphone streaming...");
            Console.ReadKey();

            // Stop the connection
            await liveClient.Stop();

            // Dispose the client
            liveClient.Dispose();

            // Terminate PortAudio
            Library.Terminate();
        }
    }
}
