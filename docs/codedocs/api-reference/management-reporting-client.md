---
title: "ManagementReportingClient"
description: "Read current project usage, billing, purchase, and reporting-field data."
---

Source files: `Deepgram/ManagementReportingClient.cs`, `Deepgram/Clients/Interfaces/v1/IManagementReportingClient.cs`, and `Deepgram/Clients/Manage/v1/ReportingClient.cs`.

Create the client with `ClientFactory.CreateManagementReportingClient()`. It is separate from `ManageClient` so current reporting models can evolve without changing the legacy management interface.

```csharp
var reporting = ClientFactory.CreateManagementReportingClient();
var usage = await reporting.GetUsageBreakdown(projectId, new UsageBreakdownSchema
{
    Start = DateTime.UtcNow.Date.AddDays(-7),
    End = DateTime.UtcNow.Date,
    Grouping = "models",
});

var billing = await reporting.GetBillingBreakdown(projectId, new BillingBreakdownSchema
{
    Grouping = new List<string> { "deployment", "tags" },
});
```

## Public Methods

```csharp
Task<UsageBreakdownResponse> GetUsageBreakdown(string projectId, UsageBreakdownSchema? schema = null, ...)
Task<BillingBreakdownResponse> GetBillingBreakdown(string projectId, BillingBreakdownSchema? schema = null, ...)
Task<BillingFieldsResponse> GetBillingFields(string projectId, BillingFieldsSchema? schema = null, ...)
Task<PurchasesResponse> GetPurchases(string projectId, PurchasesSchema? schema = null, ...)
```

All methods are read-only, but responses can reveal sensitive usage, cost, and purchase information. Limit access to the reporting client and avoid logging raw responses in production.

`GetUsageSummary` remains available on `ManageClient` for compatibility. Prefer `GetUsageBreakdown` for new reporting integrations because it exposes agent hours, input/output tokens, TTS characters, explicit groupings, and current filters.
