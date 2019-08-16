namespace Niusys.Extensions.Storage.Mongo
{
    public interface IMongoEntity<T>
    {
        T Sysid { get; set; }
    }
}
