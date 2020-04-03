using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Niusys.Extensions.Storage.PostgreSql
{
    public abstract class SqlBaseRepository : RepositoryMethodExtension
    {
        private readonly ILogger _logger;

        private SqlConnectionManager SqlConnectionManager { get; }

        public SqlBaseRepository(SqlConnectionManager sqlConnectionManager,
            SafeConnectionHelper safeConnectionHelper,
            ILogger logger)
            : base(safeConnectionHelper, logger, timeout: sqlConnectionManager.DbSetting.Timeout ?? 10)
        {
            SqlConnectionManager = sqlConnectionManager;
            this._logger = logger;
        }

        public IConfiguration Configuration { get; }
        public DatabaseOptions Options { get; }
        protected abstract string ConnectionName { get; }

        public override NpgsqlConnection GetNewConnection()
        {
            _logger.LogTrace("开始获取新的DbConn");
            var conn = SqlConnectionManager.GetNewConnection(ConnectionName ?? "Default");
            _logger.LogTrace("获取新的DbConn结束");
            return conn;
        }
    }
}
