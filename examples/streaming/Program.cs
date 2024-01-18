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
            deepgramLive.SendData(chunk);
            await Task.Delay(50);
        }

        await deepgramLive.FinishAsync();
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
    await deepgramLive.StartConnectionAsync(options);

    while (deepgramLive.State() == WebSocketState.Open) { }
}