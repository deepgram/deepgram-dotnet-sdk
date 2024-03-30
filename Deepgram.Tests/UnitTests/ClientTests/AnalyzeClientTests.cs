// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;
using Deepgram.Clients.Analyze.v1;
using Deepgram.Models.Analyze.v1;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class AnalyzeClientTests
{
    DeepgramHttpClientOptions _options;
    string _apiKey;

    [SetUp]
    public void Setup()
    {
        _apiKey = new Faker().Random.Guid().ToString();
        _options = new DeepgramHttpClientOptions(_apiKey, null, null, true);
    }

    [Test]
    public async Task AnalyzeUrl_Should_Call_PostAsync_Returning_SyncResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.READ}");
        var expectedResponse = new AutoFaker<SyncResponse>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();
        analyzeSchema.CallBack = null;
        var source = new AutoFaker<UrlSource>().Generate();

        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options, null);

        // Mock methods
        analyzeClient.When(x => x.PostAsync<UrlSource, AnalyzeSchema, SyncResponse>(Arg.Any<string>(), Arg.Any<AnalyzeSchema>(), Arg.Any<UrlSource>())).DoNotCallBase();
        analyzeClient.PostAsync<UrlSource, AnalyzeSchema, SyncResponse>(url, Arg.Any<AnalyzeSchema>(), Arg.Any<UrlSource>()).Returns(expectedResponse);

        // Act
        var result = await analyzeClient.AnalyzeUrl(source, analyzeSchema);

        // Assert
        await analyzeClient.Received().PostAsync<UrlSource, AnalyzeSchema, SyncResponse>(url, Arg.Any<AnalyzeSchema>(), Arg.Any<UrlSource>());

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
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.READ}");
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();
        var source = new AutoFaker<UrlSource>().Generate();

        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options, null);
        
        // Mock methods
        analyzeClient.When(x => x.PostAsync<AnalyzeSchema, AsyncResponse>(Arg.Any<string>(), Arg.Any<AnalyzeSchema>())).DoNotCallBase();
        analyzeClient.PostAsync<AnalyzeSchema, AsyncResponse>(url, Arg.Any<AnalyzeSchema>()).Returns(expectedResponse);

        // Act and Assert
        await analyzeClient.Invoking(y => y.AnalyzeUrl(source, analyzeSchema))
            .Should().ThrowAsync<ArgumentException>();

        await analyzeClient.DidNotReceive().PostAsync<AnalyzeSchema, SyncResponse>(url, Arg.Any<AnalyzeSchema>());
    }

    [Test]
    public async Task AnalyzeUrlCallBack_Should_Call_PostAsync_Returning_SyncResponse_With_CallBack_Parameter()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.READ}");
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var source = new AutoFaker<UrlSource>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();
        // analyzeSchema is not null here as we first need to get the querystring with the callBack included
        
        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options, null);
        
        // Mock methods
        analyzeClient.When(x => x.PostAsync<UrlSource, AnalyzeSchema, AsyncResponse>(Arg.Any<string>(), Arg.Any<AnalyzeSchema>(), Arg.Any<UrlSource>())).DoNotCallBase();
        analyzeClient.PostAsync<UrlSource, AnalyzeSchema, AsyncResponse>(url, Arg.Any<AnalyzeSchema>(), Arg.Any<UrlSource>()).Returns(expectedResponse);

        //before we act to test this call with the callBack parameter and not the callBack property we need to null the callBack property
        var callBackParameter = analyzeSchema.CallBack;
        analyzeSchema.CallBack = null;

        // Act
        var result = await analyzeClient.AnalyzeUrlCallBack(source, callBackParameter, analyzeSchema);

        // Assert
        await analyzeClient.Received().PostAsync<UrlSource, AnalyzeSchema, AsyncResponse>(url, Arg.Any<AnalyzeSchema>(), Arg.Any<UrlSource>());
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
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.READ}");
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var source = new AutoFaker<UrlSource>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();
        
        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options, null);
        
        // Mock methods
        analyzeClient.When(x => x.PostAsync<UrlSource, AnalyzeSchema, AsyncResponse>(Arg.Any<string>(), Arg.Any<AnalyzeSchema>(), Arg.Any<UrlSource>())).DoNotCallBase();
        analyzeClient.PostAsync<UrlSource, AnalyzeSchema, AsyncResponse>(url, Arg.Any<AnalyzeSchema>(), Arg.Any<UrlSource>()).Returns(expectedResponse);

        // Act
        var result = await analyzeClient.AnalyzeUrlCallBack(source, null, analyzeSchema);

        // Assert
        await analyzeClient.Received().PostAsync<UrlSource, AnalyzeSchema, AsyncResponse>(url, Arg.Any<AnalyzeSchema>(), Arg.Any<UrlSource>());
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
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.READ}");
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var source = new AutoFaker<UrlSource>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();
        
        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options, null);
        
        // Mock methods
        analyzeClient.When(x => x.PostAsync<AnalyzeSchema, AsyncResponse>(Arg.Any<string>(), Arg.Any<AnalyzeSchema>())).DoNotCallBase();
        analyzeClient.PostAsync<AnalyzeSchema, AsyncResponse>(url, Arg.Any<AnalyzeSchema>()).Returns(expectedResponse);
        var callBackParameter = analyzeSchema.CallBack;

        // Act and Assert
        await analyzeClient.Invoking(y => y.AnalyzeUrlCallBack(source, callBackParameter, analyzeSchema))
            .Should().ThrowAsync<ArgumentException>();

        await analyzeClient.DidNotReceive().PostAsync<AnalyzeSchema, AsyncResponse>(url, Arg.Any<AnalyzeSchema>());
    }

    [Test]
    public async Task AnalyzeUrlCallBack_Should_Throw_ArgumentException_With_No_CallBack_Set()
    {
        // Input and Output 
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.READ}");
        var expectedResponse = new AutoFaker<SyncResponse>().Generate();
        var source = new AutoFaker<UrlSource>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();
        analyzeSchema.CallBack = null;
        
        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options, null);
        
        // Mock methods
        analyzeClient.When(x => x.PostAsync<AnalyzeSchema, SyncResponse>(Arg.Any<string>(), Arg.Any<AnalyzeSchema>())).DoNotCallBase();
        analyzeClient.PostAsync<AnalyzeSchema, SyncResponse>(url, Arg.Any<AnalyzeSchema>()).Returns(expectedResponse);

        // Act and Assert
        await analyzeClient.Invoking(y => y.AnalyzeUrlCallBack(source, null, analyzeSchema))
            .Should().ThrowAsync<ArgumentException>();

        await analyzeClient.DidNotReceive().PostAsync<AnalyzeSchema, SyncResponse>(url, Arg.Any<AnalyzeSchema>());
    }

    [Test]
    public async Task AnalyzeFile_With_Stream_Should_Call_PostAsync_Returning_SyncResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.READ}");
        var expectedResponse = new AutoFaker<SyncResponse>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();
        analyzeSchema.CallBack = null;
        var source = GetFakeStream(GetFakeByteArray());

        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options, null);
        
        // Mock methods
        analyzeClient.When(x => x.PostAsync<Stream, AnalyzeSchema, SyncResponse>(Arg.Any<string>(), Arg.Any<AnalyzeSchema>(), Arg.Any<Stream>())).DoNotCallBase();
        analyzeClient.PostAsync<Stream, AnalyzeSchema, SyncResponse>(url, Arg.Any<AnalyzeSchema>(), Arg.Any<Stream>()).Returns(expectedResponse);

        // Act
        var result = await analyzeClient.AnalyzeFile(source, analyzeSchema);

        // Assert
        await analyzeClient.Received().PostAsync<Stream, AnalyzeSchema, SyncResponse>(url, Arg.Any<AnalyzeSchema>(), Arg.Any<Stream>());
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
        // Input and Output 
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.READ}");
        var expectedResponse = new AutoFaker<SyncResponse>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();
        analyzeSchema.CallBack = null;
        var source = GetFakeByteArray();

        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options, null);
        
        // Mock methods
        analyzeClient.When(x => x.PostAsync<Stream, AnalyzeSchema, SyncResponse>(Arg.Any<string>(), Arg.Any<AnalyzeSchema>(), Arg.Any<Stream>())).DoNotCallBase();
        analyzeClient.PostAsync<Stream, AnalyzeSchema, SyncResponse>(url, Arg.Any<AnalyzeSchema>(), Arg.Any<Stream>()).Returns(expectedResponse);

        // Act
        var result = await analyzeClient.AnalyzeFile(source, analyzeSchema);

        // Assert
        await analyzeClient.Received().PostAsync<Stream, AnalyzeSchema, SyncResponse>(url, Arg.Any<AnalyzeSchema>(), Arg.Any<Stream>());
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
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.READ}");
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();
        var source = GetFakeStream(GetFakeByteArray());

        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options, null);
        
        // Mock methods
        analyzeClient.When(x => x.PostAsync<Stream, AnalyzeSchema, AsyncResponse>(Arg.Any<string>(), Arg.Any<AnalyzeSchema>(), Arg.Any<Stream>())).DoNotCallBase();
        analyzeClient.PostAsync<Stream, AnalyzeSchema, AsyncResponse>(url, Arg.Any<AnalyzeSchema>(), Arg.Any<Stream>()).Returns(expectedResponse);

        // Act
        var result = await analyzeClient.AnalyzeFileCallBack(source, null, analyzeSchema);

        // Assert
        await analyzeClient.Received().PostAsync<Stream, AnalyzeSchema, AsyncResponse>(url, Arg.Any<AnalyzeSchema>(), Arg.Any<Stream>());
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
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.READ}");
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();
        var source = GetFakeByteArray();

        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options, null);
        
        // Mock methods
        analyzeClient.When(x => x.PostAsync<Stream, AnalyzeSchema, AsyncResponse>(Arg.Any<string>(), Arg.Any<AnalyzeSchema>(), Arg.Any<Stream>())).DoNotCallBase();
        analyzeClient.PostAsync<Stream, AnalyzeSchema, AsyncResponse>(url, Arg.Any<AnalyzeSchema>(), Arg.Any<Stream>()).Returns(expectedResponse);

        // Act
        var result = await analyzeClient.AnalyzeFileCallBack(source, null, analyzeSchema);

        // Assert
        await analyzeClient.Received().PostAsync<Stream, AnalyzeSchema, AsyncResponse>(url, Arg.Any<AnalyzeSchema>(), Arg.Any<Stream>());

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
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.READ}");
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();
        var source = GetFakeStream(GetFakeByteArray());

        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options, null);
        
        // Mock methods
        analyzeClient.When(x => x.PostAsync<Stream, AnalyzeSchema, AsyncResponse>(Arg.Any<string>(), Arg.Any<AnalyzeSchema>(), Arg.Any<Stream>())).DoNotCallBase();
        analyzeClient.PostAsync<Stream, AnalyzeSchema, AsyncResponse>(url, Arg.Any<AnalyzeSchema>(), Arg.Any<Stream>()).Returns(expectedResponse);

        //before we act to test this call with the callBack parameter and not the callBack property we need to null the callBack property
        var callBack = analyzeSchema.CallBack;
        analyzeSchema.CallBack = null;

        // Act
        var result = await analyzeClient.AnalyzeFileCallBack(source, callBack, analyzeSchema);

        // Assert
        await analyzeClient.Received().PostAsync<Stream, AnalyzeSchema, AsyncResponse>(url, Arg.Any<AnalyzeSchema>(), Arg.Any<Stream>());
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
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.READ}");
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();
        var source = GetFakeByteArray();

        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options, null);
        
        // Mock methods
        analyzeClient.When(x => x.PostAsync<Stream, AnalyzeSchema, AsyncResponse>(Arg.Any<string>(), Arg.Any<AnalyzeSchema>(), Arg.Any<Stream>())).DoNotCallBase();
        analyzeClient.PostAsync<Stream, AnalyzeSchema, AsyncResponse>(url, Arg.Any<AnalyzeSchema>(), Arg.Any<Stream>()).Returns(expectedResponse);

        //before we act to test this call with the callBack parameter and not the callBack property we need to null the callBack property
        var callBack = analyzeSchema.CallBack;
        analyzeSchema.CallBack = null;

        // Act
        var result = await analyzeClient.AnalyzeFileCallBack(source, callBack, analyzeSchema);

        // Assert
        await analyzeClient.Received().PostAsync<Stream, AnalyzeSchema, AsyncResponse>(url, Arg.Any<AnalyzeSchema>(), Arg.Any<Stream>());
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
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.READ}");
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();
        var source = GetFakeStream(GetFakeByteArray());

        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options, null);
        
        // Mock methods
        analyzeClient.When(x => x.PostAsync<Stream, AnalyzeSchema, AsyncResponse>(Arg.Any<string>(), Arg.Any<AnalyzeSchema>(), Arg.Any<Stream>())).DoNotCallBase();
        analyzeClient.PostAsync<Stream, AnalyzeSchema, AsyncResponse>(url, Arg.Any<AnalyzeSchema>(), Arg.Any<Stream>()).Returns(expectedResponse);
        
        var callBack = analyzeSchema.CallBack;

        // Act and Assert
        await analyzeClient.Invoking(y => y.AnalyzeFileCallBack(source, callBack, analyzeSchema))
           .Should().ThrowAsync<ArgumentException>();

        await analyzeClient.DidNotReceive().PostAsync<Stream, AnalyzeSchema, AsyncResponse>(url, Arg.Any<AnalyzeSchema>(), Arg.Any<Stream>());
    }

    [Test]
    public async Task AnalyzeFileCallBack_With_Bytes_Should_Throw_ArgumentException_With_No_CallBack_Set()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.READ}");
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var analyzeSchema = new AutoFaker<AnalyzeSchema>().Generate();        
        var source = GetFakeByteArray();

        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var analyzeClient = Substitute.For<AnalyzeClient>(_apiKey, _options, null);
        
        // Mock methods
        analyzeClient.When(x => x.PostAsync<Stream, AnalyzeSchema, AsyncResponse>(Arg.Any<string>(), Arg.Any<AnalyzeSchema>(), Arg.Any<Stream>())).DoNotCallBase();
        analyzeClient.PostAsync<Stream, AnalyzeSchema, AsyncResponse>(url, Arg.Any<AnalyzeSchema>(), Arg.Any<Stream>()).Returns(expectedResponse);

        analyzeSchema.CallBack = null;

        // Act and Assert
        await analyzeClient.Invoking(y => y.AnalyzeFileCallBack(source, null, analyzeSchema))
           .Should().ThrowAsync<ArgumentException>();

        await analyzeClient.DidNotReceive().PostAsync<Stream, AnalyzeSchema, AsyncResponse>(url, Arg.Any<AnalyzeSchema>(), Arg.Any<Stream>());
    }

    private static Stream GetFakeStream(byte[] source)
    {
        var stream = new MemoryStream();
        stream.Write(source, 0, source.Length);
        return stream;
    }

    private static byte[] GetFakeByteArray() => new Faker().Random.Bytes(10);

}
