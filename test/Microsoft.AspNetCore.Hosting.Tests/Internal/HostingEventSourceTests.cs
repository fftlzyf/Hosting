// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Reflection;
using Microsoft.AspNetCore.Http;
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

        [Fact]
        public void HostStart()
        {
            // Arrange
            var eventListener = new TestEventListener();
            var hostingEventSource = HostingEventSource.Log;
            eventListener.EnableEvents(hostingEventSource, EventLevel.Informational);

            // Act
            hostingEventSource.HostStart();

            // Assert
            var eventData = eventListener.EventData;
            Assert.NotNull(eventData);
            Assert.Equal(1, eventData.EventId);
#if NETCOREAPP1_1
            Assert.Equal("HostStart", eventData.EventName);
#endif
            Assert.Equal(EventLevel.Informational, eventData.Level);
            Assert.Same(hostingEventSource, eventData.EventSource);
            Assert.Null(eventData.Message);
            Assert.Empty(eventData.Payload);
        }

        [Fact]
        public void HostStop()
        {
            // Arrange
            var eventListener = new TestEventListener();
            var hostingEventSource = HostingEventSource.Log;
            eventListener.EnableEvents(hostingEventSource, EventLevel.Informational);

            // Act
            hostingEventSource.HostStop();

            // Assert
            var eventData = eventListener.EventData;
            Assert.NotNull(eventData);
            Assert.Equal(2, eventData.EventId);
#if NETCOREAPP1_1
            Assert.Equal("HostStop", eventData.EventName);
#endif
            Assert.Equal(EventLevel.Informational, eventData.Level);
            Assert.Same(hostingEventSource, eventData.EventSource);
            Assert.Null(eventData.Message);
            Assert.Empty(eventData.Payload);
        }

        public static TheoryData<DefaultHttpContext, string[]> RequestStartData
        {
            get
            {
                var variations = new TheoryData<DefaultHttpContext, string[]>();

                var context = new DefaultHttpContext();
                context.TraceIdentifier = "trace-identifier";
                context.Request.Protocol = "HTTP/1.1";
                context.Request.Method = "GET";
                context.Request.ContentType = null;
                context.Request.ContentLength = null;
                context.Request.Scheme = "https";
                context.Request.Host = new HostString("localhost:5000");
                context.Request.PathBase = "";
                context.Request.Path = "/Home/Index";
                context.Request.QueryString = new QueryString();
                variations.Add(
                    context,
                    new string[]
                    {
                        "trace-identifier",
                        "HTTP/1.1",
                        "GET",
                        string.Empty, // content-type
                        string.Empty, // content-length
                        "https",
                        "localhost:5000",
                        string.Empty, // pathbase
                        "/Home/Index",
                        string.Empty // query string
                    });

                context = new DefaultHttpContext();
                context.TraceIdentifier = "trace-identifier";
                context.Request.Protocol = "HTTP/1.0";
                context.Request.Method = "POST";
                context.Request.ContentType = "application/json; charset=utf-8";
                context.Request.ContentLength = 100;
                context.Request.Scheme = "http";
                context.Request.Host = new HostString("localhost");
                context.Request.PathBase = "/vdir1";
                context.Request.Path = "/Home/Index";
                context.Request.QueryString = new QueryString("?p1=p1-value");
                variations.Add(
                    context,
                    new string[]
                    {
                        "trace-identifier",
                        "HTTP/1.0",
                        "POST",
                        "application/json; charset=utf-8",
                        "100",
                        "http",
                        "localhost",
                        "/vdir1",
                        "/Home/Index",
                        "?p1=p1-value"
                    });

                return variations;
            }
        }

        [Theory]
        [MemberData(nameof(RequestStartData))]
        public void RequestStart(DefaultHttpContext httpContext, string[] expected)
        {
            // Arrange
            var eventListener = new TestEventListener();
            var hostingEventSource = HostingEventSource.Log;
            eventListener.EnableEvents(hostingEventSource, EventLevel.Informational);

            // Act
            hostingEventSource.RequestStart(httpContext);

            // Assert
            var eventData = eventListener.EventData;
            Assert.NotNull(eventData);
            Assert.Equal(3, eventData.EventId);
#if NETCOREAPP1_1
            Assert.Equal("RequestStart", eventData.EventName);
#endif
            Assert.Equal(EventLevel.Informational, eventData.Level);
            Assert.Same(hostingEventSource, eventData.EventSource);
            Assert.Null(eventData.Message);

            var payloadList = eventData.Payload;
            Assert.Equal(expected.Length, payloadList.Count);
            for (var i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], payloadList[i]);
            }
        }

        public static TheoryData<DefaultHttpContext, Exception, object[]> RequestStopData
        {
            get
            {
                var variations = new TheoryData<DefaultHttpContext, Exception, object[]>();

                var context = new DefaultHttpContext();
                context.TraceIdentifier = "trace-identifier";
                context.Response.StatusCode = 200;
                context.Response.ContentType = null;
                variations.Add(
                    context,
                    null,                 // exception
                    new object[]
                    {
                        200,
                        string.Empty,    // content-type
                        "trace-identifier",
                        string.Empty    // exception
                    });

                context = new DefaultHttpContext();
                context.TraceIdentifier = "trace-identifier";
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json; charset=utf-8";
                variations.Add(
                    context,
                    null,   // exception
                    new object[]
                    {
                        200,
                        "application/json; charset=utf-8",
                        "trace-identifier",
                        string.Empty    // exception
                    });

                context = new DefaultHttpContext();
                context.TraceIdentifier = "trace-identifier";
                context.Response.StatusCode = 500;
                context.Response.ContentType = null;
                var exception = GetException();
                var exceptionString = exception.ToString();
                variations.Add(
                    context,
                    exception,
                    new object[]
                    {
                        500,
                        string.Empty,   // content-type
                        "trace-identifier",
                        exceptionString
                    });

                return variations;
            }
        }

        [Theory]
        [MemberData(nameof(RequestStopData))]
        public void RequestStop(DefaultHttpContext httpContext, Exception exception, object[] expected)
        {
            // Arrange
            var eventListener = new TestEventListener();
            var hostingEventSource = HostingEventSource.Log;
            eventListener.EnableEvents(hostingEventSource, EventLevel.Informational);

            // Act
            hostingEventSource.RequestStop(httpContext, exception);

            // Assert
            var eventData = eventListener.EventData;
            Assert.Equal(4, eventData.EventId);
#if NETCOREAPP1_1
            Assert.Equal("RequestStop", eventData.EventName);
#endif
            Assert.Equal(EventLevel.Informational, eventData.Level);
            Assert.Same(hostingEventSource, eventData.EventSource);
            Assert.Null(eventData.Message);

            var payloadList = eventData.Payload;
            Assert.Equal(expected.Length, payloadList.Count);
            for (var i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], payloadList[i]);
            }
        }

        private static Exception GetException()
        {
            try
            {
                throw new InvalidOperationException("An invalid operation has occurred");
            }
            catch(Exception ex)
            {
                return ex;
            }
        }
        private class TestEventListener : EventListener
        {
            public EventWrittenEventArgs EventData { get; private set; }

            protected override void OnEventWritten(EventWrittenEventArgs eventData)
            {
                EventData = eventData;
            }
        }
    }
}
