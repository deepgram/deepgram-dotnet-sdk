// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Speak.v1;
using Deepgram.Clients.Speak.v1;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class SpeakClientTests
{
    DeepgramClientOptions _options;
    string _apiKey;

    [SetUp]
    public void Setup()
    {
        _options = new DeepgramClientOptions();
        _apiKey = new Faker().Random.Guid().ToString();
    }

    //[Test]
    //public async Task SpeakStream_Should_Call_PostFileAsync_Returning_SyncResponse()
    //{
    //    // Input and Output
    //    var url = AbstractRestClient.GetUri(_options, $"{UriSegments.SPEAK}");
    //    var expectedResponse = Arg.Any<LocalFileWithMetadata>();
    //    var speakSchema = new AutoFaker<SpeakSchema>().Generate();
    //    speakSchema.CallBack = null;
    //    var source = new TextSource("Hello World!");
    //    var keys = new List<string> { "model" };

    //    // Fake Client
    //    var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
    //    var speakClient = Substitute.For<SpeakClient>(_apiKey, _options);

    //    // Mock Methods
    //    speakClient.When(x => x.PostRetrieveLocalFileAsync<TextSource, SpeakSchema, LocalFileWithMetadata>(Arg.Any<string>(), Arg.Any<SpeakSchema>(), Arg.Any<TextSource>())).DoNotCallBase();
    //    speakClient.PostRetrieveLocalFileAsync<TextSource, SpeakSchema, LocalFileWithMetadata>(url, Arg.Any<SpeakSchema>(), Arg.Any<TextSource>()).Returns(expectedResponse);

    //    // Act
    //    var result = await speakClient.ToStream(source, speakSchema);

    //    // Assert
    //    await speakClient.Received().PostRetrieveLocalFileAsync<TextSource, SpeakSchema, LocalFileWithMetadata>(url, Arg.Any<SpeakSchema>(), Arg.Any<TextSource>());
    //    using (new AssertionScope())
    //    {
    //        result.Should().NotBeNull();
    //        result.Should().BeAssignableTo<SyncResponse>();
    //        result.Should().BeEquivalentTo(expectedResponse);
    //    }
    //}

    [Test]
    public async Task StreamCallBack_With_CallBack_Property_Should_Call_PostAsync_Returning_SyncResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.SPEAK}");
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var speakSchema = new AutoFaker<SpeakSchema>().Generate();
        var source = new TextSource("Hello World!");

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var speakClient = Substitute.For<SpeakClient>(_apiKey, _options);
        
        // Mock Methods
        speakClient.When(x => x.PostAsync<TextSource, SpeakSchema, AsyncResponse>(Arg.Any<string>(), Arg.Any<SpeakSchema>(), Arg.Any<TextSource>())).DoNotCallBase();
        speakClient.PostAsync<TextSource, SpeakSchema, AsyncResponse>(url, Arg.Any<SpeakSchema>(), Arg.Any<TextSource>()).Returns(expectedResponse);

        // Act
        var result = await speakClient.StreamCallBack(source, null, speakSchema);

        // Assert
        await speakClient.Received().PostAsync<TextSource, SpeakSchema, AsyncResponse>(url, Arg.Any<SpeakSchema>(), Arg.Any<TextSource>());
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<AsyncResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task StreamCallBack_With_CallBack_Parameter_Should_Call_PostAsync_Returning_SyncResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.SPEAK}");
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var speakSchema = new AutoFaker<SpeakSchema>().Generate();
        var source = new TextSource("Hello World!");

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var speakClient = Substitute.For<SpeakClient>(_apiKey, _options);
        
        // Mock Methods
        speakClient.When(x => x.PostAsync<TextSource, SpeakSchema, AsyncResponse>(Arg.Any<string>(), Arg.Any<SpeakSchema>(), Arg.Any<TextSource>())).DoNotCallBase();
        speakClient.PostAsync<TextSource, SpeakSchema, AsyncResponse>(url, Arg.Any<SpeakSchema>(), Arg.Any<TextSource>()).Returns(expectedResponse);

        //before we act to test this call with the callBack parameter and not the callBack property we need to null the callBack property
        var callBack = speakSchema.CallBack;
        speakSchema.CallBack = null;

        // Act
        var result = await speakClient.StreamCallBack(source, callBack, speakSchema);

        // Assert
        await speakClient.Received().PostAsync<TextSource, SpeakSchema, AsyncResponse>(url, Arg.Any<SpeakSchema>(), Arg.Any<TextSource>());
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<AsyncResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task StreamCallBack_Throw_ArgumentException_With_CallBack_Property_And_CallBack_Parameter_Set()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.SPEAK}");
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var speakSchema = new AutoFaker<SpeakSchema>().Generate();
        var source = new TextSource("Hello World!");

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var speakClient = Substitute.For<SpeakClient>(_apiKey, _options);
        
        // Mock Methods
        speakClient.When(x => x.PostAsync<Stream, SpeakSchema, AsyncResponse>(Arg.Any<string>(), Arg.Any<SpeakSchema>(), Arg.Any<Stream>())).DoNotCallBase();
        speakClient.PostAsync<Stream, SpeakSchema, AsyncResponse>(url, Arg.Any<SpeakSchema>(), Arg.Any<Stream>()).Returns(expectedResponse);
        var callBack = speakSchema.CallBack;

        // Act and Assert
        await speakClient.Invoking(y => y.StreamCallBack(source, callBack, speakSchema))
           .Should().ThrowAsync<ArgumentException>();

        await speakClient.DidNotReceive().PostAsync<Stream, SpeakSchema, AsyncResponse>(url, Arg.Any<SpeakSchema>(), Arg.Any<Stream>());
    }

    [Test]
    public async Task StreamCallBack_Should_Throw_ArgumentException_With_No_CallBack_Set()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.SPEAK}");
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var speakSchema = new AutoFaker<SpeakSchema>().Generate();
        var source = new TextSource("Hello World!");

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var speakClient = Substitute.For<SpeakClient>(_apiKey, _options);
        
        // Mock Methods
        speakClient.When(x => x.PostAsync<Stream, SpeakSchema, AsyncResponse>(Arg.Any<string>(), Arg.Any<SpeakSchema>(), Arg.Any<Stream>())).DoNotCallBase();
        speakClient.PostAsync<Stream, SpeakSchema, AsyncResponse>(url, Arg.Any<SpeakSchema>(), Arg.Any<Stream>()).Returns(expectedResponse);
        
        speakSchema.CallBack = null;

        // Act and Assert
        await speakClient.Invoking(y => y.StreamCallBack(source, null, speakSchema))
           .Should().ThrowAsync<ArgumentException>();

        await speakClient.DidNotReceive().PostAsync<Stream, SpeakSchema, AsyncResponse>(url, Arg.Any<SpeakSchema>(), Arg.Any<Stream>());
    }
}
