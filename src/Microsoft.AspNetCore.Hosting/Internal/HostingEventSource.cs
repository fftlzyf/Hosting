// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Diagnostics.Tracing;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Hosting.Internal
{
    [EventSource(Name = "Microsoft-AspNetCore-Hosting")]
    public sealed class HostingEventSource : EventSource
    {
        public static readonly HostingEventSource Log = new HostingEventSource();

        private HostingEventSource() { }

        // The 'Start' and 'Stop' suffixes on the following event names have special meaning in EventSource. They
        // enable creating 'activities'.
        // For more information, take a look at the following blog post:
        // https://blogs.msdn.microsoft.com/vancem/2015/09/14/exploring-eventsource-activity-correlation-and-causation-features/

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
                    context.Request.Host.Value,
                    context.Request.PathBase,
                    context.Request.Path,
                    context.Request.QueryString.Value);
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
