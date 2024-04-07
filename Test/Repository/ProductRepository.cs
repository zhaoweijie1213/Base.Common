using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QYQ.Base.SqlSugar;
using Test.Entity;
using Test.Repository.Interface;

namespace Test.Repository
{
    public class ProductRepository : BaseRepository<ProductInfoEntity>, IProductRepository
    {
        public ProductRepository(ILogger<ProductRepository> logger, IConfiguration configuration)
            : base(logger, configuration.GetSection("ConnectionStrings:User9100").Get<string>())
        {


        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<ProductInfoEntity> GetProducts()
        {
            return Db.Queryable<ProductInfoEntity>().Where(i=>i.Id>0 && i.Lobby >0).ToList();
        }

    }
}
