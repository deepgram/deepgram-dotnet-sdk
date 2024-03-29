﻿// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Manage.v1;

namespace Deepgram.Logger;
internal static partial class Log
{
    [LoggerMessage(
        EventId = 0002,
        Level = LogLevel.Error,
        Message = "`{requestType}` threw exception ")]
    internal static partial void Exception(this ILogger logger, string requestType, Exception ex);

    [LoggerMessage(
        EventId = 0005,
        Level = LogLevel.Information,
        Message = "Whilst creating RestClient there was no ApiKey ")]
    internal static partial void ApiKeyNotPresent(this ILogger logger);


    [LoggerMessage(
        EventId = 0006,
        Level = LogLevel.Warning,
        Message = "Trying to send message when the socket is `{state}`. Ack for this message will fail shortly.")]
    internal static partial void LiveSendWarning(this ILogger logger, WebSocketState state);


    [LoggerMessage(
       EventId = 0009,
       Level = LogLevel.Error,
       Message = "Unable to perform `{action}` Socket disposed")]
    internal static partial void SocketDisposed(this ILogger logger, string action, Exception ex);

    [LoggerMessage(
  EventId = 0014,
  Level = LogLevel.Error,
  Message = "Closing WebSocket")]
    internal static partial void ClosingSocket(this ILogger logger);

    [LoggerMessage(
      EventId = 0016,
      Level = LogLevel.Information,
      Message = "WebSocket is being closed : `{closeStatusDescription}`")]
    internal static partial void RequestedSocketClose(this ILogger logger, string closeStatusDescription);

    [LoggerMessage(
    EventId = 0018,
    Level = LogLevel.Error,
    Message = "Error creating project key both ExpirationDate and TimeToLiveInSeconds are set: `{createProjectKeySchema}`")]
    internal static partial void CreateKeyError(this ILogger logger, KeySchema createProjectKeySchema);
}
