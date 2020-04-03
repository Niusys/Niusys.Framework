using Dapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Niusys.Extensions.ComponentModels;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Niusys.Extensions.Storage.PostgreSql
{
    public abstract class RepositoryMethodExtension
    {
        public SafeConnectionHelper SafeConnectionHelper { get; }

        private readonly ILogger _logger;
        private readonly int _timeout;

        public RepositoryMethodExtension(SafeConnectionHelper safeConnectionHelper, ILogger logger, int timeout = 10)
        {
            this.SafeConnectionHelper = safeConnectionHelper ?? throw new ArgumentNullException(nameof(safeConnectionHelper));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._timeout = timeout;
        }

        #region TransScope

        /// <summary>
        /// 用一个事物执行多个数据库操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="trans">Trans=NULL会创建一个新的Connection, 并开启新的事物, 不过再此处可以选择传入SessionTrans来在NHibernate Session的基础上执行，避免开启新的Connection, 以节省Connection资源</param>
        /// <param name="isolationLevel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<T> TransScope<T>(Func<IDbTransaction, CancellationToken, Task<T>> action,
            IDbTransaction trans = null,
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            CancellationToken cancellationToken = default)
        {
            if (trans != null)
            {
                return await action(trans, cancellationToken);
            }

            GetConnection(null, out var conn, out var isTrans);

            try
            {
                await SafeConnectionHelper.SafeOpenConnection(conn, cancellationToken);
                trans = conn.BeginTransaction(isolationLevel);
                var result = await action(trans, cancellationToken);
                trans.Commit();
                return result;
            }
            catch (Exception)
            {
                //rollback when err
                trans?.Rollback();
                throw;
            }
            finally
            {
                if (!isTrans)
                {
                    trans?.Dispose();
                    //close conn
                    SafeConnectionHelper.SafeCloseConnection(conn);
                }
            }
        }

        public async Task TransScope(Func<IDbTransaction, CancellationToken, Task> action,
            IDbTransaction trans = null,
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            CancellationToken cancellationToken = default)
        {
            await TransScope<string>(async (transaction, cancelToken) =>
            {
                await action(transaction, cancelToken);
                return null;
            }, trans, isolationLevel, cancellationToken: cancellationToken);
        }

        #endregion

        #region 供服务层调用的数据库操作方法
        protected virtual async Task<int> ExecuteNoQueryAsync(string sql, object param = null,
            IDbTransaction outerTrans = null,
            CancellationToken cancellationToken = default)
        {
            return await ExecuteInternalAsync(sql, param, outerTrans, cancellationToken: cancellationToken);
        }

        protected virtual async Task ExecuteMultipleNoQueryAsync(object param = null,
            IDbTransaction outerTrans = null,
            CancellationToken cancellationToken = default,
            params string[] sqlScripts)
        {
            await ExecuteMultipleInternalAsync(string.Join(" ", sqlScripts), param, outerTrans, cancellationToken: cancellationToken);
        }

        protected virtual async Task ExecuteMultipleNoQueryAsync(object param = null,
            IDbTransaction outerTrans = null,
            params string[] sqlScripts)
        {
            await ExecuteMultipleInternalAsync(string.Join(" ", sqlScripts), param, outerTrans, cancellationToken: default);
        }

        protected virtual async Task<T> GetAsync<T>(string sql, object param = null,
            IDbTransaction outerTrans = null,
            CancellationToken cancellationToken = default)
        {
            return await GetInternalAsync<T>(sql, param, outerTrans, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <returns></returns>
        protected virtual async Task<Page<T>> GetPageListAsync<T>(int pageIndex, int pageSize,
            string selectSql, string tableName, string whereSql, string orderBy, string groupBy = "",
            object param = null,
            IDbTransaction outerTrans = null, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrWhiteSpace(whereSql))
                whereSql = $"where {whereSql} ";

            if (string.IsNullOrWhiteSpace(orderBy))
                throw new ArgumentNullException(nameof(orderBy), $"分页查询时{nameof(orderBy)}参数不能为空");

            if (!string.IsNullOrWhiteSpace(groupBy))
                groupBy = $"group by {groupBy} ";

            var pagerSql = $@"SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY {orderBy}) RowIndex, {selectSql} from {tableName} {whereSql} {groupBy}) p_paged
WHERE RowIndex > { (pageIndex - 1) * pageSize} AND RowIndex <= {pageIndex * pageSize}";
            string countSql = $@"select count(1) from {tableName} {whereSql}";
            if (!string.IsNullOrWhiteSpace(groupBy))
            {
                countSql = $@" select count(1) from ( select {selectSql} from {tableName} {whereSql} {groupBy} ) t ";
            }
            return await GetPagerInternalAsync<T>(pagerSql, countSql, param, pageIndex, pageSize, outerTrans, cancellationToken: cancellationToken);
        }

        public async Task<Page<T>> GetPageListAsync<T>(SqlBuilder obj, object param = null,
            IDbTransaction trans = null,
            CancellationToken cancellationToken = default)
        {
            return await GetPageListAsync<T>(obj.PageIndex, obj.PageSize, obj.SelectSql, obj.TableName, obj.WhereSql, obj.OrderSql, obj.GroupBy, param, trans, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// GetListAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="param"></param>
        /// <param name="trans"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetListAsync<T>(SqlBuilder obj, object param = null,
            IDbTransaction trans = null,
            CancellationToken cancellationToken = default)
        {
            return await GetListAsync<T>(obj.SelectSql, obj.TableName,
                string.IsNullOrWhiteSpace(obj.WhereSql)
                ? " 1=1 "
                : obj.WhereSql, obj.OrderSql, param, trans, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// GetListAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectSql"></param>
        /// <param name="tableName"></param>
        /// <param name="whereSql"></param>
        /// <param name="orderBy"></param>
        /// <param name="param"></param>
        /// <param name="outerTrans"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected virtual async Task<IEnumerable<T>> GetListAsync<T>(string selectSql, string tableName,
            string whereSql, string orderBy,
            object param = null,
            IDbTransaction outerTrans = null,
            CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrWhiteSpace(whereSql))
                whereSql = $"where {whereSql} ";

            if (!string.IsNullOrWhiteSpace(orderBy))
                orderBy = $"order by {orderBy} ";

            var getListSql = string.Format($@"SELECT {selectSql} from {tableName} {whereSql} {orderBy}");

            return await GetListInternalAsync<T>(getListSql, param, outerTrans, cancellationToken: cancellationToken);
        }

        //protected virtual async Task<IEnumerable<T>> GetList<T>(string sql, object param = null, IDbTransaction outerTrans = null)
        //{
        //    return GetListInternalAsync<T>(sql, param, outerTrans).Result;
        //}

        protected async Task<IEnumerable<T>> GetListAsync<T>(string sql, object param = null,
            IDbTransaction outerTrans = null,
            CancellationToken cancellationToken = default)
        {
            return await GetListInternalAsync<T>(sql, param, outerTrans, cancellationToken: cancellationToken);
        }
        #endregion

        #region Warpped Dapper Internal Method and Logging
        private async Task<int> ExecuteInternalAsync(string sql, object param = null,
            IDbTransaction outerTrans = null,
            CancellationToken cancellationToken = default)
        {
            WriteLog(sql, param);

            GetConnection(outerTrans, out var conn, out var isTrans);

            try
            {
                await SafeConnectionHelper.SafeOpenConnection(conn, cancellationToken);
                return await conn.ExecuteAsync(new CommandDefinition(sql, param, outerTrans, commandTimeout: _timeout, cancellationToken: cancellationToken));
            }
            catch (NpgsqlException)
            {
                throw;
            }
            finally
            {
                //cmd.Dispose();
                if (!isTrans)
                    SafeConnectionHelper.SafeCloseConnection(conn);
            }
        }

        private async Task ExecuteMultipleInternalAsync(string sql, object param = null,
            IDbTransaction outerTrans = null,
            CancellationToken cancellationToken = default)
        {
            WriteLog(sql, param);

            GetConnection(outerTrans, out var conn, out var isTrans);

            try
            {
                await SafeConnectionHelper.SafeOpenConnection(conn, cancellationToken);
                await conn.QueryMultipleAsync(new CommandDefinition(sql, param, outerTrans, commandTimeout: _timeout, cancellationToken: cancellationToken));
            }
            catch (NpgsqlException ex)
            {
                WriteLog(ex);
                throw;
            }
            finally
            {
                //cmd.Dispose();
                if (!isTrans)
                    SafeConnectionHelper.SafeCloseConnection(conn);
            }
        }

        protected async Task<T> ScalarAsync<T>(string sql, object param = null,
            IDbTransaction outerTrans = null,
            CommandType? commandType = null,
            CancellationToken cancellationToken = default)
        {
            WriteLog(sql, param);

            GetConnection(outerTrans, out var conn, out var isTrans);

            try
            {
                await SafeConnectionHelper.SafeOpenConnection(conn, cancellationToken);
                return await conn.ExecuteScalarAsync<T>(new CommandDefinition(sql, param, outerTrans, commandType: commandType, commandTimeout: _timeout, cancellationToken: cancellationToken));
            }
            catch (NpgsqlException ex)
            {
                WriteLog(ex);
                throw;
            }
            finally
            {
                //cmd.Dispose();
                if (!isTrans)
                    SafeConnectionHelper.SafeCloseConnection(conn);
            }
        }

        private async Task<T> GetInternalAsync<T>(string sql, object param = null,
            IDbTransaction outerTrans = null,
            CancellationToken cancellationToken = default)
        {
            WriteLog(sql, param);
            GetConnection(outerTrans, out var conn, out var isTrans);

            try
            {
                _logger.LogDebug("开始开启DbConn");
                await SafeConnectionHelper.SafeOpenConnection(conn, cancellationToken);
                _logger.LogDebug($"DbConn已开启，开始执行SQL:{sql}");
                var result = await conn.QueryFirstOrDefaultAsync<T>(new CommandDefinition(sql, param, outerTrans, commandTimeout: _timeout, cancellationToken: cancellationToken));
                _logger.LogDebug("SQL执行结束");
                return result;
            }
            catch (NpgsqlException ex)
            {
                WriteLog(ex);
                throw;
            }
            finally
            {
                if (!isTrans)
                {
                    SafeConnectionHelper.SafeCloseConnection(conn);
                    _logger.LogDebug("安全关闭DbConn");
                }
            }
        }

        private async Task<IEnumerable<T>> GetListInternalAsync<T>(string sql, object param = null,
            IDbTransaction outerTrans = null,
            CancellationToken cancellationToken = default)
        {
            WriteLog(sql, param);

            GetConnection(outerTrans, out var conn, out var isTrans);

            try
            {
                await SafeConnectionHelper.SafeOpenConnection(conn, cancellationToken);
                return await conn.QueryAsync<T>(new CommandDefinition(sql, param, outerTrans, commandTimeout: _timeout, cancellationToken: cancellationToken));
            }
            catch (NpgsqlException ex)
            {
                WriteLog(ex);
                throw;
            }
            finally
            {
                if (!isTrans)
                    SafeConnectionHelper.SafeCloseConnection(conn);
            }
        }

        private async Task<Page<T>> GetPagerInternalAsync<T>(string pagerSql, string countSql, object param, int pageIndex,
            int pageSize,
            IDbTransaction outerTrans = null,
            CancellationToken cancellationToken = default)
        {
            var sql = string.Format("{0};{1}", pagerSql, countSql);
            WriteLog(sql, param);

            GetConnection(outerTrans, out var conn, out var isTrans);

            try
            {
                await SafeConnectionHelper.SafeOpenConnection(conn, cancellationToken);
                var result = new Page<T> { Paging = new Paging { PageIndex = pageIndex, PageSize = pageSize } };

                using (var multi = await conn.QueryMultipleAsync(new CommandDefinition(sql, param, outerTrans, commandTimeout: _timeout, cancellationToken: cancellationToken)))
                {
                    result.Records = multi.Read<T>().ToList();
                    result.Paging.Total = multi.Read<int>().Single();
                }
                return result;
            }
            catch (NpgsqlException ex)
            {
                WriteLog(ex);
                throw;
            }
            finally
            {
                if (!isTrans)
                    SafeConnectionHelper.SafeCloseConnection(conn);
            }
        }

        protected void GetConnection(IDbTransaction trans, out IDbConnection conn, out bool isTransaction)
        {
            if (trans == null)
            {
                conn = GetNewConnection();
                isTransaction = false;
            }
            else
            {
                conn = trans.Connection;
                isTransaction = true;
            }
        }

        public abstract NpgsqlConnection GetNewConnection();

        protected virtual void WriteLog(string sql, object parms)
        {
            _logger.LogTrace($"SQL:{SqlCommandToStr.CommandAsSql(sql, parms)} Parms:{JsonConvert.SerializeObject(parms)}");
        }

        protected virtual void WriteLog(NpgsqlException ex)
        {
            _logger.LogError(ex, $"SQL脚本执行异常: {ex.FullMessage()}");
        }
        #endregion
    }
}
