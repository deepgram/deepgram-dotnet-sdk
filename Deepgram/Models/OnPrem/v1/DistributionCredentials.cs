// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using SH = Deepgram.Models.SelfHosted.v1;

namespace Deepgram.Models.OnPrem.v1;

/// <summary>
// *********** WARNING ***********
// This class provides the DistributionCredentials implementation
//
// Deprecated: This class is deprecated. Use the `Deepgram.Models.SelfHosted.v1` namespace instead.
// This will be removed in a future release.
//
// This package is frozen and no new functionality will be added.
// *********** WARNING ***********
/// </summary>
[Obsolete("Please use Deepgram.Models.SelfHosted.v1.DistributionCredentials instead", false)]
public record DistributionCredentials :  SH.DistributionCredentials
{
}
