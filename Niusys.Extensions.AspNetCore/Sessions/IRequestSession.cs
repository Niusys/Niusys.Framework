using Niusys.Extensions.Storage.Mongo;

namespace Niusys.Extensions.AspNetCore.Sessions
{
    public interface IRequestSession
    {
        string ClientIp { get; }
        string Host { get; }
        bool IsApiRequest { get; }
        string Schema { get; }
        string Tid { get; }
    }
}