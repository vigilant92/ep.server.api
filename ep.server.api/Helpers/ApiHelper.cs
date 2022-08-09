using Newtonsoft.Json;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ep.server.api
{
    public static class ApiHelper
    {
        public static async Task<T> GetApiDataAsync<T>(this HttpClient client, string path, CancellationToken cancellationToken)
        {
            var output = client.GetAsync(path, cancellationToken).Result;
            if (output.IsSuccessStatusCode)
            {
                var allText = await output.Content.ReadAsStringAsync(cancellationToken);
                if (allText is not null)

                {
                    var fields = JsonConvert.DeserializeObject<T>(allText);

                    return fields;
                }
            }

            return default(T);
        }
    }
}
