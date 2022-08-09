using ep.server.api.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ep.server.api.Repositiory.IRepository
{
    public interface IAlbumRepository
    {
        Task<List<Album>> GetAllAlbumsAsync(CancellationToken cancellationToken);
        Task<List<Album>> GetAlbumsByUserIdAsync(int userId, CancellationToken cancellationToken);
    }
}
