// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Abstractions;

/// <summary>
/// LocalFileWithMetadata is a class that represents a file with metadata.
/// </summary>
public class LocalFileWithMetadata
{
    public Dictionary<string, string> Metadata { get; set; }

    public MemoryStream Content { get; set; }
}
