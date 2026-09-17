// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using NUnit.Framework;

// Intentionally outside any namespace: a namespace-less [SetUpFixture] runs once before ALL
// tests in the assembly.
//
// Several authentication tests (e.g. BearerTokenTests) call Environment.SetEnvironmentVariable
// to exercise credential-fallback behavior and leave DEEPGRAM_API_KEY cleared for the rest of
// the process. Tests that legitimately need the real key (the live Flux integration test) must
// therefore read this startup snapshot instead of the live environment.
[SetUpFixture]
public class GlobalTestEnvironment
{
    public static string? DeepgramApiKeyAtStartup { get; private set; }

    [OneTimeSetUp]
    public void SnapshotEnvironment()
    {
        DeepgramApiKeyAtStartup = Environment.GetEnvironmentVariable("DEEPGRAM_API_KEY");
    }
}
