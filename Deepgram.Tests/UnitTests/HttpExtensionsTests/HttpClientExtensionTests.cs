// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Manage.v1;
using Deepgram.Encapsulations;

namespace Deepgram.Tests.UnitTests.ExtensionsTests;

public class HttpClientTests
{
    readonly string _customUrl = "acme.com";
    IHttpClientFactory _httpClientFactory;

    [SetUp]
    public void Setup()
    {
        _httpClientFactory = Substitute.For<IHttpClientFactory>();
    }

    [Test]
    public void Should_Return_HttpClient_With_Default_BaseAddress()
    {
        // Input and Output 
        var _apiKey = new Faker().Random.Guid().ToString();
        var _clientOptions = new DeepgramHttpClientOptions(_apiKey);

        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithResult(new MessageResponse(), HttpStatusCode.OK);
        _httpClientFactory.CreateClient().Returns(httpClient);

        //Act
        var SUT = HttpClientFactory.ConfigureDeepgram(httpClient, _clientOptions);

        //Assert 
        using (new AssertionScope())
        {
            SUT.Should().NotBeNull();
            SUT.BaseAddress.Should().Be($"https://{Defaults.DEFAULT_URI}/v1");
        };
    }

    [Test]
    public void Should_Return_HttpClient_With_Custom_BaseAddress()
    {
        // Input and Output
        var expectedBaseAddress = $"https://{_customUrl}/v1";
        var customBaseAddress = $"https://{_customUrl}";
        var _apiKey = new Faker().Random.Guid().ToString();
        var _clientOptions = new DeepgramHttpClientOptions(_apiKey, customBaseAddress);

        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithResult(new MessageResponse(), HttpStatusCode.OK, expectedBaseAddress);
        _httpClientFactory.CreateClient().Returns(httpClient);

        //Act
        var SUT = HttpClientFactory.ConfigureDeepgram(httpClient, _clientOptions);

        //Assert 
        using (new AssertionScope())
        {
            SUT.Should().NotBeNull();
            SUT.BaseAddress.Should().Be(expectedBaseAddress);
        };
    }

    [Test]
    public void Should_Return_HttpClient_With_Default_BaseAddress_And_Custom_Headers()
    {
        // Input and Output 
        var _apiKey = new Faker().Random.Guid().ToString();
        var _clientOptions = new DeepgramHttpClientOptions(_apiKey, null, null, null, FakeHeaders());

        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithResult(new MessageResponse(), HttpStatusCode.OK);
        _httpClientFactory.CreateClient().Returns(httpClient);

        //Act
        var SUT = HttpClientFactory.ConfigureDeepgram(httpClient, _clientOptions);

        //Assert 
        using (new AssertionScope())
        {
            SUT.Should().NotBeNull();
            SUT.BaseAddress.Should().Be($"https://{Defaults.DEFAULT_URI}/v1");
            SUT.DefaultRequestHeaders.Should().ContainKey(_clientOptions.Headers.First().Key);
        };
    }

    [Test]
    public void Should_Return_HttpClient_With_Custom_BaseAddress_And_Custom_Headers()
    {
        // Input and Output
        var expectedBaseAddress = $"https://{_customUrl}/v1";
        var customBaseAddress = $"https://{_customUrl}";
        var _apiKey = new Faker().Random.Guid().ToString();
        var _clientOptions = new DeepgramHttpClientOptions(_apiKey, customBaseAddress, null, null, FakeHeaders());

        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithResult(new MessageResponse(), HttpStatusCode.OK);
        httpClient.BaseAddress = null;
        _httpClientFactory.CreateClient().Returns(httpClient);


        //Act
        var SUT = HttpClientFactory.ConfigureDeepgram(httpClient, _clientOptions);

        //Assert 
        using (new AssertionScope())
        {
            SUT.Should().NotBeNull();
            SUT.BaseAddress.Should().Be(expectedBaseAddress);
            SUT.DefaultRequestHeaders.Should().ContainKey(_clientOptions.Headers.First().Key);
        };
    }

    [Test]
    public void Should_Return_HttpClient_With_Predefined_Values()
    {
        // Input and Output
        var expectedBaseAddress = $"https://{_customUrl}/v1";
        var customBaseAddress = $"https://{_customUrl}";
        var _apiKey = new Faker().Random.Guid().ToString();
        var _clientOptions = new DeepgramHttpClientOptions(_apiKey, customBaseAddress, null, null, FakeHeaders());

        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithResult(new MessageResponse(), HttpStatusCode.OK, expectedBaseAddress);
        _httpClientFactory.CreateClient().Returns(httpClient);

        //Act
        var SUT = HttpClientFactory.ConfigureDeepgram(httpClient, _clientOptions);

        //Assert 
        using (new AssertionScope())
        {
            SUT.Should().NotBeNull();
            SUT.BaseAddress.Should().Be(expectedBaseAddress);
            SUT.DefaultRequestHeaders.Should().ContainKey(_clientOptions.Headers.First().Key);
        };
    }

    private static Dictionary<string, string> FakeHeaders()
    {
        var headers = new Dictionary<string, string>();
        var headersCount = new Random().Next(1, 3);
        for (var i = 0; i < headersCount; i++)
        {
            headers.Add($"key{i}", $"value{i}");
        }

        return headers;
    }
}

