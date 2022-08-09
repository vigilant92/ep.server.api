using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ep.server.api.Servies
{
    public sealed class TelemetryInstance : IDisposable
    {
        // Regex to find chars that are invalid in https://httpwg.org/specs/rfc7230.html#rfc.section.3.2.6
        private static readonly Regex _invalidTokenChars = new Regex("[^&#\\$%&'\\*\\+\\-\\.\\^`\\|~\\w]");

        private readonly IServerTimings _serverMetrics;
        private readonly Activity _activity;
        private readonly ILogger<TelemetryInstance> _logger;
        private readonly Stopwatch _stopWatch;

        private long _previousTime = 0;

        public TelemetryInstance(IServerTimings serverMetrics, Activity activity, ILogger<TelemetryInstance> logger)
        {
            _serverMetrics = serverMetrics;
            _activity = activity;
            _logger = logger;
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
        }

        public string GetTraceId() => _activity.TraceId.ToString();

        public TelemetryInstance TimeElapsed(string name, string description = null)
            => SetTiming(name, _stopWatch.ElapsedMilliseconds, description);

        public TelemetryInstance TimeDelta(string name, string description = null)
        {
            var newTime = _stopWatch.ElapsedMilliseconds;
            var delta = newTime - _previousTime;
            _previousTime = newTime;
            return SetTiming(name, delta, description);
        }

        public TelemetryInstance SetTiming(string name, decimal? value, string description = null)
        {
            var cleanName = CleanName(name);

            _serverMetrics.SetTiming(cleanName, value, description);
            _logger?.LogInformation("timing name:{name}, value:{value}, description:{description}", cleanName, value, description);
            return SetTag(cleanName, value);
        }

        public TelemetryInstance SetTag(string key, object value)
        {
            _activity?.SetTag(key, value);
            return this;
        }

        public void SetStatus(Status status)
            => _activity?.SetStatus(status);

        public void RecordException(Exception ex)
            => _activity?.RecordException(ex);


        private string CleanName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                var ex = new ArgumentNullException(nameof(name), "timing/tag name must not be null");
                RecordException(ex);
                throw ex;
            }

            return _invalidTokenChars.Replace(name.Replace(' ', '-'), "");
        }

        public void Dispose()
        {
            _stopWatch?.Stop();
            _activity?.Dispose();
        }
    }
}
