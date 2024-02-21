// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Runtime.Serialization;

namespace Deepgram.Constants;

[DataContract]
public enum WarningType

{
    [EnumMember(Value = "unsupported_language")]
    UnsupportedLanguage,

    [EnumMember(Value = "unsupported_model")]
    UnsupportedModel,

    [EnumMember(Value = "unsupported_encoding")]
    UnsupportedEncoding,

    [EnumMember(Value = "deprecated")]
    Deprecated
}
