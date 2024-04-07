using QYQ.Base.Common.IOCExtensions;
using QYQ.Base.SqlSugar;
using Test.Entity;

namespace Test.Repository.Interface
{
    public interface IProductRepository : IBaseRepository<ProductInfoEntity>, ITransientDependency
    {

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<ProductInfoEntity> GetProducts();

    }
}
