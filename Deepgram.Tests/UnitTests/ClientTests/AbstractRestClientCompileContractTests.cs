// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Abstractions.v1;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Manage.v1;

namespace Deepgram.Tests.UnitTests.ClientTests;

/// <summary>
/// Source-compatibility contract for the public surface of the v1 <see cref="AbstractRestClient"/>.
/// Every public request method is called exactly as 7.0 consumers may already call it: with a
/// positional <c>default</c> in the <c>CancellationTokenSource?</c> slot. The assertion is the
/// compile itself — if any same-named overload is ever added whose parameter sits in that slot
/// (e.g. a <c>bool</c>), these calls stop compiling with CS0121 and this file fails to build.
///
/// The empty-body-tolerant behaviour needed by the Agent management endpoints lives behind
/// distinctly named <c>protected internal</c> helpers, and the public methods keep their
/// fail-fast contract on an empty 200. Both halves are pinned below.
/// </summary>
public class AbstractRestClientCompileContractTests
{
    private const string Uri = "https://api.deepgram.com/v1/projects/p";
    private const string ValidBody = """{"message":"ok"}""";

    private DeepgramHttpClientOptions _options = null!;

    [SetUp]
    public void SetUp()
    {
        _options = new DeepgramHttpClientOptions(new Faker().Random.Guid().ToString()) { OnPrem = true };
    }

    private ConcreteRestClient NewClientReturning(string rawBody)
    {
        var client = new ConcreteRestClient(_options.ApiKey, _options);
        client._httpClient = MockHttpClient.CreateHttpClientWithRawResult(rawBody, HttpStatusCode.OK);
        return client;
    }

    [Test]
    public async Task Every_Public_7_0_Signature_Compiles_With_Positional_Default_Cancellation()
    {
        var client = NewClientReturning(ValidBody);
        var body = new ProjectSchema { Name = "x" };

        // The two calls from the B6 report ...
        var del = await client.DeleteAsync<MessageResponse>(Uri, default);
        var put = await client.PutAsync<ProjectSchema, MessageResponse>(Uri, body, default);
        // ... the body-only PATCH shape that had the identical positional collision ...
        var patchRst = await client.PatchAsync<ProjectSchema, ProjectSchema, MessageResponse>(Uri, null, body, default);
        // ... and the remaining public request methods for completeness.
        var get = await client.GetAsync<MessageResponse>(Uri, default);
        var getS = await client.GetAsync<ProjectSchema, MessageResponse>(Uri, body, default);
        var post = await client.PostAsync<ProjectSchema, MessageResponse>(Uri, body, default);
        var postRst = await client.PostAsync<ProjectSchema, ProjectSchema, MessageResponse>(Uri, null, body, default);
        var patch = await client.PatchAsync<ProjectSchema, MessageResponse>(Uri, body, default);
        var delS = await client.DeleteAsync<ProjectSchema, MessageResponse>(Uri, body, default);
        var file = await client.PostRetrieveLocalFileAsync<ProjectSchema, ProjectSchema, MessageResponse>(Uri, null, body, null, default);

        // Compiling is the contract; the values just prove the calls went through the transport.
        using (new AssertionScope())
        {
            del.Message.Should().Be("ok");
            put.Message.Should().Be("ok");
            patchRst.Message.Should().Be("ok");
            get.Message.Should().Be("ok");
            getS.Message.Should().Be("ok");
            post.Message.Should().Be("ok");
            postRst.Message.Should().Be("ok");
            patch.Message.Should().Be("ok");
            delS.Message.Should().Be("ok");
            file.Content.Should().NotBeNull();
        }
    }

    [Test]
    public async Task AllowingEmptyResponse_Helpers_Return_Default_On_Empty_200()
    {
        var client = NewClientReturning("");
        var body = new ProjectSchema { Name = "x" };

        var put = await client.PutAllowingEmptyResponseAsync<ProjectSchema, MessageResponse>(Uri, body);
        var patch = await client.PatchAllowingEmptyResponseAsync<ProjectSchema, ProjectSchema, MessageResponse>(Uri, null, body);
        var del = await client.DeleteAllowingEmptyResponseAsync<MessageResponse>(Uri);

        using (new AssertionScope())
        {
            put.Should().BeNull();
            patch.Should().BeNull();
            del.Should().BeNull();
        }
    }

    [Test]
    public async Task Public_Methods_Still_Fail_Fast_On_Empty_200()
    {
        // The B2 contract: a truncated/empty 200 must never become a successful null result on
        // the public request methods. Only the distinctly named helpers above tolerate it.
        var client = NewClientReturning("");
        var body = new ProjectSchema { Name = "x" };

        await client.Invoking(c => c.PutAsync<ProjectSchema, MessageResponse>(Uri, body, default))
            .Should().ThrowAsync<JsonException>();
        await client.Invoking(c => c.PatchAsync<ProjectSchema, ProjectSchema, MessageResponse>(Uri, null, body, default))
            .Should().ThrowAsync<JsonException>();
        await client.Invoking(c => c.DeleteAsync<MessageResponse>(Uri, default))
            .Should().ThrowAsync<JsonException>();
    }
}
