using ep.server.api.Models;
using ep.server.api.Repositiory.IRepository;
using ep.server.api.Servies;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ep.server.api.Controllers
{

    [Route("api/albums")]
    [ApiController]
    public class AlbumController : ControllerBase
    {
        private readonly IAlbumRepository _albumRepo;
        private readonly IPhotoRepository _photoRepo;
        private readonly ITelemetryService _telemetryService;
        public AlbumController(IAlbumRepository albumRepo, IPhotoRepository photoRepo, ITelemetryService telemetryService)
        {
            _albumRepo = albumRepo;
            _photoRepo = photoRepo;
            _telemetryService = telemetryService;
        }

        /// <summary>
        /// GetAllAlbums.
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        public async Task<IActionResult> GetAllAlbums(CancellationToken cancellationToken)
        {
            using var telemetryClient = _telemetryService.CreateInstance("Album List");

            var albums = await _albumRepo.GetAllAlbumsAsync(cancellationToken);

            if (albums is null)
            {
                return NotFound();
            }

            var photos = await _photoRepo.GetAllPhotosAsync(cancellationToken);

            telemetryClient.TimeDelta("Album.Get");

            //TODO pagination, as there is too much data.
            return Ok(albums.ToAlbumViewModels(photos));

        }


        /// <summary>
        /// GetAllAlbumsByUserId.
        /// </summary>
        /// <param name="userId">Album Filter</param>
        /// <returns></returns>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAllAlbumsByUserId(int userId, CancellationToken cancellationToken)
        {   
            using var telemetryClient = _telemetryService.CreateInstance("Album List");

            var albums = await _albumRepo.GetAlbumsByUserIdAsync(userId, cancellationToken);

            if (albums is null)
            {
                return NotFound();
            }

            var albumIds = albums.Select(x => x.Id).ToList();

            
            var photos = await _photoRepo.GetAllPhotosByAlbumIdsAsync(albumIds, cancellationToken);

            telemetryClient.TimeDelta("Album.Get");

            return Ok(albums.ToAlbumViewModels(photos));
        }
    }

}
