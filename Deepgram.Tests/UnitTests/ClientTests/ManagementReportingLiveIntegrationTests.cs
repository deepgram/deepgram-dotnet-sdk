// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Manage.v1;

namespace Deepgram.Tests.UnitTests.ClientTests;

/// <summary>
/// Read-only verification of current reporting endpoints. This is skipped unless both an API key
/// and an explicitly selected project are available; it never prints reporting response bodies.
/// </summary>
public class ManagementReportingLiveIntegrationTests
{
    [Test]
    public async Task Live_GetCurrentReporting_Should_Return_All_Reporting_Responses()
    {
        var apiKey = GlobalTestEnvironment.DeepgramApiKeyAtStartup
            ?? Environment.GetEnvironmentVariable("DEEPGRAM_API_KEY");
        var projectId = Environment.GetEnvironmentVariable("DEEPGRAM_PROJECT_ID");
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(projectId))
        {
            Assert.Ignore("DEEPGRAM_API_KEY and DEEPGRAM_PROJECT_ID are required. Skipping live reporting test.");
        }

        var client = ClientFactory.CreateManagementReportingClient(apiKey!);
        var start = DateTime.UtcNow.Date.AddDays(-7);
        var end = DateTime.UtcNow.Date;

        var usage = await client.GetUsageBreakdown(projectId!, new UsageBreakdownSchema
        {
            Start = start,
            End = end,
            Grouping = "models",
        });
        var billing = await client.GetBillingBreakdown(projectId, new BillingBreakdownSchema
        {
            Start = start,
            End = end,
            Grouping = new List<string> { "deployment" },
        });
        var fields = await client.GetBillingFields(projectId, new BillingFieldsSchema { Start = start, End = end });
        var purchases = await client.GetPurchases(projectId, new PurchasesSchema { Limit = 1 });

        using (new AssertionScope())
        {
            usage.Results.Should().NotBeNull();
            billing.Results.Should().NotBeNull();
            fields.Should().NotBeNull();
            purchases.Should().NotBeNull();
        }
    }
}
