using ep.server.api.Repositiory;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Shouldly;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ep.server.api.test
{
    public class AlbumRepositoryTest
    {
        private Mock<ILogger<AlbumRepository>> mockAlbumRepoLogger;
        private AlbumRepository albumRepo;
        private Mock<HttpMessageHandler> handlerMock;
        private HttpClient httpClient;


        [SetUp]
        public void Setup()
        {
            mockAlbumRepoLogger = new Mock<ILogger<AlbumRepository>>();
            var mockFactory = new Mock<IHttpClientFactory>();
            handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(TestHelper.GetFileContents("Data.AllAlbums.txt"))
            };

            handlerMock.Protected()
              .Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                 ItExpr.IsAny<HttpRequestMessage>(),
                 ItExpr.IsAny<CancellationToken>())
              .ReturnsAsync(response);

            httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://jsonplaceholder.typicode.com"), };
            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            albumRepo = new AlbumRepository(mockFactory.Object, mockAlbumRepoLogger.Object);
        }

        [Test]
        public async Task GetAllAlbumsAsync_ShouldReturn_AllAlbums()
        {
            //arrange



            //act 
            var result = await albumRepo.GetAllAlbumsAsync(CancellationToken.None);

            //asert


            result.ShouldNotBeNull();
            result.Count.ShouldBe(100);
            var firstResult = result.OrderBy(x => x.Id).First();
            firstResult.UserId.ShouldBe(1);
            firstResult.Id.ShouldBe(1);
            firstResult.Title.ShouldBe("quidem molestiae enim");


        }

        [Test]
        public async Task GetAllAlbumsAsync_ShouldCallHttpCLientGetAsync_ExactlyOnce()
        {
            //arrange


            //act 
            var result = await albumRepo.GetAllAlbumsAsync(CancellationToken.None);

            //asert

            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1),
               ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
               ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task GetAlbumByUserIdAsync_ShouldReturn_AllAlbumsByUserId()
        {
            //arrange

            int userId = 1;

            var mockFactory = new Mock<IHttpClientFactory>();
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(TestHelper.GetFileContents("Data.AllAlbumsForUserId_1.txt"))
            };

            handlerMock.Protected()
              .Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                 ItExpr.IsAny<HttpRequestMessage>(),
                 ItExpr.IsAny<CancellationToken>())
              .ReturnsAsync(response);

            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://jsonplaceholder.typicode.com"), };
            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var albumRepo = new AlbumRepository(mockFactory.Object, mockAlbumRepoLogger.Object);

            //act 
            var result = await albumRepo.GetAlbumsByUserIdAsync(userId, CancellationToken.None);

            //asert


            result.ShouldNotBeNull();
            result.Count.ShouldBe(10);
            var firstResult = result.First();
            firstResult.UserId.ShouldBe(userId);
            firstResult.Id.ShouldBe(1);
            firstResult.Title.ShouldBe("quidem molestiae enim");

        }

        [Test]
        public async Task GetAlbumByUserIdAsync_InvalidContentShouldReturn_Null()
        {
            //arrange

            int userId = 1;

            var mockFactory = new Mock<IHttpClientFactory>();
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
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

            var albumRepo = new AlbumRepository(mockFactory.Object, mockAlbumRepoLogger.Object);

            //act 
            var result = await albumRepo.GetAlbumsByUserIdAsync(userId, CancellationToken.None);

            //asert


            result.ShouldBeNull();

        }

        [Test]
        public async Task GetAlbumByUserIdAsync_HttpResponseBadGetwayShouldHandle_Exception()
        {
            //arrange

            int userId = 1;

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

            var albumRepo = new AlbumRepository(mockFactory.Object, mockAlbumRepoLogger.Object);

            //act 
            var result = await albumRepo.GetAlbumsByUserIdAsync(userId, CancellationToken.None);

            //asert

            result.ShouldBeNull();

            mockAlbumRepoLogger.Verify(m => m.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().StartsWith("Error")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);

        }
    }
}