// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Manage.v1;

public class UsageBreakdownResponse
{
    [JsonPropertyName("start")] public DateTime? Start { get; set; }
    [JsonPropertyName("end")] public DateTime? End { get; set; }
    [JsonPropertyName("resolution")] public ReportingResolution? Resolution { get; set; }
    [JsonPropertyName("results")] public List<UsageBreakdownResult>? Results { get; set; }
}

public class UsageBreakdownResult
{
    [JsonPropertyName("hours")] public double? Hours { get; set; }
    [JsonPropertyName("total_hours")] public double? TotalHours { get; set; }
    [JsonPropertyName("agent_hours")] public double? AgentHours { get; set; }
    [JsonPropertyName("tokens_in")] public double? TokensIn { get; set; }
    [JsonPropertyName("tokens_out")] public double? TokensOut { get; set; }
    [JsonPropertyName("tts_characters")] public double? TtsCharacters { get; set; }
    [JsonPropertyName("requests")] public double? Requests { get; set; }
    [JsonPropertyName("grouping")] public UsageBreakdownGrouping? Grouping { get; set; }
}

public class UsageBreakdownGrouping
{
    [JsonPropertyName("start")] public DateTime? Start { get; set; }
    [JsonPropertyName("end")] public DateTime? End { get; set; }
    [JsonPropertyName("accessor")] public string? Accessor { get; set; }
    [JsonPropertyName("endpoint")] public string? Endpoint { get; set; }
    [JsonPropertyName("feature_set")] public string? FeatureSet { get; set; }
    [JsonPropertyName("models")] public List<string?>? Models { get; set; }
    [JsonPropertyName("method")] public string? Method { get; set; }
    [JsonPropertyName("tags")] public List<string>? Tags { get; set; }
    [JsonPropertyName("deployment")] public string? Deployment { get; set; }
}

public class BillingBreakdownResponse
{
    [JsonPropertyName("start")] public DateTime? Start { get; set; }
    [JsonPropertyName("end")] public DateTime? End { get; set; }
    [JsonPropertyName("resolution")] public ReportingResolution? Resolution { get; set; }
    [JsonPropertyName("results")] public List<BillingBreakdownResult>? Results { get; set; }
}

public class BillingBreakdownResult
{
    [JsonPropertyName("dollars")] public double? Dollars { get; set; }
    [JsonPropertyName("grouping")] public BillingBreakdownGrouping? Grouping { get; set; }
}

public class BillingBreakdownGrouping
{
    [JsonPropertyName("start")] public DateTime? Start { get; set; }
    [JsonPropertyName("end")] public DateTime? End { get; set; }
    [JsonPropertyName("accessor")] public string? Accessor { get; set; }
    [JsonPropertyName("deployment")] public string? Deployment { get; set; }
    [JsonPropertyName("line_item")] public string? LineItem { get; set; }
    [JsonPropertyName("tags")] public List<string>? Tags { get; set; }
}

public class ReportingResolution
{
    [JsonPropertyName("units")] public string? Units { get; set; }
    [JsonPropertyName("amount")] public decimal? Amount { get; set; }
}

public class BillingFieldsResponse
{
    [JsonPropertyName("accessors")] public List<string>? Accessors { get; set; }
    [JsonPropertyName("deployments")] public List<string>? Deployments { get; set; }
    [JsonPropertyName("tags")] public List<string>? Tags { get; set; }
    [JsonPropertyName("line_items")] public Dictionary<string, string>? LineItems { get; set; }
}

public class PurchasesResponse
{
    [JsonPropertyName("orders")] public List<PurchaseOrder>? Orders { get; set; }
}

public class PurchaseOrder
{
    [JsonPropertyName("order_id")] public string? OrderId { get; set; }
    [JsonPropertyName("expiration")] public DateTime? Expiration { get; set; }
    [JsonPropertyName("created")] public DateTime? Created { get; set; }
    [JsonPropertyName("amount")] public double? Amount { get; set; }
    [JsonPropertyName("units")] public string? Units { get; set; }
    [JsonPropertyName("order_type")] public string? OrderType { get; set; }
}
