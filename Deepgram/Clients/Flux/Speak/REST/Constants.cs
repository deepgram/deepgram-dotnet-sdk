// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Clients.Flux.Speak.REST;

/// <summary>
/// (PREVIEW) Response headers of interest from the Deepgram Flux (v2 speak) batch REST API.
/// </summary>
// [verify] The exact /v2/speak response header set is not enumerated in the OpenAPI spec; these
// mirror /v1/speak (the batch handler reuses v1's audio-format machinery) and are confirmed or
// adjusted against the live staging endpoint during validation. The abstract client strips any
// "x-dg-"/"dg-" prefix and lowercases before matching, so these are the bare, lowercase names.
public static class Constants
{
    public const string ContentType = "content-type";
    public const string RequestId = "request-id";
    public const string ModelUUID = "model-uuid";
    public const string ModelName = "model-name";
    public const string CharCount = "char-count";
    public const string TransferEncoding = "transfer-encoding";
    public const string Date = "date";
}
