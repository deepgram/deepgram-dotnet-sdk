// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Speak.v1;
using Deepgram.Clients.Speak.v1;
using NSubstitute;

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
    //    //Arrange 
    //    var expectedResponse = new AutoFaker<List<(Dictionary<string, string>, MemoryStream)>>().Generate();
    //    var speakSchema = new AutoFaker<SpeakSchema>().Generate();
    //    speakSchema.CallBack = null;
    //    var source = new TextSource("Hello World!");
    //    var keys = new List<string> { "key1", "key2" };
    //    var stringedOptions = QueryParameterUtil.GetParameters(speakSchema);
    //    var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

    //    var speakClient = Substitute.For<SpeakClient>(_apiKey, _options);


    //    speakClient.When(x => x.PostFileAsync<(Dictionary<string, string>, MemoryStream)>(Arg.Any<string>(), Arg.Any<HttpContent>(), Arg.Any<List<string>>())).DoNotCallBase();
    //    speakClient.PostFileAsync<List<(Dictionary<string, string>, MemoryStream)>>($"{UriSegments.SPEAK}?{stringedOptions}", Arg.Any<HttpContent>(), keys).Returns(expectedResponse);
 
    //    // Act
    //    var result = await speakClient.Stream(source, speakSchema);

    //    // Assert
    //    await speakClient.Received().PostFileAsync<(Dictionary<string, string>, MemoryStream)>($"{UriSegments.SPEAK}?{stringedOptions}", Arg.Any<HttpContent>(), keys);
    //    using (new AssertionScope())
    //    {
    //        result.Should().NotBeNull();
    //        // result.Should().BeAssignableTo<SyncResponse>();
    //        // result.Should().BeEquivalentTo(expectedResponse);
    //    }
    //}

    [Test]
    public async Task StreamCallBack_With_CallBack_Property_Should_Call_PostAsync_Returning_SyncResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var speakSchema = new AutoFaker<SpeakSchema>().Generate();
        var source = new TextSource("Hello World!");
        var stringedOptions = QueryParameterUtil.GetParameters(speakSchema);
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var speakClient = Substitute.For<SpeakClient>(_apiKey, _options);
        
        speakClient.When(x => x.PostAsync<AsyncResponse>(Arg.Any<string>(), Arg.Any<HttpContent>())).DoNotCallBase();
        speakClient.PostAsync<AsyncResponse>($"{UriSegments.SPEAK}?{stringedOptions}", Arg.Any<HttpContent>()).Returns(expectedResponse);

        // Act
        var result = await speakClient.StreamCallBack(source, null, speakSchema);

        // Assert
        await speakClient.Received().PostAsync<AsyncResponse>($"{UriSegments.SPEAK}?{stringedOptions}", Arg.Any<HttpContent>());
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
        //Arrange 
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var speakSchema = new AutoFaker<SpeakSchema>().Generate();
        // speakSchema is not null here as we first need to get the querystring with the callBack included
        var stringedOptions = QueryParameterUtil.GetParameters(speakSchema);
        var source = new TextSource("Hello World!");
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var speakClient = Substitute.For<SpeakClient>(_apiKey, _options);
        
        speakClient.When(x => x.PostAsync<AsyncResponse>(Arg.Any<string>(), Arg.Any<HttpContent>())).DoNotCallBase();
        speakClient.PostAsync<AsyncResponse>($"{UriSegments.SPEAK}?{stringedOptions}", Arg.Any<HttpContent>()).Returns(expectedResponse);

        var callBack = speakSchema.CallBack;

        //before we act to test this call with the callBack parameter and not the callBack property we need to null the callBack property
        speakSchema.CallBack = null;

        // Act
        var result = await speakClient.StreamCallBack(source, callBack, speakSchema);

        // Assert
        await speakClient.Received().PostAsync<AsyncResponse>($"{UriSegments.SPEAK}?{stringedOptions}", Arg.Any<HttpContent>());
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
        //Arrange 
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var speakSchema = new AutoFaker<SpeakSchema>().Generate();
        var stringedOptions = QueryParameterUtil.GetParameters(speakSchema);
        var source = new TextSource("Hello World!");
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var speakClient = Substitute.For<SpeakClient>(_apiKey, _options);
        
        speakClient.When(x => x.PostAsync<AsyncResponse>(Arg.Any<string>(), Arg.Any<HttpContent>())).DoNotCallBase();
        speakClient.PostAsync<AsyncResponse>($"{UriSegments.SPEAK}?{stringedOptions}", Arg.Any<HttpContent>()).Returns(expectedResponse);
        var callBack = speakSchema.CallBack;


        // Act  Assert
        await speakClient.Invoking(y => y.StreamCallBack(source, callBack, speakSchema))
           .Should().ThrowAsync<ArgumentException>();

        await speakClient.DidNotReceive().PostAsync<AsyncResponse>($"{UriSegments.SPEAK}?{stringedOptions}", Arg.Any<StringContent>());
    }

    [Test]
    public async Task StreamCallBack_Should_Throw_ArgumentException_With_No_CallBack_Set()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var speakSchema = new AutoFaker<SpeakSchema>().Generate();
        var source = new TextSource("Hello World!");
        // speakSchema is not null here as we first need to get the querystring with the callBack included
        var stringedOptions = QueryParameterUtil.GetParameters(speakSchema);

        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var speakClient = Substitute.For<SpeakClient>(_apiKey, _options);
        
        speakClient.When(x => x.PostAsync<AsyncResponse>(Arg.Any<string>(), Arg.Any<HttpContent>())).DoNotCallBase();
        speakClient.PostAsync<AsyncResponse>($"{UriSegments.SPEAK}?{stringedOptions}", Arg.Any<HttpContent>()).Returns(expectedResponse);
        speakSchema.CallBack = null;

        // Act  Assert
        await speakClient.Invoking(y => y.StreamCallBack(source, null, speakSchema))
           .Should().ThrowAsync<ArgumentException>();

        await speakClient.DidNotReceive().PostAsync<AsyncResponse>($"{UriSegments.SPEAK}?{stringedOptions}", Arg.Any<StringContent>());
    }
}
