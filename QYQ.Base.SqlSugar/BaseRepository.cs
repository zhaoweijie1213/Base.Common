using Microsoft.Extensions.Logging;
using SqlSugar;
using DbType = SqlSugar.DbType;

namespace QYQ.Base.SqlSugar
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class, new()
    {
        private readonly ILogger _logger;

        private readonly string _connectionString;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString"></param>
        public BaseRepository(ILogger logger, string connectionString)
        {
            _logger = logger;
            _connectionString = connectionString;

        }

        /// <summary>
        /// dbContext
        /// </summary>
        public ISqlSugarClient Db => GetSqlSugarClient();

        /// <summary>
        /// 获取SqlSugarClient客户端
        /// </summary>
        /// <returns></returns>
        public SqlSugarClient GetSqlSugarClient(string? connectionString = null)
        {
            SqlSugarClient? db = null;
            if (string.IsNullOrEmpty(connectionString))
            {
                db = new(new ConnectionConfig()
                {
                    ConnectionString = _connectionString,
                    DbType = DbType.MySql,
                    IsAutoCloseConnection = true,
                    InitKeyType = InitKeyType.Attribute
                });
            }
            else
            {
                db = new(new ConnectionConfig()
                {
                    ConnectionString = connectionString,
                    DbType = DbType.MySql,
                    IsAutoCloseConnection = true,
                    InitKeyType = InitKeyType.Attribute
                });
            }

            //SQL执行完
            db.Aop.OnLogExecuted = (sql, pars) =>
            {
                //打印SQL
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    string sqlString = UtilMethods.GetSqlString(DbType.MySql, sql, pars);
                    double sqlTime = db.Ado.SqlExecutionTime.TotalMilliseconds;
                    //执行完了可以输出SQL执行时间 (OnLogExecutedDelegate) 
                    _logger.LogDebug("执行sql语句:{sql},time:{time}ms", sqlString, sqlTime);
                }
            };


            db.Aop.OnError = (exp) =>//SQL报错
            {
                //获取无参数化 SQL  对性能有影响，特别大的SQL参数多的，调试使用
                string sqlString = UtilMethods.GetSqlString(DbType.MySql, exp.Sql, (SugarParameter[])exp.Parametres);
                string sqlTime = db.Ado.SqlExecutionTime.ToString();
                _logger.LogError("执行sql语句:{sql},time:{time}ms", sqlString, sqlTime);
            };

            return db;
        }


        /// <summary>
        /// 批量新增
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual int Insert(List<TEntity> entities)
        {
            return Db.Insertable(entities).ExecuteCommand();
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual TEntity Insert(TEntity entity)
        {
            return Db.Insertable(entity).ExecuteReturnEntity();
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual Task<TEntity> InsertAsync(TEntity entity)
        {
            return Db.Insertable(entity).ExecuteReturnEntityAsync();
        }

        /// <summary>
        /// 查询列表
        /// </summary>
        /// <returns></returns>
        public virtual List<TEntity> QueryList()
        {
            return Db.Queryable<TEntity>().ToList();
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual bool Update(TEntity entity)
        {
            return Db.Updateable(entity).ExecuteCommandHasChange();
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual Task<bool> UpdateAsync(TEntity entity)
        {
            return Db.Updateable(entity).ExecuteCommandHasChangeAsync();
        }

    }
}