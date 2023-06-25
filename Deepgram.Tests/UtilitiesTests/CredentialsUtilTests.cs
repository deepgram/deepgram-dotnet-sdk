using System;
using Deepgram.Utilities;
using Xunit;

namespace Deepgram.Tests.UtilitiesTests
{
    public class CredentialsUtilTests
    {
        [Fact]
        public void CheckApiKey_Should_Return_Same_APIKey_That_Passed_As_Parameter()
        {
            //Act
            var fakeKey = Guid.NewGuid().ToString();
            var result = CredentialsUtil.CheckApiKey(fakeKey);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
            Assert.Equal(fakeKey, result);
        }

        [Fact]
        public void CheckApiKey_Should_Throw_ArgumentException_If_No_ApiKey_Found()
        {
            //Act

            var result = Assert.Throws<ArgumentException>(() => CredentialsUtil.CheckApiKey(null));

            //Assert
            Assert.IsType<ArgumentException>(result);
            Assert.Equal("Deepgram API Key must be provided in constructor", result.Message);
        }

        [Fact]
        public void CheckApiUrl_Should_Return_TrimmedApiUrl_That_Passed_As_Parameter()
        {
            //Act
            var fakeUrl = "http://test.com";
            var result = CredentialsUtil.CleanApiUrl(fakeUrl);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
            Assert.Equal("test.com", result);
        }



        [Fact]
        public void CheckApiUrl_Should_Return_DefaultApiUrl_If_NO_ApiUrl_Present_In_Parameters()
        {
            //Act

            var result = CredentialsUtil.CleanApiUrl(null);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
            Assert.Equal("api.deepgram.com", result);
        }




        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CheckApiUrl_Should_Return_RequireSSL_That_Passed_As_Parameter(bool? requireSSL)
        {
            //Act

            var result = CredentialsUtil.CleanRequireSSL(requireSSL);

            //Assert
            Assert.IsType<bool>(result);
            Assert.Equal(requireSSL, result);
        }


    }
}