// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Text.Encodings.Web;

namespace Deepgram.Utilities;

internal class JsonSerializeOptions
{
    public static JsonSerializerOptions DefaultOptions => new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        // Use the relaxed encoder so characters like '+', '&', '<', '>' and non-ASCII
        // are emitted literally instead of as \uXXXX escapes. This keeps payloads readable
        // while still producing valid JSON (quotes and backslashes remain escaped). It
        // replaces the previous Regex.Unescape(...) hack that corrupted payloads containing
        // quotation marks or backslashes. See issues #355, #393.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
}
