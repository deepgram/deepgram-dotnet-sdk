// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Clients.Interfaces.v1;
using Deepgram.Models.Manage.v1;

namespace Deepgram;

/// <summary>
/// Current typed filters for legacy management client operations.
/// </summary>
public static class ManageClientExtensions
{
    public static Task<ProjectResponse> GetProject(this IManageClient client, string projectId, ProjectQuerySchema query,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        if (client is null)
        {
            throw new ArgumentNullException(nameof(client));
        }
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        var filters = addons is null ? new Dictionary<string, string>() : new Dictionary<string, string>(addons);
        if (query.Limit is not null) filters["limit"] = query.Limit.Value.ToString();
        if (query.Page is not null) filters["page"] = query.Page.Value.ToString();
        return client.GetProject(projectId, cancellationToken, filters, headers);
    }

    public static Task<KeysResponse> GetKeys(this IManageClient client, string projectId, KeysQuerySchema query,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        if (client is null)
        {
            throw new ArgumentNullException(nameof(client));
        }
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        var filters = addons is null ? new Dictionary<string, string>() : new Dictionary<string, string>(addons);
        if (!string.IsNullOrWhiteSpace(query.Status)) filters["status"] = query.Status!;
        return client.GetKeys(projectId, cancellationToken, filters, headers);
    }
}
