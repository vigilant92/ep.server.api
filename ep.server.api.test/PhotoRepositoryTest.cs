using ep.server.api.Repositiory;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ep.server.api.test
{
    public class PhotoRepositoryTest
    {
        private Mock<ILogger<PhotoRepository>> mockPhotoRepoLogger;
        private PhotoRepository albumRepo;
        private Mock<HttpMessageHandler> handlerMock;
        private HttpClient httpClient;

        [SetUp]
        public void Setup()
        {
            mockPhotoRepoLogger = new Mock<ILogger<PhotoRepository>>();
            var mockFactory = new Mock<IHttpClientFactory>();
            handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(TestHelper.GetFileContents("Data.AllPhotos.txt"))
            };

            handlerMock.Protected()
              .Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                 ItExpr.IsAny<HttpRequestMessage>(),
                 ItExpr.IsAny<CancellationToken>())
              .ReturnsAsync(response);

            httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://jsonplaceholder.typicode.com"), };
            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            albumRepo = new PhotoRepository(mockFactory.Object, mockPhotoRepoLogger.Object);
        }

        [Test]
        public async Task GetAllPhotosAsync_ShouldReturn_AllPhotos()
        {
            //arrange


            //act 
            var result = await albumRepo.GetAllPhotosAsync(CancellationToken.None);

            //asert


            result.ShouldNotBeNull();
            result.Count.ShouldBe(5000);
            var firstResult = result.OrderBy(x => x.Id).First();
            firstResult.AlbumId.ShouldBe(1);
            firstResult.Id.ShouldBe(1);
            firstResult.Title.ShouldBe("accusamus beatae ad facilis cum similique qui sunt");
            firstResult.Url.ShouldBe("https://via.placeholder.com/600/92c952");
            firstResult.ThumbnailUrl.ShouldBe("https://via.placeholder.com/150/92c952");


        }

        [Test]
        public async Task GetAllPhotosAsync_ShouldCallHttpCLientGetAsync_ExactlyOnce()
        {
            //arrange


            //act 
            var result = await albumRepo.GetAllPhotosAsync(CancellationToken.None);

            //asert

            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1),
               ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
               ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task GetAllPhotosByAlbumIdsAsync_ShouldReturn_AllPhotosByAlbumIds()
        {
            //arrange

            var albumIds = new List<int>() { 4, 5 };

            var mockFactory = new Mock<IHttpClientFactory>();
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(TestHelper.GetFileContents("Data.AllPhotosForAlbum_4_5.txt"))
            };

            handlerMock.Protected()
              .Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                 ItExpr.IsAny<HttpRequestMessage>(),
                 ItExpr.IsAny<CancellationToken>())
              .ReturnsAsync(response);

            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://jsonplaceholder.typicode.com"), };
            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var albumRepo = new PhotoRepository(mockFactory.Object, mockPhotoRepoLogger.Object);

            //act 
            var result = await albumRepo.GetAllPhotosByAlbumIdsAsync(albumIds, CancellationToken.None);

            //asert


            result.ShouldNotBeNull();
            result.Count.ShouldBe(100);
            var firstResult = result.First();
            firstResult.AlbumId.ShouldBe(4);
            firstResult.Id.ShouldBe(151);
            firstResult.Title.ShouldBe("possimus dolor minima provident ipsam");
            firstResult.Url.ShouldBe("https://via.placeholder.com/600/1d2ad4");
            firstResult.ThumbnailUrl.ShouldBe("https://via.placeholder.com/150/1d2ad4");


        }

        [Test]
        public async Task GetAllPhotosByAlbumIdsAsync_InvalidContentShouldReturn_Null()
        {
            //arrange

            var albumIds = new List<int>() { 4, 5 };

            var mockFactory = new Mock<IHttpClientFactory>();
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadGateway,
                Content = null
            };

            handlerMock.Protected()
              .Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                 ItExpr.IsAny<HttpRequestMessage>(),
                 ItExpr.IsAny<CancellationToken>())
              .ReturnsAsync(response);

            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://jsonplaceholder.typicode.com"), };
            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var albumRepo = new PhotoRepository(mockFactory.Object, mockPhotoRepoLogger.Object);

            //act 
            var result = await albumRepo.GetAllPhotosByAlbumIdsAsync(albumIds, CancellationToken.None);

            //asert


            result.ShouldBeNull();

        }

        [Test]
        public async Task GetPhotoByUserIdAsync_HttpResponseBadGetwayShouldHandle_Exception()
        {
            //arrange

            var albumIds = new List<int>() { 4, 5 };

            var mockFactory = new Mock<IHttpClientFactory>();
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[  {    \"userId\": 1,    \"id\": 1,")
            };

            handlerMock.Protected()
              .Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                 ItExpr.IsAny<HttpRequestMessage>(),
                 ItExpr.IsAny<CancellationToken>())
              .ReturnsAsync(response);

            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://jsonplaceholder.typicode.com"), };
            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var albumRepo = new PhotoRepository(mockFactory.Object, mockPhotoRepoLogger.Object);

            //act 
            var result = await albumRepo.GetAllPhotosByAlbumIdsAsync(albumIds, CancellationToken.None);

            //asert

            result.ShouldBeNull();

            mockPhotoRepoLogger.Verify(m => m.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().StartsWith("Error")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

        }
    }
}