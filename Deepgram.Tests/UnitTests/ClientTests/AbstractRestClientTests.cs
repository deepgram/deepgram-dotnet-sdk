// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Manage.v1;
using Deepgram.Models.PreRecorded.v1;
using Deepgram.Models.Exceptions.v1;

using Deepgram.Clients.Manage.v1;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class AbstractRestfulClientTests
{
    DeepgramHttpClientOptions _clientOptions;
    string _apiKey;

    [SetUp]
    public void Setup()
    {
        _apiKey = new Faker().Random.Guid().ToString();
        _clientOptions = new DeepgramHttpClientOptions(_apiKey)
        {
            OnPrem = true,
        };
    }

    [Test]
    public void GetAsync_Should_Throws_HttpRequestException_On_UnsuccessfulResponse()
    {
        // Input and Output       
        var expectedResponse = new AutoFaker<ProjectsResponse>().Generate();

        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithException(new HttpRequestException());
        var client = new ConcreteRestClient(_apiKey, _clientOptions);

        // Act & Assert
        client.Invoking(y => y.GetAsync<ProjectsResponse>($"{Defaults.DEFAULT_URI}/{UriSegments.PROJECTS}"))
             .Should().ThrowAsync<HttpRequestException>();
    }

    [Test]
    public void GetAsync_Should_Throws_Exception_On_UnsuccessfulResponse()
    {
        // Input and Output       
        var expectedResponse = new AutoFaker<ProjectsResponse>().Generate();

        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithException(new Exception());
        var client = new ConcreteRestClient(_apiKey, _clientOptions);

        // Act & Assert
        client.Invoking(y => y.GetAsync<ProjectsResponse>($"{Defaults.DEFAULT_URI}/{UriSegments.PROJECTS}"))
             .Should().ThrowAsync<Exception>();
    }

    // test that send stream content currently on in the prerecordedClient
    [Test]
    public void PostAsync_Which_Handles_HttpContent_Should_Throw_Exception_On_UnsuccessfulResponse()
    {
        // Input and Output     
        var response = new AutoFaker<SyncResponse>().Generate();
        var httpContent = Substitute.For<HttpContent>();

        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithException(new Exception());
        var client = new ConcreteRestClient(_apiKey, _clientOptions);

        // Act & Assert      
        client.Invoking(y => y.PostAsync<HttpContent, SyncResponse>(UriSegments.PROJECTS, httpContent))
             .Should().ThrowAsync<Exception>();
    }

    // test that send stream content currently on in the prerecordedClient
    [Test]
    public void PostAsync_Which_Handles_HttpContent_Should_Throw_HttpRequestException_On_UnsuccessfulResponse()
    {
        // Input and Output      
        var response = new AutoFaker<SyncResponse>().Generate();
        var httpContent = Substitute.For<HttpContent>();

        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithException(new HttpRequestException());
        var client = new ConcreteRestClient(_apiKey, _clientOptions);

        // Act & Assert      
        client.Invoking(y => y.PostAsync<HttpContent, SyncResponse>(UriSegments.PROJECTS, httpContent))
             .Should().ThrowAsync<HttpRequestException>();
    }


    [Test]
    public void PostAsync_Should_Throw_Exception_On_UnsuccessfulResponse()
    {
        // Input and Output      
        var response = new AutoFaker<SyncResponse>().Generate();

        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithException(new Exception());
        var client = new ConcreteRestClient(_apiKey, _clientOptions);

        // Act & Assert  
        client.Invoking(y =>
        y.PostAsync<StringContent, SyncResponse>(UriSegments.PROJECTS, new StringContent(string.Empty)))
             .Should().ThrowAsync<Exception>();
    }

    [Test]
    public void PostAsync_Should_Throw_HttpRequestException_On_UnsuccessfulResponse()
    {
        // Input and Output       
        var response = new AutoFaker<SyncResponse>().Generate();

        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithException(new HttpRequestException());
        var client = new ConcreteRestClient(_apiKey, _clientOptions);

        // Act & Assert      
        client.Invoking(y =>
        y.PostAsync<StringContent, SyncResponse>(UriSegments.PROJECTS, new StringContent(string.Empty)))
             .Should().ThrowAsync<HttpRequestException>();
    }


    //Test for the delete calls that do not return a value
    [Test]
    public async Task Delete_Should_Throws_HttpRequestException_On_Response_Containing_Error()
    {
        // Input and Output  
        var httpClient = MockHttpClient.CreateHttpClientWithException(new HttpRequestException());

        // Fake Clients
        var client = new ConcreteRestClient(_apiKey, _clientOptions);

        // Act & Assert
        await client.Invoking(async y => await y.DeleteAsync<MessageResponse>($"{Defaults.DEFAULT_URI}/{UriSegments.PROJECTS}"))
             .Should().ThrowAsync<HttpRequestException>();
    }

    //Test for the delete calls that do not return a value
    [Test]
    public async Task Delete_Should_Throws_Exception_On_Response_Containing_Error()
    {
        // Input and Output    
        var httpClient = MockHttpClient.CreateHttpClientWithException(new Exception());

        // Fake Clients
        var client = new ConcreteRestClient(_apiKey, _clientOptions);

        // Act & Assert
        await client.Invoking(async y => await y.DeleteAsync<MessageResponse>($"{Defaults.DEFAULT_URI}/{UriSegments.PROJECTS}"))
             .Should().ThrowAsync<Exception>();
    }

    [Test]
    public void DeleteAsync_TResponse_Should_Throws_HttpRequestException_On_UnsuccessfulResponse()
    {
        // Input and Output 
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();

        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithException(new HttpRequestException());
        var client = new ConcreteRestClient(_apiKey, _clientOptions);

        // Act & Assert
        client.Invoking(y => y.DeleteAsync<MessageResponse>($"{Defaults.DEFAULT_URI}/{UriSegments.PROJECTS}"))
             .Should().ThrowAsync<HttpRequestException>();
    }

    [Test]
    public void DeleteAsync_TResponse_Should_Throws_Exception_On_UnsuccessfulResponse()
    {
        // Input and Output       
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();

        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithException(new Exception());
        var client = new ConcreteRestClient(_apiKey, _clientOptions);

        // Act & Assert
        client.Invoking(y => y.DeleteAsync<MessageResponse>($"{Defaults.DEFAULT_URI}/{UriSegments.PROJECTS}"))
             .Should().ThrowAsync<Exception>();
    }

    [Test]
    public void PatchAsync_TResponse_Should_Throws_HttpRequestException_On_UnsuccessfulResponse()
    {
        // Input and Output       
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();

        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithException(new HttpRequestException());
        var client = new ConcreteRestClient(_apiKey, _clientOptions);

        //Act & Assert
        client.Invoking(y => y.PatchAsync<StringContent, MessageResponse>(UriSegments.PROJECTS, new StringContent(string.Empty)))
             .Should().ThrowAsync<HttpRequestException>();
    }

    [Test]
    public void PatchAsync_TResponse_Should_Throws_Exception_On_UnsuccessfulResponse()
    {
        // Input and Output
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();

        // Fake Clients
        var httpClient = MockHttpClient.CreateHttpClientWithException(new Exception());
        var client = new ConcreteRestClient(_apiKey, _clientOptions);

        //Act & Assert
        client.Invoking(y => y.PatchAsync<StringContent, MessageResponse>(UriSegments.PROJECTS, new StringContent(string.Empty)))
             .Should().ThrowAsync<Exception>();
    }

    [Test]
    public void PutAsync_TResponse_Should_Throws_HttpRequestException_On_UnsuccessfulResponse()
    {
        /// Input and Output
        var httpClient = MockHttpClient.CreateHttpClientWithException(new HttpRequestException());

        // Fake Clients
        var client = new ConcreteRestClient(_apiKey, _clientOptions);

        // Act & Assert
        client.Invoking(y => y.PutAsync<StringContent, MessageResponse>(UriSegments.PROJECTS, new StringContent(string.Empty)))
             .Should().ThrowAsync<HttpRequestException>();
    }

    [Test]
    public void PutAsync_TResponse_Should_Throws_Exception_On_UnsuccessfulResponse()
    {
        // Input and Output
        var httpClient = MockHttpClient.CreateHttpClientWithException(new Exception());

        // Fake Clients
        var client = new ConcreteRestClient(_apiKey, _clientOptions);

        // Act & Assert
        client.Invoking(y => y.PutAsync<StringContent, MessageResponse>(UriSegments.PROJECTS, new StringContent(string.Empty)))
             .Should().ThrowAsync<Exception>();
    }
}
