using System.Diagnostics;
using Deepgram.Models;

namespace Deepgram.Tests;
public class TimeoutTest
{
    //you need a active apikey to run this test
    //[Fact]
    //public async Task HttpClient_Should_Timeout_After_Set_Duration()
   // {
      //  // Arrange

     //   var creds = new Credentials() { ApiKey = "" };
     //   var dgc = new DeepgramClient(creds);   
    //    dgc.SetHttpClientTimeout(TimeSpan.FromMilliseconds(1));
     //   var cts = new CancellationTokenSource();
     //   cts.CancelAfter(dgc.HttpClientUtil.HttpClient.Timeout + TimeSpan.FromSeconds(1));  

    //    // Act
    //    var stopwatch = Stopwatch.StartNew();
    //    try
    //    {
    //        var response = await dgc.Transcription.Prerecorded.GetTranscriptionAsync(                
    //            new UrlSource("https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav"),
    //            new PrerecordedTranscriptionOptions()
     //           {
    //                Punctuate = true,
     //               Utterances = true,
    //            }, cts.Token);
//
  //          Assert.Fail("Expected a TaskCanceledException to be thrown");
  //      }
 //       catch (TaskCanceledException)
   //     {
   //         stopwatch.Stop();
//
   //         // Assert
   //         Assert.True(stopwatch.Elapsed >= dgc.HttpClientUtil.HttpClient.Timeout, "The HttpClient did not timeout after the set duration");

    //    }
  //  }
}
