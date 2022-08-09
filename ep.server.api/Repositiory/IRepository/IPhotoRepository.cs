using ep.server.api.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ep.server.api.Repositiory.IRepository
{
    public interface IPhotoRepository
    {
        Task<List<Photo>> GetAllPhotosAsync(CancellationToken cancellationToken);
        Task<List<Photo>> GetAllPhotosByAlbumIdsAsync(List<int> ids, CancellationToken cancellationToken);
    }
}
