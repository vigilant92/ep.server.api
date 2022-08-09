using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace ep.server.api.Servies
{
    /// <summary>
    /// We need the telemetry service to run inside a typical scoped operation (normally a request/response cycle or a hosted processing execution).
    /// This can't be a singleton because we depend on server metrics which is scoped for the same reason.  
    /// We also need the scoped service provider for the same reason, resolving things from the root provider can be problematic.
    /// Note: this doesn't affect app wide metrics, only the req/resp metrics.
    /// </summary>
    public class TelemetryService : ITelemetryService
    {
        private readonly ActivitySource _activitySource;
        private readonly IServiceProvider _serviceProvider;


        public TelemetryService(TelemetryActivitySource tas, IServiceProvider serviceProvider)
        {
            _activitySource = tas.Source;
            _serviceProvider = serviceProvider;
        }


        public TelemetryInstance CreateInstance(string description, ActivityKind activityKind = ActivityKind.Internal, bool writeToLog = false)
        {
            var metrics = _serviceProvider.GetRequiredService<IServerTimings>();
            var activity = _activitySource.StartActivity(description, activityKind);
            var logger = writeToLog ? _serviceProvider.GetRequiredService<ILogger<TelemetryInstance>>() : null;

            return new TelemetryInstance(metrics, activity, logger);
        }

    }
}