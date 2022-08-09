
using ep.server.api.Models;
using ep.server.api.Repositiory.IRepository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ep.server.api.Repositiory
{
    public class PhotoRepository : IPhotoRepository
    {

        private readonly HttpClient _namedClient;
        private readonly ILogger<PhotoRepository> _logger;
        public PhotoRepository(IHttpClientFactory factory, ILogger<PhotoRepository> logger)
        {
            _namedClient = factory.CreateClient("apiClient");
            _logger = logger;
        }

        public async Task<List<Photo>> GetAllPhotosAsync(CancellationToken cancellationToken)
        {
            try
            {
                var deserializedPhotos = await _namedClient.GetApiDataAsync<List<Photo>>("photos", cancellationToken);
                if (deserializedPhotos is null)
                    return null;
                _logger.LogInformation("Total Photos from response: {count}", deserializedPhotos.Count);
                return deserializedPhotos;

            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {error}", ex.Message);
                return null;

            }
        }

        public async Task<List<Photo>> GetAllPhotosByAlbumIdsAsync(List<int> ids, CancellationToken cancellationToken)
        {
            try
            {
                string queryParam = "";
                foreach (var id in ids)
                {
                    queryParam = queryParam + $"albumId={id}&";
                }

                var deserializedPhotos = await _namedClient.GetApiDataAsync<List<Photo>>("photos?" + queryParam, cancellationToken);
                if (deserializedPhotos is null)
                    return null;
                _logger.LogInformation("Total Photos from response: {count}", deserializedPhotos.Count);
                return deserializedPhotos;

            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {error}", ex.Message);
                return null;

            }
        }
    }
}
