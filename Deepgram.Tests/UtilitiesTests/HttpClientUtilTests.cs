using System;
using System.Net.Http;
using Deepgram.Utilities;
using Xunit;

namespace Deepgram.Tests.UtilitiesTests
{
    public class HttpClientUtilTests
    {
        [Fact]
        public void GetUserAgent_Should_Return_HttpClient_With_Accept_And_UserAgent_Headers_Set()
        {
            //Arrange
            var httpClientUtil = new HttpClientUtil();
            var agent = UserAgentUtil.GetUserAgent();

            //Act
            var result = httpClientUtil.HttpClient;

            //Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<HttpClient>(result);
            Assert.Equal("application/json", result.DefaultRequestHeaders.Accept.ToString());
            Assert.Equal(agent, result.DefaultRequestHeaders.UserAgent.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_NotExecutedWithinTimeout_ThrowsExceptionAsync()
        {
            // Arrange
            var endpoint = "https://google.com/";
            var timeoutShouldFail = TimeSpan.FromMilliseconds(1);

            var httpClientUtil = new HttpClientUtil();
            httpClientUtil.SetTimeOut(timeoutShouldFail);

            var client = httpClientUtil.HttpClient;
            bool resultShouldFail = false;

            // Act
            try
            {
                await client.GetAsync(endpoint);
            }
            catch (TaskCanceledException)
            {
                // we are expecting this to timeout
                resultShouldFail = true;
            }

            // Assert
            Assert.True(resultShouldFail);
        }

        [Fact]
        public async Task ExecuteAsync_ExecutedWithinTimeout_ThrowsExceptionAsync()
        {
            // Arrange
            var endpoint = "https://google.com/";
            var timeoutShouldSucceed = TimeSpan.FromMilliseconds(2000);

            var httpClientUtil = new HttpClientUtil();
            httpClientUtil.SetTimeOut(timeoutShouldSucceed);

            var client = httpClientUtil.HttpClient;
            bool resultShouldSucceed = false;

            // Act
            try
            {
                await client.GetAsync(endpoint);
            }
            catch (TaskCanceledException)
            {
                // we should not see the exception thrown
                resultShouldSucceed = true;
            }

            // Assert
            Assert.False(resultShouldSucceed);
        }
    }
}