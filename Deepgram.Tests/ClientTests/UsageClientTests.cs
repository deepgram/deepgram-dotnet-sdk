using System;
using AutoBogus;
using Deepgram.Models;
using Deepgram.Tests.Fakers;
using Deepgram.Tests.Fakes;
using Xunit;

namespace Deepgram.Tests.ClientTests
{
    public class UsageClientTests
    {
        [Fact]
        public async void ListRequestsAsync_Should_Return_ListAllRequestResponse()
        {
            //Arrange
            var returnObject = new AutoFaker<ListAllRequestsResponse>().Generate();
            var listAllRequestOptions = new AutoFaker<ListAllRequestsOptions>().Generate();
            var projectId = Guid.NewGuid().ToString();
            var SUT = GetDeepgramClient(returnObject);
            //Act
            var result = await SUT.Usage.ListAllRequestsAsync(projectId, listAllRequestOptions);

            //Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<ListAllRequestsResponse>(result);
            Assert.Equal(returnObject, result);
        }

        [Fact]
        public async void GetUsageRequestAsync_Should_Return_UsageRequest()
        {
            //Arrange
            var returnObject = new AutoFaker<UsageRequest>().Generate();
            var projectId = Guid.NewGuid().ToString();
            var requestId = Guid.NewGuid().ToString();
            var SUT = GetDeepgramClient(returnObject);
            //Act
            var result = await SUT.Usage.GetUsageRequestAsync(projectId, requestId);

            //Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<UsageRequest>(result);
            Assert.Equal(returnObject, result);
        }

        [Fact]
        public async void GetUsageSummaryAsync_Should_Return_UsageSummary()
        {
            //Arrange
            var returnObject = new AutoFaker<UsageSummary>().Generate();
            var projectId = Guid.NewGuid().ToString();
            var getUsageOptions = new AutoFaker<GetUsageSummaryOptions>().Generate();
            var SUT = GetDeepgramClient(returnObject);
            //Act
            var result = await SUT.Usage.GetUsageSummaryAsync(projectId, getUsageOptions);

            //Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<UsageSummary>(result);
            Assert.Equal(returnObject, result);
        }

        [Fact]
        public async void GetUsageFieldsAsync_Should_Return_UsageFields()
        {
            //Arrange
            var returnObject = new AutoFaker<UsageFields>().Generate();
            var projectId = Guid.NewGuid().ToString();
            var getUsageFieldOptions = new AutoFaker<GetUsageFieldsOptions>().Generate();
            var SUT = GetDeepgramClient(returnObject);
            //Act
            var result = await SUT.Usage.GetUsageFieldsAsync(projectId, getUsageFieldOptions);

            //Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<UsageFields>(result);
            Assert.Equal(returnObject, result);
        }






        private static DeepgramClient GetDeepgramClient<T>(T returnObject)
        {
            var mockIRequestMessageBuilder = MockIRequestMessageBuilder.Create();
            var mockIApiRequest = MockIApiRequest.Create(returnObject);
            var credentials = new CredentialsFaker().Generate();
            var SUT = new DeepgramClient(credentials);
            SUT.Usage.ApiRequest = mockIApiRequest.Object;
            SUT.Usage.RequestMessageBuilder = mockIRequestMessageBuilder.Object;
            return SUT;
        }


    }
}
