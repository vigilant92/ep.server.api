using ep.server.api.Controllers;
using ep.server.api.Repositiory;
using ep.server.api.Servies;
using ep.server.api.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ep.server.api.test
{
    public class AlbumControllerTest
    {
        private Mock<ILogger<AlbumRepository>> mockAlbumRepoLogger;
        private Mock<ILogger<PhotoRepository>> mockPhotoRepoLogger;
        private AlbumRepository albumRepo;
        private PhotoRepository photoRepo;
        private Mock<HttpMessageHandler> albumHandlerMock;
        private Mock<HttpMessageHandler> photoHandlerMock;
        private HttpClient photoHttpClient;
        private HttpClient albumHttpClient;
        private Mock<ITelemetryService> mockTelemetryService;

        [SetUp]
        public void Setup()
        {
            mockAlbumRepoLogger = new Mock<ILogger<AlbumRepository>>();
            mockPhotoRepoLogger = new Mock<ILogger<PhotoRepository>>();
            var mockPhotoFactory = new Mock<IHttpClientFactory>();
            photoHandlerMock = new Mock<HttpMessageHandler>();
            var photoResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(TestHelper.GetFileContents("Data.AllPhotos.txt"))
            };

            photoHandlerMock.Protected()
              .Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                 ItExpr.IsAny<HttpRequestMessage>(),
                 ItExpr.IsAny<CancellationToken>())
              .ReturnsAsync(photoResponse);

            photoHttpClient = new HttpClient(photoHandlerMock.Object) { BaseAddress = new Uri("https://jsonplaceholder.typicode.com"), };
            mockPhotoFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(photoHttpClient);


            photoRepo = new PhotoRepository(mockPhotoFactory.Object, mockPhotoRepoLogger.Object);

            var mockAlbumFactory = new Mock<IHttpClientFactory>();
            albumHandlerMock = new Mock<HttpMessageHandler>();

            var albumResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(TestHelper.GetFileContents("Data.AllAlbums.txt"))
            };

            albumHandlerMock.Protected()
              .Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                 ItExpr.IsAny<HttpRequestMessage>(),
                 ItExpr.IsAny<CancellationToken>())
              .ReturnsAsync(albumResponse);

            albumHttpClient = new HttpClient(albumHandlerMock.Object) { BaseAddress = new Uri("https://jsonplaceholder.typicode.com"), };
            mockAlbumFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(albumHttpClient);
            albumRepo = new AlbumRepository(mockAlbumFactory.Object, mockAlbumRepoLogger.Object);

            var mockServerTimings = new Mock<IServerTimings>();
            var dummyActivitry = new Activity("dummy");

            mockTelemetryService = new Mock<ITelemetryService>();
            mockTelemetryService.Setup(t => t.CreateInstance(It.IsAny<string>(), System.Diagnostics.ActivityKind.Internal, false)).Returns(new TelemetryInstance(mockServerTimings.Object, dummyActivitry, null));


        }

        [Test]
        public void Should_Create_Instance_Of_Controller()
        {
            //arange

            //act
            var controller = new AlbumController(albumRepo, photoRepo, mockTelemetryService.Object);


            //assert
            controller.ShouldNotBeNull();


        }

        [Test]
        public async Task GetAllAlbums_ShouldReturn_AllAlbumsWithPhotos()
        {
            //arrange

            var controller = new AlbumController(albumRepo, photoRepo, mockTelemetryService.Object);

            //act 
            var result = await controller.GetAllAlbums(CancellationToken.None);

            //asert

            controller.ShouldNotBeNull();
            result.ShouldNotBeNull();
            result.ShouldBeOfType<OkObjectResult>();
            var objectReturned = result as OkObjectResult;
            objectReturned.StatusCode.ShouldBe(200);
            objectReturned.Value.ShouldNotBeNull();
            objectReturned.Value.ShouldBeOfType<List<AlbumViewModel>>();

            var albums = objectReturned.Value as List<AlbumViewModel>;
            albums.ShouldNotBeNull();
            albums.Count.ShouldBe(100);

            var firstResult = albums.First();
            firstResult.UserId.ShouldBe(1);
            firstResult.Id.ShouldBe(1);
            firstResult.Title.ShouldBe("quidem molestiae enim");
            firstResult.Photos.Count.ShouldBe(50);

            var firstPhotoOfFirstAlbum = firstResult.Photos.First();
            firstPhotoOfFirstAlbum.AlbumId.ShouldBe(1);
            firstPhotoOfFirstAlbum.Id.ShouldBe(1);
            firstPhotoOfFirstAlbum.Title.ShouldBe("accusamus beatae ad facilis cum similique qui sunt");
            firstPhotoOfFirstAlbum.Url.ShouldBe("https://via.placeholder.com/600/92c952");
            firstPhotoOfFirstAlbum.ThumbnailUrl.ShouldBe("https://via.placeholder.com/150/92c952");

        }

        //TODO test for GetAllAlbumsByUserId controller
    }
}