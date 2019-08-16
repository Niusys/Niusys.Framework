namespace Niusys.Extensions.Storage.Mongo
{
    public abstract class MongodbOptions
    {
        public string ConnectionString { get; set; }
        public string Database { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string LoginDatabase { get; set; }
    }
}
