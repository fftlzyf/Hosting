// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.Tracing;
using System.Reflection;
using Xunit;

namespace Microsoft.AspNetCore.Hosting.Internal
{
    public class HostingEventSourceTests
    {
        [Fact]
        public void MatchesNameAndGuid()
        {
            // Arrange & Act
            var eventSourceType = typeof(WebHost).GetTypeInfo().Assembly.GetType(
                "Microsoft.AspNetCore.Hosting.Internal.HostingEventSource",
                throwOnError: true,
                ignoreCase: false);

            // Assert
            Assert.NotNull(eventSourceType);
            Assert.Equal("Microsoft-AspNetCore-Hosting", EventSource.GetName(eventSourceType));
            Assert.Equal(Guid.Parse("9e620d2a-55d4-5ade-deb7-c26046d245a8"), EventSource.GetGuid(eventSourceType));
            Assert.NotEmpty(EventSource.GenerateManifest(eventSourceType, "assemblyPathToIncludeInManifest"));
        }
    }
}
