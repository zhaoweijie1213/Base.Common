using Grpc.Net.ClientFactory;
using Grpc.Team;
using LobbyWebManagement;
using QYQ.Base.Common.IOCExtensions;
using QYQ.Base.Consul.Grpc;
using System.Net.Http;
using Xunit;

namespace Test.Test
{
    public class GameIntegrationApiTest : IDependency
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly GameIntegrationApi _gameIntegrationApi;

        private readonly GrpcClientFactory _consulGrpcClientFactory;

        public GameIntegrationApiTest(IHttpClientFactory httpClientFactory, GameIntegrationApi gameIntegrationApi, GrpcClientFactory consulGrpcClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _gameIntegrationApi = gameIntegrationApi;
            _consulGrpcClientFactory = consulGrpcClientFactory;
        }

        ///// <summary>
        ///// 获取游戏列表
        ///// </summary>
        ///// <param name="options"></param>
        //[Theory]
        //[InlineData(null)]
        //public async void GetCasinoGamesTest(string options)
        //{
        //    CasinoGamesRequest request = new()
        //    {
        //        secureLogin = "eveb_mn",
        //        options = options
        //    };
        //    var res = await _gameIntegrationApi.GetCasinoGames(request);

        //    Assert.Equal(0, res?.error);
        //}

        /// <summary>
        /// 健康检查
        /// </summary>
        [Fact]
        public async void HealthCheck()
        {

            var client = _consulGrpcClientFactory.CreateClient<GamePlay.Grpc.GamePlay.GamePlayClient>("Gameplay");

            var res = await _gameIntegrationApi.GetCasinoGamesAsync(new CasinoGamesRequest());
            Assert.Equal(0, res?.Error);
        }
    }
}