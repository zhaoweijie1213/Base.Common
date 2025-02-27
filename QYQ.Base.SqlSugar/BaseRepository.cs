﻿using Microsoft.Extensions.Logging;
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
        /// <summary>
        /// 
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// 
        /// </summary>
        private readonly string? _connectionString;

        /// <summary>
        /// 
        /// </summary>
        private readonly ConnectionStringConfig? _connectionStringConfig;


        /// <summary>
        /// 
        /// </summary>
        public BaseRepository(ILogger logger, string connectionString)
        {
            _logger = logger;
            _connectionString = connectionString;

        }

        /// <summary>
        /// 
        /// </summary>
        public BaseRepository(ILogger logger, ConnectionStringConfig connectionStringConfig)
        {
            _logger = logger;
            _connectionStringConfig = connectionStringConfig;

        }

        /// <summary>
        /// dbContext
        /// </summary>
        protected ISqlSugarClient Db => GetSqlSugarClient();


        private const int maxRetryCount = 3; // 最大重试次数
        private int currentRetryCount = 0; // 当前重试计数

        /// <summary>
        /// 获取SqlSugarClient客户端
        /// </summary>
        /// <returns></returns>
        protected SqlSugarClient GetSqlSugarClient()
        {
            var config = new ConnectionConfig() 
            {
                DbType = DbType.MySql,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            };
            if (!string.IsNullOrEmpty(_connectionString))
            {
                config.ConnectionString = _connectionString;
            }
            else if (_connectionStringConfig != null)
            {
                config.ConnectionString = _connectionStringConfig.Master;
                config.SlaveConnectionConfigs = _connectionStringConfig.SlaveConnections;
            }

            SqlSugarClient  db = new(config);
            return ConfigDb(db);
        }

        /// <summary>
        /// 获取自定义连接字符串的SqlSugarClient客户端
        /// </summary>
        /// <returns></returns>
        protected SqlSugarClient GetSqlSugarClient(string connectionString)
        {
            var config = new ConnectionConfig()
            {
                DbType = DbType.MySql,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute,
                ConnectionString = connectionString
            };
            SqlSugarClient db = new(config);
            return ConfigDb(db);
        }

        /// <summary>
        /// 配置客户端
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private SqlSugarClient ConfigDb(SqlSugarClient db)
        {
            //SQL执行完
            db.Aop.OnLogExecuted = (sql, pars) =>
            {
                double sqlTime = db.Ado.SqlExecutionTime.TotalMilliseconds;
                //打印SQL
                if (_logger.IsEnabled(LogLevel.Debug) && sqlTime <= 200)
                {
                    string sqlString = UtilMethods.GetSqlString(DbType.MySql, sql, pars);
                    //执行完了可以输出SQL执行时间 (OnLogExecutedDelegate) 
                    _logger.LogDebug("执行sql语句:{sql},time:{time}ms", sqlString, sqlTime);
                }
                else if (sqlTime > 200)
                {
                    string sqlString = UtilMethods.GetSqlString(DbType.MySql, sql, pars);
                    //执行完了可以输出SQL执行时间 (OnLogExecutedDelegate) 
                    _logger.LogWarning("执行sql语句:{sql},time:{time}ms", sqlString, sqlTime);
                }
            };
            db.Aop.OnError = (exp) =>//SQL报错
            {
                ////获取无参数化 SQL  对性能有影响，特别大的SQL参数多的，调试使用
                //string sqlString = UtilMethods.GetSqlString(DbType.MySql, exp.Sql, (SugarParameter[])exp.Parametres);
                //string sqlTime = db.Ado.SqlExecutionTime.ToString();
                //_logger.LogError("执行sql语句:{sql},time:{time}ms", sqlString, sqlTime);

                //if (currentRetryCount < maxRetryCount)
                //{
                //    if (exp.Message.Contains("网络错误") || exp.Message.Contains("timeout"))
                //    {
                //        currentRetryCount++;
                //        try
                //        {
                //            db.Ado.Connection.Close();
                //            db.Ado.Connection.Open(); // 尝试重新连接
                //            db.Ado.ExecuteCommand(exp.Sql); // 重试执行之前失败的命令
                //        }
                //        catch (Exception retryExp)
                //        {
                //            _logger.LogError($"重试 {currentRetryCount} 失败: {retryExp.Message}");
                //            if (currentRetryCount >= maxRetryCount)
                //            {
                //                throw new Exception("达到最大重试次数，仍然失败", retryExp);
                //            }
                //            Thread.Sleep(60000); // 等待一段时间后再重试
                //        }
                //    }
                //}
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
        /// 批量分页入库
        /// </summary>
        /// <param name="list"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual Task<int> InsertByPageAsync(List<TEntity> list, int pageSize = 100)
        {
            return Db.Insertable(list).PageSize(pageSize).ExecuteCommandAsync();
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