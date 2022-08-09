using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;

namespace ep.server.api.Servies
{
    public class ServerTimings : IServerTimings
    {
        private ConcurrentDictionary<string, Metric> _metrics { get; } = new ConcurrentDictionary<string, Metric>();

        /// <summary>
        /// Add metric with a description but no value (sneaky way to get key info to the browser timings)
        /// </summary>
        /// <param name="metricName"></param>
        /// <param name="description"></param>
        public void SetTiming(string metricName, string description = null)
            => SetTiming(metricName, null, description);

        /// <summary>
        /// Add a metric with a duration and optional description
        /// </summary>
        public void SetTiming(string metricName, decimal? duration, string description = null)
        {
            var metric = new Metric(metricName, duration, description);
            _metrics.AddOrUpdate(metricName, metric, (m, v) => metric);
        }

        /// <summary>
        /// Get a string formatted for attaching to a servier timeing response.
        /// </summary>
        public string GetResponseString()
            => string.Join(",", _metrics.Values.OrderBy(x => x.Name)).Trim();

        private struct Metric
        {
            public string Name { get; }
            public decimal? Value { get; }
            public string Description { get; }
            public Metric(string name, decimal? value, string description)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
                Value = value;
                Description = description;
            }

            public override string ToString()
            {
                var duration = Value.HasValue ? $";dur={Value.Value.ToString(CultureInfo.InvariantCulture)}" : null;
                var desc = !string.IsNullOrWhiteSpace(Description) ? $";desc=\"{Description}\"" : null;
                return $"{Name}{duration}{desc}".Trim();
            }
        }
    }
}
