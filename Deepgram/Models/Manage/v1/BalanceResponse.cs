// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Manage.v1;

public record BalanceResponse
{
    /// <summary>
    /// Balance id
    /// </summary>
    [JsonPropertyName("balance_id")]
    public string? BalanceId { get; set; }

    /// <summary>
    /// Balance amount
    /// </summary>
    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }

    /// <summary>
    /// Units of the balance. May use usd or hour, depending on the project billing settings
    /// </summary>
    [JsonPropertyName("units")]
    public string? Units { get; set; }

    /// <summary>
    /// Unique identifier of the purchase order associated with the balance
    /// </summary>
    [JsonPropertyName("purchase")]
    public string? Purchase { get; set; }
}
