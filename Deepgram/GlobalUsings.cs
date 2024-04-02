// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

global using System.Collections.Concurrent;
global using System.Net.Http.Headers;
global using System.Net.WebSockets;
global using System.Reflection;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Text.RegularExpressions;
global using System.Threading.Channels;
global using System.Web;
global using Deepgram.Abstractions;
global using Deepgram.Constants;
global using Deepgram.Logger;
global using Deepgram.Utilities;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Polly;
global using Polly.Contrib.WaitAndRetry;
