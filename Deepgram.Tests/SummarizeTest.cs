using System;
using Deepgram.Clients;
using Deepgram.Models;
using Deepgram.Request;
using Xunit;

namespace Deepgram.Tests
{
    public class SummarizeTest
    {
        [Fact]
        public async void Should_Return_A_Summary_Short_When_Summarize_Set_To_v2()
        {
            var responseObject = new PrerecordedTranscription()
            {
                Results = new PrerecordedTranscriptionResult()
                {
                    Summary = new Summary() { Short = "This is a test summary" }
                }
            };

            var client = FakeHttpMessageHandler.CreateHttpClientWithResult(responseObject);

            var creds = new Credentials()
            {
                ApiKey = Guid.NewGuid().ToString(),
                ApiUrl = "test.com",
                RequireSSL = true,
            };

            var fakeSource = new UrlSource("https://test.com");

            var fakeOptions = new PrerecordedTranscriptionOptions()
            {
                Summarize = "v2"
            };

            var SUT = new PrerecordedTranscriptionClient(creds);
            SUT._apiRequest = new ApiRequest(client);

            //Act
            var result = await SUT.GetTranscriptionAsync(fakeSource, fakeOptions);

            //Assert         
            Assert.NotNull(result);
            Assert.NotNull(result.Results.Summary.Short);
            Assert.Equal(responseObject.Results.Summary.Short, result.Results.Summary.Short);
        }



        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void Should_Return_A_Summary_Short_When_Summarize_Set_To_bool(bool value)
        {
            var responseObject = new PrerecordedTranscription()
            {
                Results = new PrerecordedTranscriptionResult()
                {
                    Utterances = new Utterance[]
                     {
                        new Utterance()
                        {
                          Words = new Words[]
                           {
                               new Words(){ Word = "test"}
                           }
                        }
                     }
                }
            };

            var client = FakeHttpMessageHandler.CreateHttpClientWithResult(responseObject);

            var creds = new Credentials()
            {
                ApiKey = Guid.NewGuid().ToString(),
                ApiUrl = "test.com",
                RequireSSL = true,
            };

            var fakeSource = new UrlSource("https://test.com");

            var fakeOptions = new PrerecordedTranscriptionOptions()
            {
                Summarize = value
            };

            var SUT = new PrerecordedTranscriptionClient(creds);
            SUT._apiRequest = new ApiRequest(client);

            //Act
            var result = await SUT.GetTranscriptionAsync(fakeSource, fakeOptions);

            //Assert         
            Assert.NotNull(result);
            Assert.NotNull(result.Results);
        }
    }
}