// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Encapsulations;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Manage.v1;
using Deepgram.Models.PreRecorded.v1;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class AbstractRestfulClientTests
{
    DeepgramClientOptions _clientOptions;
    string _apiKey;

    [SetUp]
    public void Setup()
    {
        _clientOptions = new DeepgramClientOptions();
        _apiKey = new Faker().Random.Guid().ToString();
    }


    [Test]
    public void GetAsync_Should_Throws_HttpRequestException_On_UnsuccessfulResponse()
    {
        // Arrange       
        var expectedResponse = new AutoFaker<ProjectsResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithException(new HttpRequestException());

        var client = new ConcreteRestClient(_apiKey, _clientOptions);
        // Act & Assert
        client.Invoking(y => y.GetAsync<ProjectsResponse>(UriSegments.PROJECTS))
             .Should().ThrowAsync<HttpRequestException>();
    }

    [Test]
    public void GetAsync_Should_Throws_Exception_On_UnsuccessfulResponse()
    {
        // Arrange       
        var expectedResponse = new AutoFaker<ProjectsResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithException(new Exception());

        var client = new ConcreteRestClient(_apiKey, _clientOptions);
        // Act & Assert
        client.Invoking(y => y.GetAsync<ProjectsResponse>(UriSegments.PROJECTS))
             .Should().ThrowAsync<Exception>();
    }

    // test that send stream content currently on in the prerecordedClient
    [Test]
    public void PostAsync_Which_Handles_HttpContent_Should_Throw_Exception_On_UnsuccessfulResponse()
    {
        // Arrange       
        var response = new AutoFaker<SyncResponse>().Generate();
        var httpContent = Substitute.For<HttpContent>();
        var httpClient = MockHttpClient.CreateHttpClientWithException(new Exception());

        var client = new ConcreteRestClient(_apiKey, _clientOptions);

        // Act & Assert      
        client.Invoking(y => y.PostAsync<SyncResponse>(UriSegments.PROJECTS, httpContent))
             .Should().ThrowAsync<Exception>();
    }

    // test that send stream content currently on in the prerecordedClient
    [Test]
    public void PostAsync_Which_Handles_HttpContent_Should_Throw_HttpRequestException_On_UnsuccessfulResponse()
    {
        // Arrange       
        var response = new AutoFaker<SyncResponse>().Generate();
        var httpContent = Substitute.For<HttpContent>();
        var httpClient = MockHttpClient.CreateHttpClientWithException(new HttpRequestException());
        var client = new ConcreteRestClient(_apiKey, _clientOptions);

        // Act & Assert      
        client.Invoking(y => y.PostAsync<SyncResponse>(UriSegments.PROJECTS, httpContent))
             .Should().ThrowAsync<HttpRequestException>();
    }


    [Test]
    public void PostAsync_Should_Throw_Exception_On_UnsuccessfulResponse()
    {
        // Arrange       
        var response = new AutoFaker<SyncResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithException(new Exception());

        var client = new ConcreteRestClient(_apiKey, _clientOptions);

        // Act & Assert  
        client.Invoking(y =>
        y.PostAsync<SyncResponse>(UriSegments.PROJECTS, new StringContent(string.Empty)))
             .Should().ThrowAsync<Exception>();
    }

    [Test]
    public void PostAsync_Should_Throw_HttpRequestException_On_UnsuccessfulResponse()
    {
        // Arrange       
        var response = new AutoFaker<SyncResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithException(new HttpRequestException());

        var client = new ConcreteRestClient(_apiKey, _clientOptions);

        // Act & Assert      
        client.Invoking(y =>
        y.PostAsync<SyncResponse>(UriSegments.PROJECTS, new StringContent(string.Empty)))
             .Should().ThrowAsync<HttpRequestException>();
    }


    //Test for the delete calls that do not return a value
    [Test]
    public async Task Delete_Should_Throws_HttpRequestException_On_Response_Containing_Error()
    {
        // Arrange    
        var httpClient = MockHttpClient.CreateHttpClientWithException(new HttpRequestException());

        var client = new ConcreteRestClient(_apiKey, _clientOptions);

        // Act & Assert
        await client.Invoking(async y => await y.DeleteAsync(UriSegments.PROJECTS))
             .Should().ThrowAsync<HttpRequestException>();
    }

    //Test for the delete calls that do not return a value
    [Test]
    public async Task Delete_Should_Throws_Exception_On_Response_Containing_Error()
    {
        // Arrange    
        var httpClient = MockHttpClient.CreateHttpClientWithException(new Exception());

        var client = new ConcreteRestClient(_apiKey, _clientOptions);

        // Act & Assert
        await client.Invoking(async y => await y.DeleteAsync(UriSegments.PROJECTS))
             .Should().ThrowAsync<Exception>();
    }


    [Test]
    public void DeleteAsync_TResponse_Should_Throws_HttpRequestException_On_UnsuccessfulResponse()
    {
        // Arrange       
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithException(new HttpRequestException());

        var client = new ConcreteRestClient(_apiKey, _clientOptions);

        // Act & Assert
        client.Invoking(y => y.DeleteAsync<MessageResponse>(UriSegments.PROJECTS))
             .Should().ThrowAsync<HttpRequestException>();
    }

    [Test]
    public void DeleteAsync_TResponse_Should_Throws_Exception_On_UnsuccessfulResponse()
    {
        // Arrange       
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithException(new Exception());

        var client = new ConcreteRestClient(_apiKey, _clientOptions);

        // Act & Assert
        client.Invoking(y => y.DeleteAsync<MessageResponse>(UriSegments.PROJECTS))
             .Should().ThrowAsync<Exception>();
    }

    [Test]
    public void PatchAsync_TResponse_Should_Throws_HttpRequestException_On_UnsuccessfulResponse()
    {
        // Arrange       
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithException(new HttpRequestException());

        var client = new ConcreteRestClient(_apiKey, _clientOptions);

        //Act & Assert
        client.Invoking(y => y.PatchAsync<MessageResponse>(UriSegments.PROJECTS, new StringContent(string.Empty)))
             .Should().ThrowAsync<HttpRequestException>();
    }

    [Test]
    public void PatchAsync_TResponse_Should_Throws_Exception_On_UnsuccessfulResponse()
    {
        // Arrange       
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithException(new Exception());

        var client = new ConcreteRestClient(_apiKey, _clientOptions);

        //Act & Assert
        client.Invoking(y => y.PatchAsync<MessageResponse>(UriSegments.PROJECTS, new StringContent(string.Empty)))
             .Should().ThrowAsync<Exception>();
    }


    [Test]
    public void PutAsync_TResponse_Should_Throws_HttpRequestException_On_UnsuccessfulResponse()
    {
        // Arrange       
        var httpClient = MockHttpClient.CreateHttpClientWithException(new HttpRequestException());

        var client = new ConcreteRestClient(_apiKey, _clientOptions);

        // Act & Assert
        client.Invoking(y => y.PutAsync<MessageResponse>(UriSegments.PROJECTS, new StringContent(string.Empty)))
             .Should().ThrowAsync<HttpRequestException>();
    }

    [Test]
    public void PutAsync_TResponse_Should_Throws_Exception_On_UnsuccessfulResponse()
    {
        // Arrange       
        var httpClient = MockHttpClient.CreateHttpClientWithException(new Exception());

        var client = new ConcreteRestClient(_apiKey, _clientOptions);
        // Act & Assert
        client.Invoking(y => y.PutAsync<MessageResponse>(UriSegments.PROJECTS, new StringContent(string.Empty)))
             .Should().ThrowAsync<Exception>();
    }
}
