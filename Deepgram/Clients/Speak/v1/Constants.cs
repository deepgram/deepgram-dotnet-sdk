// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Clients.Speak.v1;

/// <summary>
// *********** WARNING ***********
// This package provides Constants for the Speak Client for the Deepgram API
//
// Deprecated: This class is deprecated. Use the namespace `Deepgram.Clients.Speak.v1.REST` instead.
// This will be removed in a future release.
//
// This class is frozen and no new functionality will be added.
// *********** WARNING ***********
/// </summary>
[Obsolete("Please use Deepgram.Clients.Speak.v1.REST.Constants instead", false)]
public static class Constants
{
    // For Speak Headers
    public const string ContentType = "content-type";
    public const string RequestId = "request-id";
    public const string ModelUUID = "model-uuid";
    public const string ModelName = "model-name";
    public const string CharCount = "char-count";
    public const string TransferEncoding = "transfer-encoding";
    public const string Date = "date";
}

