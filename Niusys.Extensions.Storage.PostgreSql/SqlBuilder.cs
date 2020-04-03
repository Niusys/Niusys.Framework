namespace Niusys.Extensions.Storage.PostgreSql
{
    public class SqlBuilder
    {
        public SqlBuilder(string tableName)
        {
            this.TableName = tableName;
        }

        public string SelectSql { get; set; } = "*";
        public string WhereSql { get; set; } = "1=1";
        public string OrderSql { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string TableName { get; set; }
        public string GroupBy { get; set; }
    }
}
