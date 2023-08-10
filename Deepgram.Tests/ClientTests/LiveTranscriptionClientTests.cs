using Deepgram.Clients;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Deepgram.Tests.ClientTests;
public class LiveTranscriptionClientTests
{
    // Fake
    class LiveTranscriptionClientFake : LiveTranscriptionClient
    {
        private readonly Action<byte[]> _sendDataCallback;
        public LiveTranscriptionClientFake(Action<byte[]> sendDataCallback) : base(new Models.Credentials("fake", "fake"))
        {
            _sendDataCallback = sendDataCallback ?? throw new ArgumentNullException(nameof(sendDataCallback));
        }

        public override void SendData(byte[] data)
        {
            _sendDataCallback(data);
        }
    }

    [Fact]
    public void Test_OnKeepAliveExecuted_SendDataCalledWithData()
    {
        // Arrange
        var keepAliveMessage = JsonConvert.SerializeObject(new { type = "KeepAlive" });
        var expected = Encoding.Default.GetBytes(keepAliveMessage);

        var sendDataCalled = false;
        byte[] sendDataRecieved = null;
        var client = new LiveTranscriptionClientFake((data) => { 
            sendDataCalled = true;
            sendDataRecieved = data;
        });

        // Act
        client.KeepAlive();

        // Assert
        Assert.True(sendDataCalled);
        Assert.Equal(sendDataRecieved, expected);
    }

    [Fact]
    public async Task Test_OnStartConnectionExecuted_SendDataCalled()
    {
        // Arrange
        var sendDataCalled = false;
        var client = new LiveTranscriptionClientFake((data) => sendDataCalled = true);

        // Act
        await client.StartConnectionAsync(new Models.LiveTranscriptionOptions());

        // Assert
        Assert.True(sendDataCalled);
    }
}
