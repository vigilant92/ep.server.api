using ep.server.api.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace ep.server.api.Models
{
    public static class AlbumMapper
    {
        public static List<AlbumViewModel> ToAlbumViewModels(this List<Album> albums, List<Photo> photos)
        {
            var albumViewModels = new List<AlbumViewModel>();

            foreach (var album in albums)
            {
                var albumViewModel = new AlbumViewModel()
                {
                    Id = album.Id,
                    Title = album.Title,
                    UserId = album.UserId,
                };

                if (photos != null && photos.Select(x => x.AlbumId == album.Id).Any())
                {
                    albumViewModel.Photos.AddRange(photos.Where(x => x.AlbumId == album.Id).ToList());
                }

                albumViewModels.Add(albumViewModel);
            }
            
            return albumViewModels;
        }
    }
}
