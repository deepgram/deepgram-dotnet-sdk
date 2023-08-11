using AutoBogus;
using Deepgram.Models;
using Deepgram.Tests.Fakes;

namespace Deepgram.Tests.ClientTests
{
    public class KeyClientTests
    {
        [Fact]
        public async void ListKeysAsync_Should_Return_KeyList()
        {
            //Arrange
            var returnObject = new AutoFaker<KeyList>().Generate();
            var SUT = GetDeepgramClient(returnObject);
            var projectId = Guid.NewGuid().ToString();

            //Act
            var result = await SUT.Keys.ListKeysAsync(projectId);

            //Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<KeyList>(result);
            Assert.Equal(returnObject, result);
        }

        [Fact]
        public async void GetKeyAsync_Should_Return_Key()
        {
            //Arrange 
            var returnObject = new AutoFaker<Key>().Generate();
            var SUT = GetDeepgramClient(returnObject);

            var projectId = Guid.NewGuid().ToString();
            var keyId = Guid.NewGuid().ToString();

            //Act
            var result = await SUT.Keys.GetKeyAsync(projectId, keyId);

            //Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<Key>(result);
            Assert.Equal(returnObject, result);
        }

        [Fact]
        public async void CreateKeyAsync_Should_Return_ApiKey()
        {
            //Arrange
            var returnObject = new AutoFaker<ApiKey>().Generate();
            DeepgramClient SUT = GetDeepgramClient(returnObject);

            var faker = new Faker();
            var projectId = faker.Random.Guid().ToString();
            var scopes = faker.Random.WordsArray(1, 3);
            var comment = faker.Lorem.Sentence();

            //Act
            var result = await SUT.Keys.CreateKeyAsync(projectId, comment, scopes);

            //Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<ApiKey>(result);
            Assert.Equal(returnObject, result);
        }

        [Fact]
        public async void DeleteKeyAsync_Should_Return_MessageResponse()
        {
            //Arrange
            var returnObject = new AutoFaker<MessageResponse>().Generate();
            var SUT = GetDeepgramClient(returnObject);

            var projectId = Guid.NewGuid().ToString();
            var keyId = Guid.NewGuid().ToString();

            //Act
            var result = await SUT.Keys.DeleteKeyAsync(projectId, keyId);

            //Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<MessageResponse>(result);
            Assert.Equal(returnObject, result);
        }



        private static DeepgramClient GetDeepgramClient<T>(T returnObject)
        {
            var mockIRequestMessageBuilder = MockIRequestMessageBuilder.Create();
            var mockIApiRequest = MockIApiRequest.Create(returnObject);
            var credentials = new CredentialsFaker().Generate();
            var SUT = new DeepgramClient(credentials);
            SUT.Keys.ApiRequest = mockIApiRequest;
            SUT.Keys.RequestMessageBuilder = mockIRequestMessageBuilder;
            return SUT;
        }


    }
}
