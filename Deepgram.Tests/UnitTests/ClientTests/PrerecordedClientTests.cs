// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;
using Deepgram.Clients.PreRecorded.v1;
using Deepgram.Models.PreRecorded.v1;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class PreRecordedClientTests
{
    DeepgramHttpClientOptions _options;
    string _apiKey;

    [SetUp]
    public void Setup()
    {
        _apiKey = new Faker().Random.Guid().ToString();
        _options = new DeepgramHttpClientOptions(_apiKey)
        {
            OnPrem = true,
        };
    }

    [Test]
    public async Task TranscribeUrl_Should_Call_PostAsync_Returning_SyncResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.LISTEN}");
        var expectedResponse = new AutoFaker<SyncResponse>().Generate();
        var prerecordedSchema = new AutoFaker<PreRecordedSchema>().Generate();
        prerecordedSchema.CallBack = null;
        var source = new AutoFaker<UrlSource>().Generate();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var prerecordedClient = Substitute.For<PreRecordedClient>(_apiKey, _options, null);
        
        // Mock Methods
        prerecordedClient.When(x => x.PostAsync<UrlSource, PreRecordedSchema, SyncResponse>(Arg.Any<string>(), Arg.Any<PreRecordedSchema>(), Arg.Any<UrlSource>())).DoNotCallBase();
        prerecordedClient.PostAsync<UrlSource, PreRecordedSchema, SyncResponse>(url, Arg.Any<PreRecordedSchema>(), Arg.Any<UrlSource>()).Returns(expectedResponse);

        // Act
        var result = await prerecordedClient.TranscribeUrl(source, prerecordedSchema);

        // Assert
        await prerecordedClient.Received().PostAsync<UrlSource, PreRecordedSchema, SyncResponse>(url, Arg.Any<PreRecordedSchema>(), Arg.Any<UrlSource>());

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<SyncResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task TranscribeUrl_Should_Throw_ArgumentException_If_CallBack_Not_Null()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.LISTEN}");
        var expectedResponse = new AutoFaker<SyncResponse>().Generate();
        var prerecordedSchema = new AutoFaker<PreRecordedSchema>().Generate();
        var source = new AutoFaker<UrlSource>().Generate();
        
        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var prerecordedClient = Substitute.For<PreRecordedClient>(_apiKey, _options, null);
        
        // Mock Methods
        prerecordedClient.When(x => x.PostAsync<UrlSource, PreRecordedSchema, SyncResponse>(Arg.Any<string>(), Arg.Any<PreRecordedSchema>(), Arg.Any<UrlSource>())).DoNotCallBase();
        prerecordedClient.PostAsync<UrlSource, PreRecordedSchema, SyncResponse>(url, Arg.Any<PreRecordedSchema>(), Arg.Any<UrlSource>()).Returns(expectedResponse);

        // Act and Assert
        await prerecordedClient.Invoking(y => y.TranscribeUrl(source, prerecordedSchema))
            .Should().ThrowAsync<ArgumentException>();

        await prerecordedClient.DidNotReceive().PostAsync<UrlSource, PreRecordedSchema, SyncResponse>(url, Arg.Any<PreRecordedSchema>(), Arg.Any<UrlSource>());
    }

    [Test]
    public async Task TranscribeUrlCallBack_Should_Call_PostAsync_Returning_SyncResponse_With_CallBack_Parameter()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.LISTEN}");
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var source = new AutoFaker<UrlSource>().Generate();
        var prerecordedSchema = new AutoFaker<PreRecordedSchema>().Generate();
        
        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var prerecordedClient = Substitute.For<PreRecordedClient>(_apiKey, _options, null);
        
        // Mock Methods
        prerecordedClient.When(x => x.PostAsync<UrlSource, PreRecordedSchema, AsyncResponse>(Arg.Any<string>(), Arg.Any<PreRecordedSchema>(), Arg.Any<UrlSource>())).DoNotCallBase();
        prerecordedClient.PostAsync<UrlSource, PreRecordedSchema, AsyncResponse>(url, Arg.Any<PreRecordedSchema>(), Arg.Any<UrlSource>()).Returns(expectedResponse);

        //before we act to test this call with the callBack parameter and not the callBack property we need to null the callBack property
        var callBackParameter = prerecordedSchema.CallBack;
        prerecordedSchema.CallBack = null;

        // Act
        var result = await prerecordedClient.TranscribeUrlCallBack(source, callBackParameter, prerecordedSchema);

        // Assert
        await prerecordedClient.Received().PostAsync<UrlSource, PreRecordedSchema, AsyncResponse>(url, Arg.Any<PreRecordedSchema>(), Arg.Any<UrlSource>());
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<AsyncResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task TranscribeUrlCallBack_Should_Call_PostAsync_Returning_SyncResponse_With_CallBack_Property()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.LISTEN}");
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var source = new AutoFaker<UrlSource>().Generate();
        var prerecordedSchema = new AutoFaker<PreRecordedSchema>().Generate();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var prerecordedClient = Substitute.For<PreRecordedClient>(_apiKey, _options, null);
        
        // Mock Methods
        prerecordedClient.When(x => x.PostAsync<UrlSource, PreRecordedSchema, AsyncResponse>(Arg.Any<string>(), Arg.Any<PreRecordedSchema>(), Arg.Any<UrlSource>())).DoNotCallBase();
        prerecordedClient.PostAsync<UrlSource, PreRecordedSchema, AsyncResponse>(url, Arg.Any<PreRecordedSchema>(), Arg.Any<UrlSource>()).Returns(expectedResponse);

        // Act
        var result = await prerecordedClient.TranscribeUrlCallBack(source, null, prerecordedSchema);

        // Assert
        await prerecordedClient.Received().PostAsync<UrlSource, PreRecordedSchema, AsyncResponse>(url, Arg.Any<PreRecordedSchema>(), Arg.Any<UrlSource>());
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<AsyncResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task TranscribeUrlCallBack_Should_Throw_ArgumentException_With_CallBack_Property_And_CallBack_Parameter_Set()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.LISTEN}");
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var source = new AutoFaker<UrlSource>().Generate();
        var prerecordedSchema = new AutoFaker<PreRecordedSchema>().Generate();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var prerecordedClient = Substitute.For<PreRecordedClient>(_apiKey, _options, null);
        
        // Mock Methods
        prerecordedClient.When(x => x.PostAsync<PreRecordedSchema, AsyncResponse>(Arg.Any<string>(), Arg.Any<PreRecordedSchema>())).DoNotCallBase();
        prerecordedClient.PostAsync<PreRecordedSchema, AsyncResponse>(url, Arg.Any<PreRecordedSchema>()).Returns(expectedResponse);

        var callBackParameter = prerecordedSchema.CallBack;

        // Act and Assert
        await prerecordedClient.Invoking(y => y.TranscribeUrlCallBack(source, callBackParameter, prerecordedSchema))
            .Should().ThrowAsync<ArgumentException>();

        await prerecordedClient.DidNotReceive().PostAsync<PreRecordedSchema, SyncResponse>(url, Arg.Any<PreRecordedSchema>());
    }

    [Test]
    public async Task TranscribeUrlCallBack_Should_Throw_ArgumentException_With_No_CallBack_Set()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.LISTEN}");
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var source = new AutoFaker<UrlSource>().Generate();
        var prerecordedSchema = new AutoFaker<PreRecordedSchema>().Generate();
        prerecordedSchema.CallBack = null;

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var prerecordedClient = Substitute.For<PreRecordedClient>(_apiKey, _options, null);
        
        // Mock Methods
        prerecordedClient.When(x => x.PostAsync<PreRecordedSchema, AsyncResponse>(Arg.Any<string>(), Arg.Any<PreRecordedSchema>())).DoNotCallBase();
        prerecordedClient.PostAsync<UrlSource, PreRecordedSchema, AsyncResponse>(url, Arg.Any<PreRecordedSchema>(), Arg.Any<UrlSource>()).Returns(expectedResponse);

        // Act and Assert
        await prerecordedClient.Invoking(y => y.TranscribeUrlCallBack(source, null, prerecordedSchema))
            .Should().ThrowAsync<ArgumentException>();

        await prerecordedClient.DidNotReceive().PostAsync<PreRecordedSchema, SyncResponse>(url, Arg.Any<PreRecordedSchema>());
    }

    [Test]
    public async Task TranscribeFile_With_Stream_Should_Call_PostAsync_Returning_SyncResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.LISTEN}");
        var expectedResponse = new AutoFaker<SyncResponse>().Generate();
        var prerecordedSchema = new AutoFaker<PreRecordedSchema>().Generate();
        prerecordedSchema.CallBack = null;

        // Fake Client
        var source = GetFakeStream(GetFakeByteArray());
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var prerecordedClient = Substitute.For<PreRecordedClient>(_apiKey, _options, null);

        // Mock Methods
        prerecordedClient.When(x => x.PostAsync<Stream, PreRecordedSchema, SyncResponse>(Arg.Any<string>(), Arg.Any<PreRecordedSchema>(), Arg.Any<Stream>())).DoNotCallBase();
        prerecordedClient.PostAsync<Stream, PreRecordedSchema, SyncResponse>(url, Arg.Any<PreRecordedSchema>(), Arg.Any<Stream>()).Returns(expectedResponse);

        // Act
        var result = await prerecordedClient.TranscribeFile(source, prerecordedSchema);

        // Assert
        await prerecordedClient.Received().PostAsync<Stream, PreRecordedSchema, SyncResponse>(url, Arg.Any<PreRecordedSchema>(), Arg.Any<Stream>());
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<SyncResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task TranscribeFile_With_Bytes_Should_Call_PostAsync_Returning_SyncResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.LISTEN}");
        var expectedResponse = new AutoFaker<SyncResponse>().Generate();
        var prerecordedSchema = new AutoFaker<PreRecordedSchema>().Generate();
        prerecordedSchema.CallBack = null;
        var source = GetFakeByteArray();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var prerecordedClient = Substitute.For<PreRecordedClient>(_apiKey, _options, null);

        // Mock Methods
        prerecordedClient.When(x => x.PostAsync<Stream, PreRecordedSchema, SyncResponse>(Arg.Any<string>(), Arg.Any<PreRecordedSchema>(), Arg.Any<Stream>())).DoNotCallBase();
        prerecordedClient.PostAsync<Stream, PreRecordedSchema, SyncResponse>(url, Arg.Any<PreRecordedSchema>(), Arg.Any<Stream>()).Returns(expectedResponse);

        // Act
        var result = await prerecordedClient.TranscribeFile(source, prerecordedSchema);

        // Assert
        await prerecordedClient.Received().PostAsync<Stream, PreRecordedSchema, SyncResponse>(url, Arg.Any<PreRecordedSchema>(), Arg.Any<Stream>());
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<SyncResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task TranscribeFileCallBack_With_Stream_With_CallBack_Property_Should_Call_PostAsync_Returning_SyncResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.LISTEN}");
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var prerecordedSchema = new AutoFaker<PreRecordedSchema>().Generate();
        var source = GetFakeStream(GetFakeByteArray());

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var prerecordedClient = Substitute.For<PreRecordedClient>(_apiKey, _options, null);
        
        // Mock Methods
        prerecordedClient.When(x => x.PostAsync<Stream, PreRecordedSchema, AsyncResponse>(Arg.Any<string>(), Arg.Any<PreRecordedSchema>(), Arg.Any<Stream>())).DoNotCallBase();
        prerecordedClient.PostAsync<Stream, PreRecordedSchema, AsyncResponse>(url, Arg.Any<PreRecordedSchema>(), Arg.Any<Stream>()).Returns(expectedResponse);

        // Act
        var result = await prerecordedClient.TranscribeFileCallBack(source, null, prerecordedSchema);

        // Assert
        await prerecordedClient.Received().PostAsync<Stream, PreRecordedSchema, AsyncResponse>(url, Arg.Any<PreRecordedSchema>(), Arg.Any<Stream>());
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<AsyncResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task TranscribeFileCallBack_With_Bytes_With_CallBack_Property_Should_Call_PostAsync_Returning_SyncResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.LISTEN}");
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var prerecordedSchema = new AutoFaker<PreRecordedSchema>().Generate();
        var source = GetFakeByteArray();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var prerecordedClient = Substitute.For<PreRecordedClient>(_apiKey, _options, null);
        
        // Mock Methods
        prerecordedClient.When(x => x.PostAsync<Stream, PreRecordedSchema, AsyncResponse>(Arg.Any<string>(), Arg.Any<PreRecordedSchema>(), Arg.Any<Stream>())).DoNotCallBase();
        prerecordedClient.PostAsync<Stream, PreRecordedSchema, AsyncResponse>(url, Arg.Any<PreRecordedSchema>(), Arg.Any<Stream>()).Returns(expectedResponse);

        // Act
        var result = await prerecordedClient.TranscribeFileCallBack(source, null, prerecordedSchema);

        // Assert
        await prerecordedClient.Received().PostAsync<Stream, PreRecordedSchema, AsyncResponse>(url, Arg.Any<PreRecordedSchema>(), Arg.Any<Stream>());

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<AsyncResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task TranscribeFileCallBack_With_Stream_With_CallBack_Parameter_Should_Call_PostAsync_Returning_SyncResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.LISTEN}");
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var prerecordedSchema = new AutoFaker<PreRecordedSchema>().Generate();
        var source = GetFakeStream(GetFakeByteArray());

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var prerecordedClient = Substitute.For<PreRecordedClient>(_apiKey, _options, null);
        
        // Mock Methods
        prerecordedClient.When(x => x.PostAsync<Stream, PreRecordedSchema, AsyncResponse>(Arg.Any<string>(), Arg.Any<PreRecordedSchema>(), Arg.Any<Stream>())).DoNotCallBase();
        prerecordedClient.PostAsync<Stream, PreRecordedSchema, AsyncResponse>(url, Arg.Any<PreRecordedSchema>(), Arg.Any<Stream>()).Returns(expectedResponse);

        //before we act to test this call with the callBack parameter and not the callBack property we need to null the callBack property
        var callBack = prerecordedSchema.CallBack;
        prerecordedSchema.CallBack = null;

        // Act
        var result = await prerecordedClient.TranscribeFileCallBack(source, callBack, prerecordedSchema);

        // Assert
        await prerecordedClient.Received().PostAsync<Stream, PreRecordedSchema, AsyncResponse>(url, Arg.Any<PreRecordedSchema>(), Arg.Any<Stream>());
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<AsyncResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task TranscribeFileCallBack_With_Bytes_With_CallBack_Parameter_Should_Call_PostAsync_Returning_SyncResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.LISTEN}");
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var prerecordedSchema = new AutoFaker<PreRecordedSchema>().Generate();
        var source = GetFakeByteArray();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var prerecordedClient = Substitute.For<PreRecordedClient>(_apiKey, _options, null);
        
        // Mock Methods
        prerecordedClient.When(x => x.PostAsync<Stream, PreRecordedSchema, AsyncResponse>(Arg.Any<string>(), Arg.Any<PreRecordedSchema>(), Arg.Any<Stream>())).DoNotCallBase();
        prerecordedClient.PostAsync<Stream, PreRecordedSchema, AsyncResponse>(url, Arg.Any<PreRecordedSchema>(), Arg.Any<Stream>()).Returns(expectedResponse);

        //before we act to test this call with the callBack parameter and not the callBack property we need to null the callBack property
        var callBack = prerecordedSchema.CallBack;
        prerecordedSchema.CallBack = null;

        // Act
        var result = await prerecordedClient.TranscribeFileCallBack(source, callBack, prerecordedSchema);

        // Assert
        await prerecordedClient.Received().PostAsync<Stream, PreRecordedSchema, AsyncResponse>(url, Arg.Any<PreRecordedSchema>(), Arg.Any<Stream>());
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<AsyncResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task TranscribeFileCallBack_With_Stream_Throw_ArgumentException_With_CallBack_Property_And_CallBack_Parameter_Set()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.LISTEN}");
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var prerecordedSchema = new AutoFaker<PreRecordedSchema>().Generate();
        var source = GetFakeStream(GetFakeByteArray());

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var prerecordedClient = Substitute.For<PreRecordedClient>(_apiKey, _options, null);
        
        // Mock Methods
        prerecordedClient.When(x => x.PostAsync<Stream, PreRecordedSchema, AsyncResponse>(Arg.Any<string>(), Arg.Any<PreRecordedSchema>(), Arg.Any<Stream>())).DoNotCallBase();
        prerecordedClient.PostAsync<Stream, PreRecordedSchema, AsyncResponse>(url, Arg.Any<PreRecordedSchema>(), Arg.Any<Stream>()).Returns(expectedResponse);
        
        var callBack = prerecordedSchema.CallBack;

        // Act and Assert
        await prerecordedClient.Invoking(y => y.TranscribeFileCallBack(source, callBack, prerecordedSchema))
           .Should().ThrowAsync<ArgumentException>();

        await prerecordedClient.DidNotReceive().PostAsync<Stream, PreRecordedSchema, AsyncResponse>(url, Arg.Any<PreRecordedSchema>(), Arg.Any<Stream>());
    }

    [Test]
    public async Task TranscribeFileCallBack_With_Bytes_Should_Throw_ArgumentException_With_No_CallBack_Set()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.LISTEN}");
        var expectedResponse = new AutoFaker<AsyncResponse>().Generate();
        var prerecordedSchema = new AutoFaker<PreRecordedSchema>().Generate();
        var source = GetFakeByteArray();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var prerecordedClient = Substitute.For<PreRecordedClient>(_apiKey, _options, null);
        
        // Mock Methods
        prerecordedClient.When(x => x.PostAsync<Stream, PreRecordedSchema, AsyncResponse>(Arg.Any<string>(), Arg.Any<PreRecordedSchema>(), Arg.Any<Stream>())).DoNotCallBase();
        prerecordedClient.PostAsync<Stream, PreRecordedSchema, AsyncResponse>(url, Arg.Any<PreRecordedSchema>(), Arg.Any<Stream>()).Returns(expectedResponse);
        
        prerecordedSchema.CallBack = null;

        // Act and Assert
        await prerecordedClient.Invoking(y => y.TranscribeFileCallBack(source, null, prerecordedSchema))
           .Should().ThrowAsync<ArgumentException>();

        await prerecordedClient.DidNotReceive().PostAsync<Stream, PreRecordedSchema, AsyncResponse>(url, Arg.Any<PreRecordedSchema>(), Arg.Any<Stream>());
    }

    private static Stream GetFakeStream(byte[] source)
    {
        var stream = new MemoryStream();
        stream.Write(source, 0, source.Length);
        return stream;
    }

    private static byte[] GetFakeByteArray() => new Faker().Random.Bytes(10);
}
