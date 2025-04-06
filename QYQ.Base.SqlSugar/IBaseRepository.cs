using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.SqlSugar
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public int Insert(List<TEntity> entities);

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public TEntity Insert(TEntity entity);

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public Task<TEntity> InsertAsync(TEntity entity);

        /// <summary>
        /// 批量分页入库
        /// </summary>
        /// <param name="list"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public Task<int> InsertByPageAsync(List<TEntity> list, int pageSize = 100);

        /// <summary>
        /// 查询列表
        /// </summary>
        /// <returns></returns>
        public List<TEntity> QueryList();

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool UpdateByPrimaryKey(TEntity entity);

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public Task<bool> UpdateByPrimaryKeyAsync(TEntity entity);

    }
}
