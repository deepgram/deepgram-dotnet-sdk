// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Exceptions.v1;

public class DeepgramException : Exception
{
    public DeepgramException() : base()
    {
        ErrMsg = "Unknown DeepgramException";
        ErrCode = "Unknown Error Code";
        RequestId = "Unknown Request ID";
    }

    public DeepgramException(string errMsg) : base(errMsg)
    {
        ErrMsg = errMsg;
        ErrCode = "Unknown Error Code";
        RequestId = "Unknown Request ID";
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
}
