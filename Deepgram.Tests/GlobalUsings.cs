// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

global using System.Net;
global using System.Text;
global using System.Text.Json;
global using System.Web;
global using AutoBogus;
global using Bogus;
global using Deepgram.Abstractions;
global using Deepgram.Constants;
global using Deepgram.Extensions;
global using Deepgram.Tests.Fakes;
global using Deepgram.Utilities;
global using FluentAssertions;
global using FluentAssertions.Execution;
global using Microsoft.Extensions.DependencyInjection;
global using NSubstitute;
global using NUnit.Framework;
