// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Clients.Interfaces.v1;
using Deepgram.Clients.Manage.v1;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Manage.v1;
using Deepgram.Utilities;
using ReportingClient = Deepgram.Clients.Manage.v1.ReportingClient;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class ManagementReportingClientTests
{
    private const string ApiKey = "test-api-key";
    private const string ProjectId = "project-id";
    private readonly DeepgramHttpClientOptions _options = new(ApiKey) { OnPrem = true };

    [Test]
    public async Task ReportingClient_Should_Call_Current_Reporting_Routes()
    {
        var client = Substitute.For<ReportingClient>(ApiKey, _options, null);
        var usageSchema = new UsageBreakdownSchema { Grouping = "models" };
        var billingSchema = new BillingBreakdownSchema { Grouping = new List<string> { "deployment", "tags" } };
        var fieldsSchema = new BillingFieldsSchema { Start = new DateTime(2026, 1, 1) };
        var purchasesSchema = new PurchasesSchema { Limit = 25 };
        var usageUri = Deepgram.Abstractions.v1.AbstractRestClient.GetUri(_options, "projects/project-id/usage/breakdown");
        var billingUri = Deepgram.Abstractions.v1.AbstractRestClient.GetUri(_options, "projects/project-id/billing/breakdown");
        var fieldsUri = Deepgram.Abstractions.v1.AbstractRestClient.GetUri(_options, "projects/project-id/billing/fields");
        var purchasesUri = Deepgram.Abstractions.v1.AbstractRestClient.GetUri(_options, "projects/project-id/purchases");

        client.GetAsync<UsageBreakdownSchema, UsageBreakdownResponse>(usageUri, usageSchema).Returns(new UsageBreakdownResponse());
        client.GetAsync<BillingBreakdownSchema, BillingBreakdownResponse>(billingUri, billingSchema).Returns(new BillingBreakdownResponse());
        client.GetAsync<BillingFieldsSchema, BillingFieldsResponse>(fieldsUri, fieldsSchema).Returns(new BillingFieldsResponse());
        client.GetAsync<PurchasesSchema, PurchasesResponse>(purchasesUri, purchasesSchema).Returns(new PurchasesResponse());

        await client.GetUsageBreakdown(ProjectId, usageSchema);
        await client.GetBillingBreakdown(ProjectId, billingSchema);
        await client.GetBillingFields(ProjectId, fieldsSchema);
        await client.GetPurchases(ProjectId, purchasesSchema);

        await client.Received(1).GetAsync<UsageBreakdownSchema, UsageBreakdownResponse>(usageUri, usageSchema);
        await client.Received(1).GetAsync<BillingBreakdownSchema, BillingBreakdownResponse>(billingUri, billingSchema);
        await client.Received(1).GetAsync<BillingFieldsSchema, BillingFieldsResponse>(fieldsUri, fieldsSchema);
        await client.Received(1).GetAsync<PurchasesSchema, PurchasesResponse>(purchasesUri, purchasesSchema);
    }

    [Test]
    public void ReportingSchemas_Should_Serialize_Current_Query_Parameters()
    {
        var usage = QueryParameterUtil.FormatURL("https://api.deepgram.com/v1/projects/project-id/usage/breakdown",
            new UsageBreakdownSchema { Start = new DateTime(2026, 1, 1), Grouping = "models", Deployment = "hosted", Numerals = true });
        var billing = QueryParameterUtil.FormatURL("https://api.deepgram.com/v1/projects/project-id/billing/breakdown",
            new BillingBreakdownSchema { Grouping = new List<string> { "deployment", "tags" }, LineItem = "streaming::nova-3" });
        var fields = QueryParameterUtil.FormatURL("https://api.deepgram.com/v1/projects/project-id/billing/fields",
            new BillingFieldsSchema { Start = new DateTime(2026, 1, 1), End = new DateTime(2026, 1, 31) });
        var purchases = QueryParameterUtil.FormatURL("https://api.deepgram.com/v1/projects/project-id/purchases",
            new PurchasesSchema { Limit = 25 });
        var requests = QueryParameterUtil.FormatURL("https://api.deepgram.com/v1/projects/project-id/requests",
            new UsageRequestsSchema { Accessor = "accessor-id", RequestId = "request-id", Deployment = "hosted", Endpoint = "listen", Method = "sync" });

        using (new AssertionScope())
        {
            usage.Should().Contain("start=2026-01-01").And.Contain("grouping=models").And.Contain("deployment=hosted").And.Contain("numerals=true");
            billing.Should().Contain("grouping=%5b%22deployment%22%2c%22tags%22%5d").And.Contain("line_item=streaming%3a%3anova-3");
            fields.Should().Contain("start=2026-01-01").And.Contain("end=2026-01-31");
            purchases.Should().Contain("limit=25");
            requests.Should().Contain("accessor=accessor-id").And.Contain("request_id=request-id").And.Contain("deployment=hosted").And.Contain("endpoint=listen").And.Contain("method=sync");
        }
    }

    [Test]
    public async Task ManageClientExtensions_Should_Forward_Typed_Project_And_Key_Filters()
    {
        var client = Substitute.For<IManageClient>();
        client.GetProject(ProjectId, Arg.Any<CancellationTokenSource>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>())
            .Returns(new ProjectResponse());
        client.GetKeys(ProjectId, Arg.Any<CancellationTokenSource>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>())
            .Returns(new KeysResponse());

        await client.GetProject(ProjectId, new ProjectQuerySchema { Limit = 50, Page = 2 });
        await client.GetKeys(ProjectId, new KeysQuerySchema { Status = "active" });

        await client.Received(1).GetProject(ProjectId, null,
            Arg.Is<Dictionary<string, string>>(query => query["limit"] == "50" && query["page"] == "2"), null);
        await client.Received(1).GetKeys(ProjectId, null,
            Arg.Is<Dictionary<string, string>>(query => query["status"] == "active"), null);
    }

    [Test]
    public void ReportingResponses_Should_Deserialize_Current_Contract()
    {
        var usage = JsonSerializer.Deserialize<UsageBreakdownResponse>("""{"start":"2026-01-01","end":"2026-01-31","resolution":{"units":"day","amount":1},"results":[{"hours":1.5,"total_hours":2,"agent_hours":0.5,"tokens_in":12,"tokens_out":8,"tts_characters":40,"requests":3,"grouping":{"models":["nova-3",null],"tags":null}}]}""");
        var billing = JsonSerializer.Deserialize<BillingBreakdownResponse>("""{"start":"2026-01-01","end":"2026-01-31","resolution":{"units":"day","amount":1},"results":[{"dollars":0.25,"grouping":{"line_item":"streaming::nova-3","tags":["production"]}}]}""");
        var fields = JsonSerializer.Deserialize<BillingFieldsResponse>("""{"accessors":["accessor-id"],"deployments":["hosted"],"tags":["production"],"line_items":{"streaming::nova-3":"Streaming STT"}}""");
        var purchases = JsonSerializer.Deserialize<PurchasesResponse>("""{"orders":[{"order_id":"order-id","expiration":"2026-12-31T00:00:00Z","created":"2026-01-01T01:02:03Z","amount":150,"units":"usd","order_type":"promotional"}]}""");

        using (new AssertionScope())
        {
            usage!.Results![0].AgentHours.Should().Be(0.5d);
            usage.Results[0].Grouping!.Models.Should().Contain("nova-3").And.Contain((string?)null);
            billing!.Results![0].Grouping!.LineItem.Should().Be("streaming::nova-3");
            fields!.LineItems!["streaming::nova-3"].Should().Be("Streaming STT");
            purchases!.Orders![0].Expiration.Should().Be(new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc));
        }
    }
}
