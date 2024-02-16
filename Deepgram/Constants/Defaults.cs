// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Constants;

public static class Defaults
{
    // Deepgram specific consts
    public const string DEFAULT_URI = "api.deepgram.com";
    public const string DEEPGRAM_API_KEY = "DEEPGRAM_API_KEY";

    // HTTP specific consts
    public const string DEFAULT_CONTENT_TYPE = "application/json";
    public const int DEFAULT_HTTP_TINEOUT_IN_MINUTES = 5;

    // Client Names
    public const string HTTPCLIENT_NAME = "DEEPGRAM_HTTP_CLIENT";
    public const string WSSCLIENT_NAME = "DEEPGRAM_WSS_CLIENT";
}
