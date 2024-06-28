// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT


using Deepgram.Models.Authenticate.v1;
using OLD = Deepgram.Models.Live.v1;
using NEW = Deepgram.Models.Listen.v1.WebSocket;

using Deepgram.Clients.Interfaces.v1;
using WS = Deepgram.Clients.Listen.v1.WebSocket;
using System;

namespace Deepgram.Clients.Live.v1;

/// <summary>
// *********** WARNING ***********
// This class provides the Live Client implementation
//
// Deprecated: This class is deprecated. Use the `Deepgram.Clients.Listen.v1.WebSocket` namespace instead.
// This will be removed in a future release.
//
// This package is frozen and no new functionality will be added.
// *********** WARNING ***********
/// </summary>
[Obsolete("Please use Deepgram.Clients.Listen.v1.WebSocket.Client instead", false)]
public class Client : WS.Client, ILiveClient
{
    #region Fields
    private readonly SemaphoreSlim _mutexSubscribe = new SemaphoreSlim(1, 1);
    #endregion

    /// <param name="apiKey">Required DeepgramApiKey</param>
    /// <param name="deepgramClientOptions"><see cref="IDeepgramClientOptions"/> for HttpClient Configuration</param>
    public Client(string? apiKey = null, IDeepgramClientOptions? options = null) : base(apiKey, options)
    {
    }

    private static class EventHandlerOpenWrapper
    {
        public static EventHandler<NEW.OpenResponse> Wrap(EventHandler<OLD.OpenResponse> oldHandler)
        {
            return (sender, newArgs) =>
            {
                var oldArgs = new OLD.OpenResponse
                {
                    Type = newArgs.Type
                };

                oldHandler(sender, oldArgs);
            };
        }
    }

    // The private static classes below provide the ability to nest the EventHandlers between this legacy LiveClient and the
    // ListenWebSocketClient. This is needed because the EventHandler are templatized and are defined using a concrete class type.
    // Even though the older/obsolete response classes extend the newer response classes because they are concrete types this doesn't
    // really buy use anything when it comes to the EventHandlers.
    //
    // This means we need to translate the older/obsolete response classes into the new classes and chain the EventHandlers from the newer
    // ListenWebSocketClient to the legacy/obsolete LiveClient. In otherwords, since LiveClient extends ListenWebSocketClient the event coming out
    // are NEW.Response type. This needs to be converted to OLD.Response. To do this, we need to chain how the events are emitted.
    //
    // ListenWebSocketClient --emit NEW.Response--> Wrapper Class --emit OLD.Response--> User-Defined Callback is Notified
    private static class EventHandlerMetadataWrapper
    {
        public static EventHandler<NEW.MetadataResponse> Wrap(EventHandler<OLD.MetadataResponse> oldHandler)
        {
            return (sender, newArgs) =>
            {
                var oldArgs = new OLD.MetadataResponse
                {
                    Type = newArgs.Type,
                    Channels = newArgs.Channels,
                    Created = newArgs.Created,
                    Duration = newArgs.Duration,
                    ModelInfo = newArgs.ModelInfo,
                    Models = newArgs.Models,
                    RequestId = newArgs.RequestId,
                    Sha256 = newArgs.Sha256,
                    TransactionKey = newArgs.TransactionKey,
                    Extra = newArgs.Extra,
                };

                oldHandler(sender, oldArgs);
            };
        }
    }

    private static class EventHandlerResultWrapper
    {
        public static EventHandler<NEW.ResultResponse> Wrap(EventHandler<OLD.ResultResponse> oldHandler)
        {
            return (sender, newArgs) =>
            {
                var oldArgs = new OLD.ResultResponse
                {
                    Type = newArgs.Type,
                    Channel = newArgs.Channel,
                    ChannelIndex = newArgs.ChannelIndex,
                    Duration = newArgs.Duration,
                    IsFinal = newArgs.IsFinal,
                    MetaData = newArgs.MetaData,
                    SpeechFinal = newArgs.SpeechFinal,
                    Start = newArgs.Start,
                    Error = newArgs.Error,
                };

                oldHandler(sender, oldArgs);
            };
        }
    }

    private static class EventHandlerUtteranceEndWrapper
    {
        public static EventHandler<NEW.UtteranceEndResponse> Wrap(EventHandler<OLD.UtteranceEndResponse> oldHandler)
        {
            return (sender, newArgs) =>
            {
                var oldArgs = new OLD.UtteranceEndResponse
                {
                    Type = newArgs.Type,
                    Channel = newArgs.Channel,
                    LastWordEnd = newArgs.LastWordEnd,
                };

                oldHandler(sender, oldArgs);
            };
        }
    }

    private static class EventHandlerSpeechStartedWrapper
    {
        public static EventHandler<NEW.SpeechStartedResponse> Wrap(EventHandler<OLD.SpeechStartedResponse> oldHandler)
        {
            return (sender, newArgs) =>
            {
                var oldArgs = new OLD.SpeechStartedResponse
                {
                    Type = newArgs.Type,
                    Channel = newArgs.Channel,
                    Timestamp = newArgs.Timestamp,
                };

                oldHandler(sender, oldArgs);
            };
        }
    }

    private static class EventHandlerCloseWrapper
    {
        public static EventHandler<NEW.CloseResponse> Wrap(EventHandler<OLD.CloseResponse> oldHandler)
        {
            return (sender, newArgs) =>
            {
                var oldArgs = new OLD.CloseResponse
                {
                    Type = newArgs.Type
                };

                oldHandler(sender, oldArgs);
            };
        }
    }

    private static class EventHandlerUnhandledWrapper
    {
        public static EventHandler<NEW.UnhandledResponse> Wrap(EventHandler<OLD.UnhandledResponse> oldHandler)
        {
            return (sender, newArgs) =>
            {
                var oldArgs = new OLD.UnhandledResponse
                {
                    Type = newArgs.Type,
                    Raw = newArgs.Raw
                };

                oldHandler(sender, oldArgs);
            };
        }
    }

    private static class EventHandlerErrorWrapper
    {
        public static EventHandler<NEW.ErrorResponse> Wrap(EventHandler<OLD.ErrorResponse> oldHandler)
        {
            return (sender, newArgs) =>
            {
                var oldArgs = new OLD.ErrorResponse
                {
                    Type = newArgs.Type,
                    Description = newArgs.Description,
                    Message = newArgs.Message,
                    Variant = newArgs.Variant
                };

                oldHandler(sender, oldArgs);
            };
        }
    }

    #region Subscribe Event
    /// <summary>
    /// Subscribe to an Open event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<OLD.OpenResponse> eventHandler)
    {
        var ret = false;
        lock (_mutexSubscribe)
        {
            var wrappedHandler = EventHandlerOpenWrapper.Wrap(eventHandler);
            ret = base.Subscribe(wrappedHandler);
        }
        return ret;
    }

    /// <summary>
    /// Subscribe to a Metadata event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<OLD.MetadataResponse> eventHandler)
    {
        var ret = false;
        lock (_mutexSubscribe)
        {
            var wrappedHandler = EventHandlerMetadataWrapper.Wrap(eventHandler);
            ret = base.Subscribe(wrappedHandler);
        }
        return ret;
    }

    /// <summary>
    /// Subscribe to a Results event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<OLD.ResultResponse> eventHandler)
    {
        var ret = false;
        lock (_mutexSubscribe)
        {
            var wrappedHandler = EventHandlerResultWrapper.Wrap(eventHandler);
            ret = base.Subscribe(wrappedHandler);
        }
        return ret;
    }

    /// <summary>
    /// Subscribe to an UtteranceEnd event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<OLD.UtteranceEndResponse> eventHandler)
    {
        var ret = false;
        lock (_mutexSubscribe)
        {
            var wrappedHandler = EventHandlerUtteranceEndWrapper.Wrap(eventHandler);
            ret = base.Subscribe(wrappedHandler);
        }
        return ret;
    }

    /// <summary>
    /// Subscribe to a SpeechStarted event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<OLD.SpeechStartedResponse> eventHandler)
    {
        var ret = false;
        lock (_mutexSubscribe)
        {
            var wrappedHandler = EventHandlerSpeechStartedWrapper.Wrap(eventHandler);
            ret = base.Subscribe(wrappedHandler);
        }
        return ret;
    }

    /// <summary>
    /// Subscribe to a Close event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<OLD.CloseResponse> eventHandler)
    {
        var ret = false;
        lock (_mutexSubscribe)
        {
            var wrappedHandler = EventHandlerCloseWrapper.Wrap(eventHandler);
            ret = base.Subscribe(wrappedHandler);
        }
        return ret;
    }

    /// <summary>
    /// Subscribe to an Unhandled event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<OLD.UnhandledResponse> eventHandler)
    {
        var ret = false;
        lock (_mutexSubscribe)
        {
            var wrappedHandler = EventHandlerUnhandledWrapper.Wrap(eventHandler);
            ret = base.Subscribe(wrappedHandler);
        }
        return ret;
    }

    /// <summary>
    /// Subscribe to an Error event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<OLD.ErrorResponse> eventHandler)
    {
        var ret = false;
        lock (_mutexSubscribe)
        {
            var wrappedHandler = EventHandlerErrorWrapper.Wrap(eventHandler);
            ret = base.Subscribe(wrappedHandler);
        }
        return ret;
    }
    #endregion
}
