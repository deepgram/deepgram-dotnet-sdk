using System;
using System.Collections.Generic;
using Bogus;
using Deepgram.Utilities;
using Xunit;

namespace Deepgram.Tests.UtilitiesTests
{

    public class UriUtilTests
    {
        readonly string _uriSegment;
        readonly string _apiUrl;

        public UriUtilTests()
        {
            _uriSegment = new Faker().Lorem.Word();
            var domain = new Faker().Internet.Url();
            _apiUrl = domain.Substring(domain.IndexOf("//") + 2);
        }
        [Theory]
        [InlineData("https")]
        [InlineData("http")]
        [InlineData("ws")]
        [InlineData("wss")]

        public void ResolveUri_Should_Return_Uri_Without_Parameters(string protocol)
        {
            //Act
            var result = UriUtil.ResolveUri(_apiUrl, _uriSegment, protocol);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<Uri>(result);
            Assert.Equal($"{protocol}://{_apiUrl}/v1/{_uriSegment}", result.AbsoluteUri);
            Assert.Equal(protocol, result.Scheme);
            Assert.Equal(_apiUrl, result.Host);
            Assert.Contains(_uriSegment, result.Segments);
        }

        [Theory]
        [InlineData("https")]
        [InlineData("http")]
        [InlineData("ws")]
        [InlineData("wss")]

        public void ResolveUri_Should_Return_Uri_With_Parameters(string protocol)
        {
            var parameters = new Dictionary<string, string>
    {
        { "key", "value" }
    };
            //Act
            var result = UriUtil.ResolveUri(_apiUrl, _uriSegment, protocol, parameters);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<Uri>(result);
            Assert.Equal($"{protocol}://{_apiUrl}/v1/{_uriSegment}?key=value", result.AbsoluteUri);
            Assert.Equal(protocol, result.Scheme);
            Assert.Equal(_apiUrl, result.Host);
            Assert.Contains(_uriSegment, result.Segments);
            Assert.Contains("key=value", result.Query);
        }
    }
}