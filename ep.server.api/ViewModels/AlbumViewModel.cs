using ep.server.api.Models;
using System.Collections.Generic;

namespace ep.server.api.ViewModels
{
    public class AlbumViewModel
    {
        public int UserId { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public List<Photo> Photos { get; set; } = new();
    }
}
