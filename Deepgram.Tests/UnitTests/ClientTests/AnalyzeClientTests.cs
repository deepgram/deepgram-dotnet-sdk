// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Encapsulations;
using Deepgram.Models.Analyze.v1;
using Deepgram.Models.Authenticate.v1;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class AnalyzeClientTests
{
    DeepgramClientOptions _options;
    string _apiKey;

    [SetUp]
    public void Setup()
    {
        _options = new DeepgramClientOptions();
        _apiKey = new Faker().Random.Guid().ToString();
    }

    [Test]
    public async Task AnalyzeUrl_Should_Call_PostAsync_Returning_SyncResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<SyncResponse>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();
        analyzeSchema.CallBack = null;
        var stringedOptions = QueryParameterUtil.GetParameters(analyzeSchema);
        var source = new AutoFaker<UrlSource>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options);
        analyzeClient._httpClientWrapper = new HttpClientWrapper(httpClient);

        analyzeClient.When(x => x.PostAsync<SyncResponse>(Arg.Any<string>(), Arg.Any<StringContent>())).DoNotCallBase();
        analyzeClient.PostAsync<SyncResponse>($"{UriSegments.ANALYZE}?{stringedOptions}", Arg.Any<StringContent>()).Returns(expectedResponse);

        // Act
        var result = await analyzeClient.AnalyzeUrl(source, analyzeSchema);

        // Assert
        await analyzeClient.Received().PostAsync<SyncResponse>($"{UriSegments.ANALYZE}?{stringedOptions}", Arg.Any<StringContent>());

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<SyncResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task AnalyzeUrl_Should_Throw_ArgumentException_If_CallBack_Not_Null()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<SyncResponse>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();
        var stringedOptions = QueryParameterUtil.GetParameters(analyzeSchema);
        var source = new AutoFaker<UrlSource>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options);
        analyzeClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        analyzeClient.When(x => x.PostAsync<SyncResponse>(Arg.Any<string>(), Arg.Any<StringContent>())).DoNotCallBase();
        analyzeClient.PostAsync<SyncResponse>($"{UriSegments.ANALYZE}?{stringedOptions}", Arg.Any<StringContent>()).Returns(expectedResponse);

        // Act and Assert
        await analyzeClient.Invoking(y => y.AnalyzeUrl(source, analyzeSchema))
            .Should().ThrowAsync<ArgumentException>();

        await analyzeClient.DidNotReceive().PostAsync<SyncResponse>($"{UriSegments.ANALYZE}?{stringedOptions}", Arg.Any<StringContent>());
    }

    [Test]
    public async Task AnalyzeUrlCallBack_Should_Call_PostAsync_Returning_SyncResponse_With_CallBack_Parameter()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var source = new AutoFaker<UrlSource>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();
        // analyzeSchema is not null here as we first need to get the querystring with the callBack included
        var stringedQuery = QueryParameterUtil.GetParameters(analyzeSchema);
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options);
        analyzeClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        analyzeClient.When(x => x.PostAsync<AsyncResponse>(Arg.Any<string>(), Arg.Any<StringContent>())).DoNotCallBase();
        analyzeClient.PostAsync<AsyncResponse>($"{UriSegments.ANALYZE}?{stringedQuery}", Arg.Any<StringContent>()).Returns(expectedResponse);
        var callBackParameter = analyzeSchema.CallBack;
        //before we act to test this call with the callBack parameter and not the callBack property we need to null the callBack property
        analyzeSchema.CallBack = null;


        // Act
        var result = await analyzeClient.AnalyzeUrlCallBack(source, callBackParameter, analyzeSchema);

        // Assert
        await analyzeClient.Received().PostAsync<AsyncResponse>($"{UriSegments.ANALYZE}?{stringedQuery}", Arg.Any<StringContent>());
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<AsyncResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task AnalyzeUrlCallBack_Should_Call_PostAsync_Returning_SyncResponse_With_CallBack_Property()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var source = new AutoFaker<UrlSource>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();
        var stringedQuery = QueryParameterUtil.GetParameters(analyzeSchema);
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options);
        analyzeClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        analyzeClient.When(x => x.PostAsync<AsyncResponse>(Arg.Any<string>(), Arg.Any<StringContent>())).DoNotCallBase();
        analyzeClient.PostAsync<AsyncResponse>($"{UriSegments.ANALYZE}?{stringedQuery}", Arg.Any<StringContent>()).Returns(expectedResponse);

        // Act
        var result = await analyzeClient.AnalyzeUrlCallBack(source, null, analyzeSchema);

        // Assert
        await analyzeClient.Received().PostAsync<AsyncResponse>($"{UriSegments.ANALYZE}?{stringedQuery}", Arg.Any<StringContent>());
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<AsyncResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task AnalyzeUrlCallBack_Should_Throw_ArgumentException_With_CallBack_Property_And_CallBack_Parameter_Set()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var source = new AutoFaker<UrlSource>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();
        var stringedQuery = QueryParameterUtil.GetParameters(analyzeSchema);

        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options);
        analyzeClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        analyzeClient.When(x => x.PostAsync<AsyncResponse>(Arg.Any<string>(), Arg.Any<StringContent>())).DoNotCallBase();
        analyzeClient.PostAsync<AsyncResponse>($"{UriSegments.ANALYZE}?{stringedQuery}", Arg.Any<StringContent>()).Returns(expectedResponse);
        var callBackParameter = analyzeSchema.CallBack;

        // Act Assert
        await analyzeClient.Invoking(y => y.AnalyzeUrlCallBack(source, callBackParameter, analyzeSchema))
            .Should().ThrowAsync<ArgumentException>();

        await analyzeClient.DidNotReceive().PostAsync<SyncResponse>($"{UriSegments.ANALYZE}?{stringedQuery}", Arg.Any<StringContent>());
    }

    [Test]
    public async Task AnalyzeUrlCallBack_Should_Throw_ArgumentException_With_No_CallBack_Set()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var source = new AutoFaker<UrlSource>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();
        analyzeSchema.CallBack = null;
        var stringedQuery = QueryParameterUtil.GetParameters(analyzeSchema);

        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options);
        analyzeClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        analyzeClient.When(x => x.PostAsync<AsyncResponse>(Arg.Any<string>(), Arg.Any<StringContent>())).DoNotCallBase();
        analyzeClient.PostAsync<AsyncResponse>($"{UriSegments.ANALYZE}?{stringedQuery}", Arg.Any<StringContent>()).Returns(expectedResponse);


        // Act Assert
        await analyzeClient.Invoking(y => y.AnalyzeUrlCallBack(source, null, analyzeSchema))
            .Should().ThrowAsync<ArgumentException>();

        await analyzeClient.DidNotReceive().PostAsync<SyncResponse>($"{UriSegments.ANALYZE}?{stringedQuery}", Arg.Any<StringContent>());
    }

    [Test]
    public async Task AnalyzeFile_With_Stream_Should_Call_PostAsync_Returning_SyncResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<SyncResponse>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();
        analyzeSchema.CallBack = null;
        var stringedOptions = QueryParameterUtil.GetParameters(analyzeSchema);
        var source = GetFakeStream(GetFakeByteArray());
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options);
        analyzeClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        analyzeClient.When(x => x.PostAsync<SyncResponse>(Arg.Any<string>(), Arg.Any<HttpContent>())).DoNotCallBase();
        analyzeClient.PostAsync<SyncResponse>($"{UriSegments.ANALYZE}?{stringedOptions}", Arg.Any<HttpContent>()).Returns(expectedResponse);

        // Act
        var result = await analyzeClient.AnalyzeFile(source, analyzeSchema);

        // Assert
        await analyzeClient.Received().PostAsync<SyncResponse>($"{UriSegments.ANALYZE}?{stringedOptions}", Arg.Any<HttpContent>());
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<SyncResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task AnalyzeFile_With_Bytes_Should_Call_PostAsync_Returning_SyncResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<SyncResponse>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();
        analyzeSchema.CallBack = null;
        var stringedOptions = QueryParameterUtil.GetParameters(analyzeSchema);
        var source = GetFakeByteArray();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options);
        analyzeClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        analyzeClient.When(x => x.PostAsync<SyncResponse>(Arg.Any<string>(), Arg.Any<HttpContent>())).DoNotCallBase();
        analyzeClient.PostAsync<SyncResponse>($"{UriSegments.ANALYZE}?{stringedOptions}", Arg.Any<HttpContent>()).Returns(expectedResponse);

        // Act
        var result = await analyzeClient.AnalyzeFile(source, analyzeSchema);

        // Assert
        await analyzeClient.Received().PostAsync<SyncResponse>($"{UriSegments.ANALYZE}?{stringedOptions}", Arg.Any<HttpContent>());
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<SyncResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task AnalyzeFileCallBack_With_Stream_With_CallBack_Property_Should_Call_PostAsync_Returning_SyncResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();
        var stringedOptions = QueryParameterUtil.GetParameters(analyzeSchema);
        var source = GetFakeStream(GetFakeByteArray());
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options);
        analyzeClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        analyzeClient.When(x => x.PostAsync<AsyncResponse>(Arg.Any<string>(), Arg.Any<HttpContent>())).DoNotCallBase();
        analyzeClient.PostAsync<AsyncResponse>($"{UriSegments.ANALYZE}?{stringedOptions}", Arg.Any<HttpContent>()).Returns(expectedResponse);

        // Act
        var result = await analyzeClient.AnalyzeFileCallBack(source, null, analyzeSchema);

        // Assert
        await analyzeClient.Received().PostAsync<AsyncResponse>($"{UriSegments.ANALYZE}?{stringedOptions}", Arg.Any<HttpContent>());
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<AsyncResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task AnalyzeFileCallBack_With_Bytes_With_CallBack_Property_Should_Call_PostAsync_Returning_SyncResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();
        var stringedOptions = QueryParameterUtil.GetParameters(analyzeSchema);
        var source = GetFakeByteArray();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options);
        analyzeClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        analyzeClient.When(x => x.PostAsync<AsyncResponse>(Arg.Any<string>(), Arg.Any<HttpContent>())).DoNotCallBase();
        analyzeClient.PostAsync<AsyncResponse>($"{UriSegments.ANALYZE}?{stringedOptions}", Arg.Any<HttpContent>()).Returns(expectedResponse);

        // Act
        var result = await analyzeClient.AnalyzeFileCallBack(source, null, analyzeSchema);

        // Assert
        await analyzeClient.Received().PostAsync<AsyncResponse>($"{UriSegments.ANALYZE}?{stringedOptions}", Arg.Any<HttpContent>());

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<AsyncResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task AnalyzeFileCallBack_With_Stream_With_CallBack_Parameter_Should_Call_PostAsync_Returning_SyncResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();
        // analyzeSchema is not null here as we first need to get the querystring with the callBack included
        var stringedOptions = QueryParameterUtil.GetParameters(analyzeSchema);
        var source = GetFakeStream(GetFakeByteArray());
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options);
        analyzeClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        analyzeClient.When(x => x.PostAsync<AsyncResponse>(Arg.Any<string>(), Arg.Any<HttpContent>())).DoNotCallBase();
        analyzeClient.PostAsync<AsyncResponse>($"{UriSegments.ANALYZE}?{stringedOptions}", Arg.Any<HttpContent>()).Returns(expectedResponse);

        var callBack = analyzeSchema.CallBack;

        //before we act to test this call with the callBack parameter and not the callBack property we need to null the callBack property
        analyzeSchema.CallBack = null;

        // Act
        var result = await analyzeClient.AnalyzeFileCallBack(source, callBack, analyzeSchema);

        // Assert
        await analyzeClient.Received().PostAsync<AsyncResponse>($"{UriSegments.ANALYZE}?{stringedOptions}", Arg.Any<HttpContent>());
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<AsyncResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task AnalyzeFileCallBack_With_Bytes_With_CallBack_Parameter_Should_Call_PostAsync_Returning_SyncResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();
        // analyzeSchema is not null here as we first need to get the querystring with the callBack included
        var stringedOptions = QueryParameterUtil.GetParameters(analyzeSchema);
        var source = GetFakeByteArray();

        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options);
        analyzeClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        analyzeClient.When(x => x.PostAsync<AsyncResponse>(Arg.Any<string>(), Arg.Any<HttpContent>())).DoNotCallBase();
        analyzeClient.PostAsync<AsyncResponse>($"{UriSegments.ANALYZE}?{stringedOptions}", Arg.Any<HttpContent>()).Returns(expectedResponse);

        var callBack = analyzeSchema.CallBack;

        //before we act to test this call with the callBack parameter and not the callBack property we need to null the callBack property
        analyzeSchema.CallBack = null;

        // Act
        var result = await analyzeClient.AnalyzeFileCallBack(source, callBack, analyzeSchema);

        // Assert
        await analyzeClient.Received().PostAsync<AsyncResponse>($"{UriSegments.ANALYZE}?{stringedOptions}", Arg.Any<HttpContent>());
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<AsyncResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task AnalyzeFileCallBack_With_Stream_Throw_ArgumentException_With_CallBack_Property_And_CallBack_Parameter_Set()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();
        var stringedOptions = QueryParameterUtil.GetParameters(analyzeSchema);
        var source = GetFakeStream(GetFakeByteArray());
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options);
        analyzeClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        analyzeClient.When(x => x.PostAsync<AsyncResponse>(Arg.Any<string>(), Arg.Any<HttpContent>())).DoNotCallBase();
        analyzeClient.PostAsync<AsyncResponse>($"{UriSegments.ANALYZE}?{stringedOptions}", Arg.Any<HttpContent>()).Returns(expectedResponse);
        var callBack = analyzeSchema.CallBack;


        // Act  Assert
        await analyzeClient.Invoking(y => y.AnalyzeFileCallBack(source, callBack, analyzeSchema))
           .Should().ThrowAsync<ArgumentException>();

        await analyzeClient.DidNotReceive().PostAsync<AsyncResponse>($"{UriSegments.ANALYZE}?{stringedOptions}", Arg.Any<StringContent>());


    }

    [Test]
    public async Task AnalyzeFileCallBack_With_Bytes_Should_Throw_ArgumentException_With_No_CallBack_Set()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();
        // analyzeSchema is not null here as we first need to get the querystring with the callBack included
        var stringedOptions = QueryParameterUtil.GetParameters(analyzeSchema);
        var source = GetFakeByteArray();

        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options);
        analyzeClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        analyzeClient.When(x => x.PostAsync<AsyncResponse>(Arg.Any<string>(), Arg.Any<HttpContent>())).DoNotCallBase();
        analyzeClient.PostAsync<AsyncResponse>($"{UriSegments.ANALYZE}?{stringedOptions}", Arg.Any<HttpContent>()).Returns(expectedResponse);
        analyzeSchema.CallBack = null;

        // Act  Assert
        await analyzeClient.Invoking(y => y.AnalyzeFileCallBack(source, null, analyzeSchema))
           .Should().ThrowAsync<ArgumentException>();

        await analyzeClient.DidNotReceive().PostAsync<AsyncResponse>($"{UriSegments.ANALYZE}?{stringedOptions}", Arg.Any<StringContent>());
    }

    private static Stream GetFakeStream(byte[] source)
    {
        var stream = new MemoryStream();
        stream.Write(source, 0, source.Length);
        return stream;
    }

    private static byte[] GetFakeByteArray() => new Faker().Random.Bytes(10);

}
