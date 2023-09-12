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
    public void Test_SendDataCalledWithData()
    {
        // Arrange
        var expected = Encoding.Default.GetBytes("0000");

        var sendDataCalled = false;
        byte[] sendDataRecieved = null;
        var client = new LiveTranscriptionClientFake((data) => { 
            sendDataCalled = true;
            sendDataRecieved = data;
        });

        // Act
        client.SendData(expected);

        // Assert
        Assert.True(sendDataCalled);
        Assert.Equal(sendDataRecieved, expected);
    }
}
