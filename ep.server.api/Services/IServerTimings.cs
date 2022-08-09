namespace ep.server.api.Servies
{
    public interface IServerTimings
    {
        void SetTiming(string metricName, decimal? duration, string description = null);
        void SetTiming(string metricName, string description = null);
        string GetResponseString();
    }
}
