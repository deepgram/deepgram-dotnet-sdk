namespace Deepgram.Tests;
public class TimeoutTest
{
    // you need a active apikey to run this test
    //[Fact]
    //public async Task HttpClient_Should_Timeout_After_Set_Duration()
    //{
    //    // Arrange
    //    var creds = new Credentials() { ApiKey =  };
    //    var dgc = new DeepgramClient(creds);
    //    // Set the timeout to 2 seconds
    //    dgc.SetHttpClientTimeout(TimeSpan.FromMilliseconds(1));
    //    var cts = new CancellationTokenSource();
    //    cts.CancelAfter(dgc.HttpClientUtil.HttpClient.Timeout + TimeSpan.FromSeconds(1));  // Cancel the task after 3 seconds

    //    // Act
    //    var stopwatch = Stopwatch.StartNew();
    //    try
    //    {

    //        var response = await dgc.Transcription.Prerecorded.GetTranscriptionAsync(
    //            // UNCOMMENT IF USING LOCAL FILE:
    //            // new Deepgram.Transcription.StreamSource(
    //            //     fs,
    //            //     "audio/wav"),
    //            new UrlSource("https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav"),
    //            new PrerecordedTranscriptionOptions()
    //            {
    //                Punctuate = true,
    //                Utterances = true,
    //            }, cts.Token);

    //        Assert.Fail("Expected a TaskCanceledException to be thrown");
    //    }
    //    catch (TaskCanceledException)
    //    {
    //        stopwatch.Stop();

    //        // Assert
    //        Assert.True(stopwatch.Elapsed >= dgc.HttpClientUtil.HttpClient.Timeout, "The HttpClient did not timeout after the set duration");

    //    }
    //}
}
