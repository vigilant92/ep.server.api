using ep.server.api.Servies;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ep.server.api.Services
{
    public class ServerTimingMiddleware
    {
        private const string TRACE_ID = "Trace-Id";
        private const string SERVER_TIMING = "Server-Timing";
        private const string TIMING_ALLOW_ORIGIN = "Timing-Allow-Origin";
        private const string ACCEPT_TRANSFER_ENCODING = "TE";
        private const string ACCEPT_TRAILERS = "trailers";

        private readonly RequestDelegate _next;
        private readonly string _timingAllowOriginHeaderValue;

        public ServerTimingMiddleware(RequestDelegate next, string timingAllowOrigin)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _timingAllowOriginHeaderValue = timingAllowOrigin;
        }

        public async Task Invoke(HttpContext context, IServerTimings serverTimings, ITelemetryService telemetryService)
        {
            // something not right, there should be a scoped timing service
            if (serverTimings == null)
                throw new ArgumentNullException(nameof(serverTimings));

            using var telemetryClient = telemetryService.CreateInstance(context.Request.Path);
            telemetryClient.TimeDelta("request");

            // we are restricting the origins so set the header so the upstream browser and other message inspectors know
            if (!string.IsNullOrWhiteSpace(_timingAllowOriginHeaderValue))
                SetResponseHeader(context.Response, TIMING_ALLOW_ORIGIN, _timingAllowOriginHeaderValue);

            // see if we can use response trailers and if we can declare the trailer and then wait until we are on the way
            // back to set the metrics (make sure to exit!)
            var useTrailers = context.Request.Headers.ContainsKey(ACCEPT_TRANSFER_ENCODING) &&
                              context.Request.Headers[ACCEPT_TRANSFER_ENCODING].Contains(ACCEPT_TRAILERS) &&
                              context.Response.SupportsTrailers();

            if (useTrailers)
            {
                context.Response.DeclareTrailer(TRACE_ID);
                context.Response.DeclareTrailer(SERVER_TIMING);
                await _next(context);
                telemetryClient.TimeDelta("response");

                context.Response.AppendTrailer(TRACE_ID, telemetryClient.GetTraceId());
                context.Response.AppendTrailer(SERVER_TIMING, serverTimings.GetResponseString());
                return;
            }

            // we can't use trailers so hook the on response starting so that we only write the metrics at the last possible second
            // and then hand off to the next middleware.
            context.Response.OnStarting(() =>
            {
                SetResponseHeader(context.Response, TRACE_ID, ""); //TODO getting null always
                SetResponseHeader(context.Response, SERVER_TIMING, serverTimings.GetResponseString());
                telemetryClient.TimeDelta("response");
                return Task.CompletedTask;
            });

            await _next(context);
            telemetryClient.TimeDelta("response");
        }

        private static void SetResponseHeader(HttpResponse response, string headerName, string headerValue)
        {
            if (string.IsNullOrWhiteSpace(headerValue))
                return;

            if (response.Headers.ContainsKey(headerName))
            {
                response.Headers[headerName] = headerValue;
                return;
            }

            response.Headers.Append(headerName, headerValue);
        }
    }
}