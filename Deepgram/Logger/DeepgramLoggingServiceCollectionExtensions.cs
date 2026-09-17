// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Logger;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency-injection helpers for wiring the Deepgram SDK's logging to a consumer-owned
/// <see cref="ILoggerFactory"/>.
/// </summary>
public static class DeepgramLoggingServiceCollectionExtensions
{
    /// <summary>
    /// Ensures logging services are registered so an <see cref="ILoggerFactory"/> is available
    /// for the SDK. Call <see cref="UseDeepgramLogging(IServiceProvider)"/> after building the
    /// provider to route SDK logs through that factory.
    /// </summary>
    public static IServiceCollection AddDeepgramLogging(this IServiceCollection services)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        services.AddLogging();
        return services;
    }

    /// <summary>
    /// Routes SDK logs through the <see cref="ILoggerFactory"/> resolved from
    /// <paramref name="provider"/>. Does nothing (and never throws) if the container has no
    /// logger factory registered. The SDK never reconfigures the consumer's global logging.
    /// </summary>
    public static IServiceProvider UseDeepgramLogging(this IServiceProvider provider)
    {
        if (provider == null) throw new ArgumentNullException(nameof(provider));

        var factory = (ILoggerFactory?)provider.GetService(typeof(ILoggerFactory));
        if (factory != null)
        {
            Log.Configure(factory);
        }

        return provider;
    }
}
