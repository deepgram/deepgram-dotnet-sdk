using System.Net.Http;
using AutoBogus;
using Deepgram.Projects;
using Deepgram.Request;
using Deepgram.Tests.Fakers;
using Deepgram.Tests.Fakes;
using Xunit;

namespace Deepgram.Tests.RequestTests
{

    public class ApiRequestTests
    {
        [Fact]

        public async void Should_Return_A_Valid_Object_When_Deserialized()
        {
            //Arrange
            var responseObject = new AutoFaker<Project>().Generate();
            var client = FakeHttpMessageHandler.CreateHttpClientWithResult(responseObject);
            var SUT = new ApiRequest(client);

            //Act
            var result = await SUT.DoRequestAsync<Project>(
                HttpMethod.Get,
                "test.com",
                new CleanCredentialsFaker().Generate(),
           null,
           null
                );

            //Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<Project>(result);
        }
    }
}