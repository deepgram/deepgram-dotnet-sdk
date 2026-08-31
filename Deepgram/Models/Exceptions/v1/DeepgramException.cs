// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Exceptions.v1;

public class DeepgramException : Exception
{
    // Sentinels preserved from the original implementation; the Message override treats them
    // as "not provided" so they never mask a real error body.
    internal const string UnknownErrMsg = "Unknown DeepgramException";
    internal const string UnknownErrCode = "Unknown Error Code";
    internal const string UnknownRequestId = "Unknown Request ID";

    public DeepgramException() : base()
    {
        ErrMsg = UnknownErrMsg;
        ErrCode = UnknownErrCode;
        RequestId = UnknownRequestId;
    }

    public DeepgramException(string errMsg) : base(errMsg)
    {
        ErrMsg = errMsg;
        ErrCode = UnknownErrCode;
        RequestId = UnknownRequestId;
    }

    /// <summary>
    /// Error code
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("err_code")]
    public string ErrCode { get; set; }

    /// <summary>
    /// Error code
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("err_msg")]
    public string ErrMsg { get; set; }

    /// <summary>
    /// Error code
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("request_id")]
    public string RequestId { get; set; }

    /// <summary>
    /// Error category. Management endpoints (projects, keys, agents, agent-variables) return
    /// errors shaped {"category","message","details","request_id"} rather than
    /// {"err_code","err_msg","request_id"}.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    /// <summary>
    /// Human-readable error message from the management error shape.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("message")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Additional detail from the management error shape (e.g. which scope is missing).
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("details")]
    public string? Details { get; set; }

    /// <summary>
    /// Composes the most useful message available. Exceptions deserialized from an API error
    /// body carry the error in the properties above rather than the constructor message (which
    /// is empty on that path), so without this override a failed request surfaced as a bare
    /// exception type with no explanation.
    /// </summary>
    public override string Message
    {
        get
        {
            var text = !string.IsNullOrWhiteSpace(ErrorMessage) ? ErrorMessage!
                : !string.IsNullOrWhiteSpace(ErrMsg) && ErrMsg != UnknownErrMsg ? ErrMsg
                : base.Message;

            var prefix = !string.IsNullOrWhiteSpace(Category) ? Category
                : !string.IsNullOrWhiteSpace(ErrCode) && ErrCode != UnknownErrCode ? ErrCode
                : null;
            if (prefix != null)
            {
                text = string.IsNullOrEmpty(text) ? prefix : $"{prefix}: {text}";
            }

            if (!string.IsNullOrWhiteSpace(Details))
            {
                text = string.IsNullOrEmpty(text) ? Details! : $"{text} {Details}";
            }

            if (!string.IsNullOrWhiteSpace(RequestId) && RequestId != UnknownRequestId)
            {
                text = $"{text} (request_id: {RequestId})";
            }

            return text;
        }
    }
}
