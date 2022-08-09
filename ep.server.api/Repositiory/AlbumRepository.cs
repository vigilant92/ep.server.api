
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
    public class AlbumRepository : IAlbumRepository
    {

        private readonly HttpClient _namedClient;
        private readonly ILogger<AlbumRepository> _logger;
        public AlbumRepository(IHttpClientFactory factory, ILogger<AlbumRepository> logger)
        {
            _namedClient = factory.CreateClient("apiClient");
            _logger = logger;
        }

        public async Task<List<Album>> GetAllAlbumsAsync(CancellationToken cancellationToken)
        {
            try
            {
                var deserializedAlbums = await _namedClient.GetApiDataAsync<List<Album>>("albums", cancellationToken);
                if (deserializedAlbums is null || deserializedAlbums.Count == 0)
                    return null;
                _logger.LogInformation("Total Albums from response: {count}", deserializedAlbums.Count);
                return deserializedAlbums;

            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {error}", ex.Message);
                return null;

            }
        }

        public async Task<List<Album>> GetAlbumsByUserIdAsync(int userId, CancellationToken cancellationToken)
        {
            try
            {
                var deserializedAlbums = await _namedClient.GetApiDataAsync<List<Album>>($"albums?userId={userId}", cancellationToken);
                if (deserializedAlbums is null || deserializedAlbums.Count == 0)
                    return null;
                _logger.LogInformation("Total Albums from response: {count}", deserializedAlbums.Count);
                return deserializedAlbums;

            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {error}", ex.Message);
                return null;
            }
        }

    }
}
