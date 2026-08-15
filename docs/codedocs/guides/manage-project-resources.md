---
title: "Manage Project Resources"
description: "List projects, rotate keys, and manage self-hosted credentials with the management clients."
---

This guide combines the patterns spread across `examples/manage/projects/Program.cs`, `examples/manage/keys/Program.cs`, and the self-hosted client implementation. The goal is to show one realistic admin workflow: locate a project, create a scoped key, and then inspect self-hosted credentials for that same project.

<Steps>
<Step>
### List projects and pick one

```csharp
using Deepgram;
using Deepgram.Models.Manage.v1;

Library.Initialize();

var manage = ClientFactory.CreateManageClient();
var projects = await manage.GetProjects();

var project = projects.Projects!.First();
Console.WriteLine($"{project.Name} -> {project.ProjectId}");
```

</Step>
<Step>
### Create a scoped API key

```csharp
var key = await manage.CreateKey(
    project.ProjectId,
    new KeySchema
    {
        Comment = "CI key for nightly transcription jobs",
        Scopes = new List<string> { "usage:read", "member:read" },
        Tags = new List<string> { "ci", "nightly" },
        TimeToLiveInSeconds = 86400
    });

Console.WriteLine(key);
```

</Step>
<Step>
### Inspect self-hosted credentials for the same project

```csharp
using Deepgram.Models.Authenticate.v1;

var selfHosted = ClientFactory.CreateSelfHostedClient(
    options: new DeepgramHttpClientOptions(onPrem: false));

var credentials = await selfHosted.ListCredentials(project.ProjectId);
Console.WriteLine(credentials);

Library.Terminate();
```

</Step>
</Steps>

Important details from the SDK:

- `ManageClient.CreateKey` throws if you set both `ExpirationDate` and `TimeToLiveInSeconds`.
- Management APIs and self-hosted credential APIs use different clients, but they share the same REST abstraction and auth model.
- Per-request `headers` and `addons` are available on every method if your admin workflow needs custom metadata.

This workflow is intentionally split across two clients because the repository separates general project administration from self-hosted credential operations. That separation is useful in larger codebases: you can keep management-only permissions in one service while placing self-hosted credential operations behind a smaller operational boundary.

If you only need reporting, the same `ManageClient` also exposes `GetUsageRequests`, `GetUsageSummary`, `GetUsageFields`, `GetBalances`, and `GetBalance`. Those methods follow the same parameter conventions shown here.

For production automation, a common pattern is:

- use `GetProjects` once during startup to resolve project IDs by name
- mint or rotate short-lived keys with `CreateKey`
- collect reporting through usage or balance methods on a schedule

That keeps your operational code close to the SDK's intended client boundaries instead of mixing administrative endpoints into your speech-processing paths.
