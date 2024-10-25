// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Abstractions.v2;

/// <summary>
/// LocalFileWithMetadata is a class that represents a file with metadata.
/// </summary>
public class LocalFileWithMetadata :  IDisposable
{
    /// <summary>
    /// Gets or sets the metadata associated with the file content.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; }

    /// <summary>
    /// Gets or sets the file content as a MemoryStream.
    /// The caller is responsible for disposing of this stream when no longer needed.
    /// </summary>
    /// <remarks>
    /// This property should be properly disposed of to prevent memory leaks.
    /// </remarks>
    public MemoryStream Content { get; set; }

    /// <summary>
    /// Releases the resources used by the Content stream.
    /// </summary>
    public void Dispose()
    {
        Content?.Dispose();
    }
}
