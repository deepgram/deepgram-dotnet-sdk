// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Listen.v2.WebSocket;

/// <summary>
/// Updates selected settings on an open <c>/v1/listen</c> stream. Keyterm updates work on Nova-3
/// streams on <c>api.deepgram.com</c>. They are not yet available on the EU, Australia, or India
/// regional hosts, and as of October 2026 the EU host accepts a keyterm update without an Error, so
/// do not rely on an Error to detect an unsupported host. A successful update has no
/// acknowledgement. Most rejections arrive as an Error response with a <c>Variant</c> and, when one
/// applies, a <c>Code</c>, for example <c>InvalidConfigureMessage</c> /
/// <c>KeytermsNotSupported</c> (keyterms on a model other than Nova-3); the stream keeps its
/// previous settings. Keep each keyterm update under the 500-token limit and check the list size
/// before sending; the SDK does not check it. As of October 2026, an over-limit update does not
/// return an Error: transcription stops, and the server closes the stream with
/// <c>1011 (NET-0000)</c> after about 30 seconds.
/// </summary>
public class ConfigureSchema
{
    [JsonPropertyName("type")]
    public string Type { get; } = "Configure";

    /// <summary>
    /// Replaces the current keyterm list. An empty list clears keyterms; <c>null</c> leaves them unchanged.
    /// Keep each update under the 500-token keyterm limit.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("keyterms")]
    public List<string>? Keyterms { get; set; }

    /// <summary>
    /// Updates supported formatting features, such as <c>numerals</c>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("features")]
    public Dictionary<string, bool>? Features { get; set; }

    public override string ToString() => JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
}
