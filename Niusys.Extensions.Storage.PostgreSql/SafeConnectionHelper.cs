using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Niusys.Extensions.Storage.PostgreSql
{
    public class SafeConnectionHelper
    {
        private readonly ILogger<SafeConnectionHelper> _logger;

        public SafeConnectionHelper(ILogger<SafeConnectionHelper> logger)
        {
            _logger = logger;
        }

        public void SafeCloseConnection(IDbConnection conn)
        {
            try
            {
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            catch (DbException e)
            {
                _logger.LogError(e, e.FullMessage());
                throw new RdbException(RdbExceptionCode.DatabaseConnectionOpenFail, e);
            }
        }

        public async Task SafeOpenConnection(IDbConnection conn, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!(conn is NpgsqlConnection pgConn))
                {
                    if (conn == null)
                    {
                        throw new ArgumentNullException(nameof(conn), $"不能为null");
                    }
                    else
                    {
                        throw new InvalidCastException($"{nameof(conn)}参数不能转换为{nameof(NpgsqlConnection)}类型");
                    }
                }

                if (pgConn.State != ConnectionState.Open)
                {
                    await pgConn.OpenAsync(cancellationToken);
                }
            }
            catch (DbException e)
            {
                _logger.LogError(e, e.FullMessage());
                throw new RdbException(RdbExceptionCode.DatabaseConnectionOpenFail, e);
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e, e.FullMessage());
                throw new RdbException(RdbExceptionCode.DatabaseConnectionOpenFail, e);
            }
        }
    }
}
