// Copyright 2021-2023 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Analyze.v1;

public record AsyncResponse
{
    /// <summary>
    /// Id of transcription request returned when
    /// a CallBack has been supplied in request
    /// </summary>
    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }
}

