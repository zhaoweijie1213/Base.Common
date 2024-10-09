using Asp.Versioning;
using GamePlay.Grpc;
using Grpc.Net.ClientFactory;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using QYQ.Base.Common.ApiResult;
using QYQ.Base.Consul.Grpc;
using QYQ.Base.SnowId.Interface;
using Test.Models.Input;
using Test.Web.Models;

namespace Test.Web.Controllers
{
    /// <summary>
    /// 支付
    /// </summary>
    [Route("/api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1")]
    [ApiExplorerSettings(GroupName = "v1")]
    [OpenApiTag("V3VersionedValues", Description = "New operations that should be only visible for version 3")]
    //[ApiExplorerSettings(GroupName = "v1")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        private readonly ISnowIdGenerator _snowIdGenerator;

        private readonly GrpcClientFactory _consulGrpcClientFactory;

        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="consulGrpcClientFactory"></param>
        public WeatherForecastController(ILogger<WeatherForecastController> logger, GrpcClientFactory consulGrpcClientFactory, ISnowIdGenerator snowIdGenerator, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _consulGrpcClientFactory = consulGrpcClientFactory;
            _snowIdGenerator = snowIdGenerator;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        [HttpPost("Test")]
        public async Task<IEnumerable<WeatherForecast>> Test(WeatherInput input)
        {

            var client = _consulGrpcClientFactory.CreateClient<GamePlay.Grpc.GamePlay.GamePlayClient>("Gameplay");

            var res = await client.TicketAsync(new TicketRequest()
            {
                GameId = "vs40wildwest",
                UserId = 126730573,
                Language = "en",
                Platform = "web"
            });


           var httpClient = _httpClientFactory.CreateClient("game-play");

            var healthRes = await httpClient.GetAsync("api/Health");

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet]
        public ApiResult<string> Get(string name)
        {
            ApiResult<string> result = new();

            var id = _snowIdGenerator.CreateId();

            DateTime dateTime = _snowIdGenerator.GetDateTime(id);

            result.SetRsult(ApiResultCode.Success, $"{name} {dateTime:g}");
            return result;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("Email")]
        public ApiResult<string> Email([FromForm] EmailInput input)
        {
            ApiResult<string> result = new();
            result.SetRsult(ApiResultCode.Success, input.Email);
            return result;

        }
    }
}