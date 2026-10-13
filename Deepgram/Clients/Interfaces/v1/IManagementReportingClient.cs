// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Manage.v1;

namespace Deepgram.Clients.Interfaces.v1;

/// <summary>
/// Retrieves current project usage, billing, and purchase reporting.
/// </summary>
public interface IManagementReportingClient
{
    Task<UsageBreakdownResponse> GetUsageBreakdown(string projectId, UsageBreakdownSchema? schema = null,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    Task<BillingBreakdownResponse> GetBillingBreakdown(string projectId, BillingBreakdownSchema? schema = null,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    Task<BillingFieldsResponse> GetBillingFields(string projectId, BillingFieldsSchema? schema = null,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    Task<PurchasesResponse> GetPurchases(string projectId, PurchasesSchema? schema = null,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);
}
