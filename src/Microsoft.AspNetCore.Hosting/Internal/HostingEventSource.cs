// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.Tracing;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Hosting.Internal
{
    [EventSource(Name = "Microsoft-AspNetCore-Hosting")]
    public sealed class HostingEventSource : EventSource
    {
        public static readonly HostingEventSource Log = new HostingEventSource();

        private HostingEventSource() { }

        [Event(1, Level = EventLevel.Informational)]
        public void HostStart()
        {
            WriteEvent(1);
        }

        [Event(2, Level = EventLevel.Informational)]
        public void HostStop()
        {
            WriteEvent(2);
        }

        [NonEvent]
        public void RequestStart(HttpContext context)
        {
            if (IsEnabled())
            {
                RequestStart(
                    context.TraceIdentifier,
                    context.Request.Protocol,
                    context.Request.Method,
                    context.Request.ContentType ?? string.Empty,
                    context.Request.ContentLength.HasValue ? context.Request.ContentLength.Value.ToString() : string.Empty,
                    context.Request.Scheme,
                    context.Request.Host.ToString(),
                    context.Request.PathBase,
                    context.Request.Path,
                    context.Request.QueryString.ToString());
            }
        }

        [NonEvent]
        public void RequestStop(HttpContext context, Exception exception = null)
        {
            if (IsEnabled())
            {
                RequestStop(
                    context.Response.StatusCode,
                    context.Response.ContentType ?? string.Empty,
                    context.TraceIdentifier,
                    exception == null ? string.Empty : exception.ToString());
            }
        }

        [Event(3, Level = EventLevel.Informational)]
        private void RequestStart(
            string requestTraceIdentifier,
            string protocol,
            string method,
            string contentType,
            string contentLength,
            string scheme,
            string host,
            string pathBase,
            string path,
            string queryString)
        {
            WriteEvent(
                3,
                requestTraceIdentifier,
                protocol,
                method,
                contentType,
                contentLength,
                scheme,
                host,
                pathBase,
                path,
                queryString);
        }

        [Event(4, Level = EventLevel.Informational)]
        private void RequestStop(int statusCode, string contentType, string requestTraceIdentifier, string exception)
        {
            WriteEvent(4, statusCode, contentType, requestTraceIdentifier, exception);
        }
    }
}
