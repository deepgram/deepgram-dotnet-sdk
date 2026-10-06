// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Abstractions.v1;
using Deepgram.Clients.Interfaces.v1;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Manage.v1;

namespace Deepgram.Clients.Manage.v1;

/// <summary>
/// Implements current project usage, billing, and purchase reporting endpoints.
/// </summary>
public class ReportingClient(string? apiKey = null, IDeepgramClientOptions? deepgramClientOptions = null, string? httpId = null)
    : AbstractRestClient(apiKey, deepgramClientOptions, httpId), IManagementReportingClient
{
    public async Task<UsageBreakdownResponse> GetUsageBreakdown(string projectId, UsageBreakdownSchema? schema = null,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.USAGE}/{UriSegments.BREAKDOWN}");
        return (await GetAsync<UsageBreakdownSchema, UsageBreakdownResponse>(uri, schema, cancellationToken, addons, headers))!;
    }

    public async Task<BillingBreakdownResponse> GetBillingBreakdown(string projectId, BillingBreakdownSchema? schema = null,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.BILLING}/{UriSegments.BREAKDOWN}");
        return (await GetAsync<BillingBreakdownSchema, BillingBreakdownResponse>(uri, schema, cancellationToken, addons, headers))!;
    }

    public async Task<BillingFieldsResponse> GetBillingFields(string projectId, BillingFieldsSchema? schema = null,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.BILLING}/{UriSegments.FIELDS}");
        return (await GetAsync<BillingFieldsSchema, BillingFieldsResponse>(uri, schema, cancellationToken, addons, headers))!;
    }

    public async Task<PurchasesResponse> GetPurchases(string projectId, PurchasesSchema? schema = null,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.PURCHASES}");
        return (await GetAsync<PurchasesSchema, PurchasesResponse>(uri, schema, cancellationToken, addons, headers))!;
    }
}
