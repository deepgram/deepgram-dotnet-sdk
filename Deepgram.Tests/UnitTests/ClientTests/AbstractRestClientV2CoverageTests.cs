// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Abstractions.v2;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Exceptions.v1;
using Deepgram.Models.Manage.v1;
using Deepgram.Tests.Fakes;
using FluentAssertions;
using FluentAssertions.Execution;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class AbstractRestClientV2CoverageTests
{
    private const string Uri = "https://api.deepgram.com/v1/projects";
    private const string SuccessBody = """{"message":"ok"}""";

    private sealed class TestRestClient : AbstractRestClient
    {
        public TestRestClient(string apiKey, DeepgramHttpClientOptions options)
            : base(apiKey, options)
        {
        }

        public void SetHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
    }

    private static TestRestClient NewClient(string responseBody = SuccessBody, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var options = new DeepgramHttpClientOptions("test-api-key") { OnPrem = true };
        var client = new TestRestClient("test-api-key", options);
        client.SetHttpClient(MockHttpClient.CreateHttpClientWithRawResult(responseBody, statusCode));
        return client;
    }

    [Test]
    public async Task Public_Request_Methods_Should_Deserialize_Successful_Responses()
    {
        var client = NewClient();
        var body = new ProjectSchema { Name = "coverage" };
        var headers = new Dictionary<string, string> { ["X-Coverage-Test"] = "true" };

        var get = await client.GetAsync<MessageResponse>(Uri, headers: headers);
        var getWithParameters = await client.GetAsync<ProjectSchema, MessageResponse>(Uri, body);
        var post = await client.PostAsync<ProjectSchema, MessageResponse>(Uri, body);
        var postWithContent = await client.PostAsync<ProjectSchema, ProjectSchema, MessageResponse>(Uri, body, body);
        var postWithStream = await client.PostAsync<Stream, ProjectSchema, MessageResponse>(Uri, body, new MemoryStream([1, 2, 3]));
        var postWithEmptyStream = await client.PostAsync<Stream, ProjectSchema, MessageResponse>(Uri, body, null);
        var patch = await client.PatchAsync<ProjectSchema, MessageResponse>(Uri, body);
        var put = await client.PutAsync<ProjectSchema, MessageResponse>(Uri, body);
        var delete = await client.DeleteAsync<MessageResponse>(Uri);
        var deleteWithParameters = await client.DeleteAsync<ProjectSchema, MessageResponse>(Uri, body);
        var file = await client.PostRetrieveLocalFileAsync<ProjectSchema, ProjectSchema, MessageResponse>(
            Uri,
            body,
            body,
            ["content-type"]);

        using (new AssertionScope())
        {
            get.Message.Should().Be("ok");
            getWithParameters.Message.Should().Be("ok");
            post.Message.Should().Be("ok");
            postWithContent.Message.Should().Be("ok");
            postWithStream.Message.Should().Be("ok");
            postWithEmptyStream.Message.Should().Be("ok");
            patch.Message.Should().Be("ok");
            put.Message.Should().Be("ok");
            delete.Message.Should().Be("ok");
            deleteWithParameters.Message.Should().Be("ok");
            file.Metadata["content-type"].Should().Be("text/plain");
            file.Content.Should().NotBeNull();
        }
    }

    [Test]
    public async Task Request_Methods_Should_Deserialize_Deepgram_Error_Responses()
    {
        var client = NewClient(
            """{"err_code":"INVALID_AUTH","err_msg":"invalid credentials","request_id":"request-1"}""",
            HttpStatusCode.Unauthorized);

        var exception = await client.Invoking(c => c.GetAsync<MessageResponse>(Uri))
            .Should().ThrowAsync<DeepgramRESTException>();

        using (new AssertionScope())
        {
            exception.Which.ErrCode.Should().Be("INVALID_AUTH");
            exception.Which.ErrMsg.Should().Be("invalid credentials");
            exception.Which.RequestId.Should().Be("request-1");
        }
    }
}
