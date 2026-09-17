// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

// Deepgram Flux over a raw System.Net.WebSockets.ClientWebSocket - no SDK required.
// Demonstrates:
//   - authenticating with the Authorization header on the WebSocket upgrade
//   - streaming linear16 audio in ~80ms chunks
//   - handling the turn lifecycle events (StartOfTurn / Update / EagerEndOfTurn /
//     TurnResumed / EndOfTurn) carried in TurnInfo messages
//   - clean shutdown via the CloseStream control message
//
// Flux accepts exactly three things from the client: binary audio, {"type":"CloseStream"},
// and {"type":"Configure",...}. Do NOT send the v1 KeepAlive/Finalize messages - the v2
// endpoint rejects them with an UNPARSABLE_CLIENT_MESSAGE error.

using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

const int ChunkBytes = 2560; // 80ms of linear16 @ 16000 Hz mono
const int WavHeaderBytes = 44;

var apiKey = Environment.GetEnvironmentVariable("DEEPGRAM_API_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.WriteLine("Set the DEEPGRAM_API_KEY environment variable.");
    return;
}

var uri = new Uri("wss://api.deepgram.com/v2/listen"
    + "?model=flux-general-en"
    + "&encoding=linear16"
    + "&sample_rate=16000"
    + "&eot_threshold=0.7");

using var ws = new ClientWebSocket();
// Authentication rides on the upgrade request. "Token <API_KEY>" for API keys,
// or "Bearer <ACCESS_TOKEN>" for short-lived access tokens.
ws.Options.SetRequestHeader("Authorization", $"Token {apiKey}");

await ws.ConnectAsync(uri, CancellationToken.None);
Console.WriteLine("WebSocket connected");

// Receiver: parse each text frame and switch on its "type" (and "event" for TurnInfo).
var receiver = Task.Run(async () =>
{
    var buffer = new byte[16 * 1024];
    var message = new MemoryStream();
    while (ws.State == WebSocketState.Open || ws.State == WebSocketState.CloseSent)
    {
        WebSocketReceiveResult result;
        message.SetLength(0);
        do
        {
            result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                Console.WriteLine($"Server closed the connection ({result.CloseStatus})");
                return;
            }
            message.Write(buffer, 0, result.Count);
        } while (!result.EndOfMessage);

        using var doc = JsonDocument.Parse(Encoding.UTF8.GetString(message.ToArray()));
        var root = doc.RootElement;
        var type = root.GetProperty("type").GetString();

        switch (type)
        {
            case "Connected":
                Console.WriteLine($"Connected. Request ID: {root.GetProperty("request_id").GetString()}");
                break;
            case "TurnInfo":
                var turnEvent = root.GetProperty("event").GetString();
                var transcript = root.GetProperty("transcript").GetString();
                var turnIndex = root.GetProperty("turn_index").GetInt32();
                switch (turnEvent)
                {
                    case "StartOfTurn":
                        Console.WriteLine($"\n[Turn {turnIndex}] started");
                        break;
                    case "Update":
                        Console.WriteLine($"[Turn {turnIndex}] ... {transcript}");
                        break;
                    case "EagerEndOfTurn": // only sent when eager_eot_threshold is set
                        Console.WriteLine($"[Turn {turnIndex}] (eager) {transcript}");
                        break;
                    case "TurnResumed":
                        Console.WriteLine($"[Turn {turnIndex}] resumed");
                        break;
                    case "EndOfTurn":
                        Console.WriteLine($"[Turn {turnIndex}] FINAL: {transcript}");
                        foreach (var word in root.GetProperty("words").EnumerateArray())
                        {
                            // word-level start/end timestamps may be omitted by the API
                            var start = word.TryGetProperty("start", out var s) ? s.GetDouble() : (double?)null;
                            var end = word.TryGetProperty("end", out var e) ? e.GetDouble() : (double?)null;
                            Console.WriteLine($"    {word.GetProperty("word").GetString()} ({start}s - {end}s)");
                        }
                        break;
                }
                break;
            case "Error": // fatal - the server closes the connection after this
                Console.WriteLine($"Error: {root.GetProperty("code").GetString()} - {root.GetProperty("description").GetString()}");
                break;
            default:
                Console.WriteLine($"Unhandled message type: {type}");
                break;
        }
    }
});

// Stream the raw PCM (skipping the 44-byte WAV header) in ~80ms chunks, paced in real time.
var audio = File.ReadAllBytes("preamble-16k.wav");
for (var offset = WavHeaderBytes; offset < audio.Length; offset += ChunkBytes)
{
    var length = Math.Min(ChunkBytes, audio.Length - offset);
    await ws.SendAsync(new ArraySegment<byte>(audio, offset, length), WebSocketMessageType.Binary, true, CancellationToken.None);
    await Task.Delay(80);
}

// Clean shutdown: send CloseStream, then let the server flush the final results and close.
Console.WriteLine("-> CloseStream");
var closeStream = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { type = "CloseStream" }));
await ws.SendAsync(new ArraySegment<byte>(closeStream), WebSocketMessageType.Text, true, CancellationToken.None);

await Task.WhenAny(receiver, Task.Delay(10000));
if (ws.State == WebSocketState.CloseReceived || ws.State == WebSocketState.Open)
{
    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
}
Console.WriteLine("Done");
