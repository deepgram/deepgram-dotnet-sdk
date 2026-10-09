// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Listen.v1.REST;

public class DiarizeInfo
{
    [JsonPropertyName("model_uuid")]
    public string? ModelUuid { get; set; }

    [JsonPropertyName("arch")]
    public string? Arch { get; set; }
}
