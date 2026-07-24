using Asp.Versioning;
//using GamePlay.Grpc;
using Grpc.Net.ClientFactory;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using QYQ.Base.Common.ApiResult;
using QYQ.Base.Common.Tool;
using QYQ.Base.Consul.Grpc;
using QYQ.Base.SnowId.Interface;
using Test.Models.Input;
using Test.Web.Models;
using static Grpc.ShortLink.ShortLink;

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


        private readonly IHttpClientFactory _httpClientFactory;

        private readonly GrpcClientFactory _grpcClientFactory;

        /// <summary>
        /// 
        /// </summary>
        public WeatherForecastController(ILogger<WeatherForecastController> logger,ISnowIdGenerator snowIdGenerator, IHttpClientFactory httpClientFactory, GrpcClientFactory grpcClientFactory)
        {
            _logger = logger;
            _snowIdGenerator = snowIdGenerator;
            _httpClientFactory = httpClientFactory;
            _grpcClientFactory = grpcClientFactory;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        [HttpPost("Test")]
        public async Task<IEnumerable<WeatherForecast>> Test(WeatherInput input)
        {

            //var client = _consulGrpcClientFactory.CreateClient<GamePlay.Grpc.GamePlay.GamePlayClient>("Gameplay");

            //var res = await client.TicketAsync(new TicketRequest()
            //{
            //    GameId = "vs40wildwest",
            //    UserId = 126730573,
            //    Language = "en",
            //    Platform = "web"
            //});


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
        /// <returns></returns>
        [HttpGet]
        public ApiResult<string> Get()
        {
            ApiResult<string> result = new();

            var dateTimeNow = DateTime.UtcNow;

            var id = _snowIdGenerator.CreateId(dateTimeNow);

            DateTime dateTime = _snowIdGenerator.GetDateTime(id);

            if (dateTimeNow.ToString("yyyy-MM-dd HH:mm:ss:fff") == dateTime.ToString("yyyy-MM-dd HH:mm:ss:fff"))
            {
                result.SetResult(ApiResultCode.Success, $"{dateTime:g} ID:{id}");
            }

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
            result.SetResult(ApiResultCode.Success, input.Email);
            return result;

        }

        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        [HttpGet("GetQRCodeAsync")]
        public async Task<ApiResult<byte[]>> GetQRCodeAsync(string content)
        {
            ApiResult<byte[]> result = new();
            var qr = await QRCoderHepler.GenerateQRCodeAsync(content);
            result.SetResult(ApiResultCode.Success, qr);
            return result;

        }

        /// <summary>
        /// 生成Base64二维码
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        [HttpGet("GetBase64QRCodeAsync")]
        public async Task<ApiResult<string>> GetBase64QRCodeAsync(string content)
        {
            ApiResult<string> result = new();
            var qr = await QRCoderHepler.GenerateBase64QRCodeAsync(content);
            result.SetResult(ApiResultCode.Success, qr);
            return result;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet("GetUrlAsync")]
        public async Task<ApiResult<string>> GetUrlAsync(string code)
        {
            ApiResult<string> result = new();
            try
            {
                var client = _grpcClientFactory.CreateClient<ShortLinkClient>("short-link");
                //client.BaseAddress = new Uri(options.CurrentValue.BaseUrl.TrimEnd('/') + "/");
                var output = await client.ResolveAsync(new Grpc.ShortLink.ResolveRequest() { Code = code });
                return result.SetResult(ApiResultCode.Success, output.Data.LongUrl);
            }
            catch (Exception ex)
            {
                return result.SetResult(ApiResultCode.InternalServerError, null, ex.Message);
            }
        }


    }
}