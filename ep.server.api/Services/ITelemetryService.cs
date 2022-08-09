using System.Diagnostics;

namespace ep.server.api.Servies
{
    public interface ITelemetryService
    {
        TelemetryInstance CreateInstance(string description, ActivityKind activityKind = ActivityKind.Internal, bool writeToLog = false);

    }
}
