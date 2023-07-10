using System.Net.Http;
using AutoBogus;
using Deepgram.Common;
using Deepgram.Projects;
using Deepgram.Request;
using Deepgram.Tests.Fakers;
using Deepgram.Tests.Fakes;
using Deepgram.Transcription;
using Deepgram.Usage;
using Xunit;

namespace Deepgram.Tests.RequestTests
{

    public class ApiRequestTests
    {
        [Fact]

        public async void Should_Return_A_Project_When_HttpMethod_Get_Without_Parameters()
        {
            //Arrange
            var responseObject = new AutoFaker<Project>().Generate();
            var credentials = new CleanCredentialsFaker().Generate();
            var client = FakeHttpMessageHandler.CreateHttpClientWithResult(responseObject);
            var SUT = new ApiRequest(client, credentials);

            //Act
            var result = await SUT.DoRequestAsync<Project>(HttpMethod.Get, "fake.com", null, null);

            //Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<Project>(result);
        }

        [Fact]

        public async void Should_Return_A_object_When_HttpMethod_Get_With_Parameters()
        {
            //Arrange
            var responseObject = new AutoFaker<UsageSummary>().Generate();
            var credentials = new CleanCredentialsFaker().Generate();
            var client = FakeHttpMessageHandler.CreateHttpClientWithResult(responseObject);
            var SUT = new ApiRequest(client, credentials);
            var parameters = new AutoFaker<GetUsageSummaryOptions>().Generate();
            //Act
            var result = await SUT.DoRequestAsync<UsageSummary>(HttpMethod.Get, "fake.com", null, parameters);

            //Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<UsageSummary>(result);
        }

        [Fact]
        public async void Should_Return_A_MessageResponse_When_HttpMethod_Put()
        {
            //Arrange
            var responseObject = new AutoFaker<MessageResponse>().Generate();
            var credentials = new CleanCredentialsFaker().Generate();
            var client = FakeHttpMessageHandler.CreateHttpClientWithResult(responseObject);
            var SUT = new ApiRequest(client, credentials);
            var options = new AutoFaker<UpdateScopeOptions>().Generate();
            //Act
            var result = await SUT.DoRequestAsync<MessageResponse>(HttpMethod.Put, "fake.com", options, null);

            //Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<MessageResponse>(result);
        }

        [Fact]
        public async void Should_Return_A_PrerecorededTranscription_When_HttpMethod_Post_Using_UrlSource()
        {
            //Arrange
            var responseObject = new AutoFaker<PrerecordedTranscription>().Generate();
            var credentials = new CleanCredentialsFaker().Generate();
            var client = FakeHttpMessageHandler.CreateHttpClientWithResult(responseObject);
            var SUT = new ApiRequest(client, credentials);
            var source = new AutoFaker<UrlSource>().Generate();
            var options = new PrerecordedTranscriptionOptionsFaker().Generate();
            //Act
            var result = await SUT.DoRequestAsync<PrerecordedTranscription>(HttpMethod.Post, "fake.com", source, options);

            //Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<PrerecordedTranscription>(result);
        }

        [Fact]
        public async void Should_Return_A_PrerecorededTranscription_When_HttpMethod_Post_Using_StreamSource()
        {
            //Arrange
            var responseObject = new AutoFaker<PrerecordedTranscription>().Generate();
            var credentials = new CleanCredentialsFaker().Generate();
            var client = FakeHttpMessageHandler.CreateHttpClientWithResult(responseObject);
            var SUT = new ApiRequest(client, credentials);
            var source = new StreamSourceFaker().Generate();
            var options = new PrerecordedTranscriptionOptionsFaker().Generate();
            //Act
            var result = await SUT.DoStreamRequestAsync<PrerecordedTranscription>(HttpMethod.Post, "fake.com", source, options);

            //Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<PrerecordedTranscription>(result);
        }


        [Fact]
        public async void Should_Return_A_MessageResponse_When_HttpMethod_Patch()
        {
            //Arrange
            var responseObject = new AutoFaker<MessageResponse>().Generate();
            var credentials = new CleanCredentialsFaker().Generate();
            var client = FakeHttpMessageHandler.CreateHttpClientWithResult(responseObject);
            var SUT = new ApiRequest(client, credentials);
            var body = new AutoFaker<Project>().Generate();
            //Act
            var result = await SUT.DoRequestAsync<MessageResponse>(HttpMethod.Patch, "fake.com", body, null);

            //Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<MessageResponse>(result);
        }

        [Fact]
        public async void Should_Return_A_MessageResponse_When_HttpMethod_Delete()
        {
            //Arrange
            var responseObject = new AutoFaker<MessageResponse>().Generate();
            var credentials = new CleanCredentialsFaker().Generate();
            var client = FakeHttpMessageHandler.CreateHttpClientWithResult(responseObject);
            var SUT = new ApiRequest(client, credentials);

            //Act
            var result = await SUT.DoRequestAsync<MessageResponse>(HttpMethod.Delete, "fake.com", null, null);

            //Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<MessageResponse>(result);
        }

    }
}