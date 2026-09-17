using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Deepgram.Abstractions.v2;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class WebSocketUpgradeTests
{
    [Test]
    public async Task Connect_Does_Not_Send_Content_Length_Header()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var requestTask = AcceptUpgrade(listener);
        using var client = new TestWebSocketClient();

        var connected = await client.Connect($"ws://127.0.0.1:{port}/v1/listen");
        var stopped = await client.Stop();
        var request = await requestTask;

        Assert.Multiple(() =>
        {
            Assert.That(connected, Is.True);
            Assert.That(stopped, Is.True);
            Assert.That(request.Contains("Content-Length:", StringComparison.OrdinalIgnoreCase), Is.False);
        });
    }

    private static async Task<string> AcceptUpgrade(TcpListener listener)
    {
        using var connection = await listener.AcceptTcpClientAsync();
        await using var stream = connection.GetStream();
        using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
        var request = new StringBuilder();
        string? key = null;

        while (await reader.ReadLineAsync() is { } line && line.Length > 0)
        {
            request.AppendLine(line);
            if (line.StartsWith("Sec-WebSocket-Key: ", StringComparison.OrdinalIgnoreCase))
            {
                key = line[19..];
            }
        }

        Assert.That(key, Is.Not.Null);
        var accept = Convert.ToBase64String(SHA1.HashData(Encoding.ASCII.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
        var response = $"HTTP/1.1 101 Switching Protocols\r\nConnection: Upgrade\r\nUpgrade: websocket\r\nSec-WebSocket-Accept: {accept}\r\n\r\n";
        await stream.WriteAsync(Encoding.ASCII.GetBytes(response));
        await stream.FlushAsync();

        var closeFrame = new byte[8];
        await stream.ReadExactlyAsync(closeFrame);
        Assert.That(closeFrame[0], Is.EqualTo(0x88));

        // Reply to the client's close frame so the receiver exits without an expected transport error.
        await stream.WriteAsync(new byte[] { 0x88, 0x02, 0x03, 0xe8 });
        await stream.FlushAsync();

        return request.ToString();
    }

    private sealed class TestWebSocketClient : AbstractWebSocketClient
    {
        public TestWebSocketClient() : base("test") { }

        public override Task SendClose(bool nullByte = false, CancellationTokenSource? cancellationToken = null) => Task.CompletedTask;
    }
}
