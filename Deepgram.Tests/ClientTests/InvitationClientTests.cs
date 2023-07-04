using AutoBogus;
using Bogus;
using Deepgram.Common;
using Deepgram.Invitation;
using Deepgram.Tests.Fakers;
using Deepgram.Tests.Fakes;
using Xunit;

namespace Deepgram.Tests.ClientTests
{

    public class InvitationClientTests
    {
        [Fact]
        public async void ListInvitationsAsync_Should_Return_InvitationList()
        {
            //Arrange
            var returnObject = new AutoFaker<InvitationList>().Generate();
            var SUT = GetDeepgramClient(returnObject);
            var projectId = new Faker().Random.Guid().ToString();

            //Act
            var result = await SUT.Invitation.ListInvitationsAsync(projectId);

            //Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<InvitationList>(result);
            Assert.Equal(returnObject, result);
        }

        [Fact]
        public async void SendInvitation_Should_Return_InvitationResponse()
        {
            //Arrange
            var returnObject = new AutoFaker<InvitationResponse>().Generate();
            var invitationOptions = new AutoFaker<InvitationOptions>().Generate();
            var SUT = GetDeepgramClient(returnObject);
            var projectId = new Faker().Random.Guid().ToString();

            //Act
            var result = await SUT.Invitation.SendInvitationAsync(projectId, invitationOptions);

            //Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<InvitationResponse>(result);
            Assert.Equal(returnObject, result);
        }

        [Fact]
        public async void DeleteInvitation_Should_Return_MessageResponse()
        {

            //Arrange
            var returnObject = new AutoFaker<MessageResponse>().Generate();
            var email = new Faker().Internet.Email();
            var SUT = GetDeepgramClient(returnObject);
            var projectId = new Faker().Random.Guid().ToString();

            //Act
            var result = await SUT.Invitation.DeleteInvitationAsync(projectId, email);

            //Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<MessageResponse>(result);
            Assert.Equal(returnObject, result);
        }


        private static DeepgramClient GetDeepgramClient<T>(T returnObject)
        {
            var mockIRequestMessageBuilder = MockIRequestMessageBuilder.Create();
            var mockIApiRequest = MockIApiRequest.Create(returnObject);
            mockIApiRequest.SetupGet(x => x._requestMessageBuilder).Returns(mockIRequestMessageBuilder.Object);
            var credentials = new CredentialsFaker().Generate();
            var SUT = new DeepgramClient(credentials);
            SUT.Invitation.ApiRequest = mockIApiRequest.Object;
            return SUT;
        }
    }
}