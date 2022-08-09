using System.Diagnostics;

namespace ep.server.api.Servies
{
    /// <summary>
    /// We want to declare a single activity source for the application and as it is dynamic we do it by
    /// storing a singleton with the name/source 
    /// </summary>
    public class TelemetryActivitySource
    {
        public TelemetryActivitySource(string sourceName) => Source = new ActivitySource(sourceName);
        public ActivitySource Source { get; internal set; }
    }
}