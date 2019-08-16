namespace Niusys.Security.Tokens
{
    public interface IPackable
    {
        ByteBuf marshal(ByteBuf outBuf);
        void unmarshal(ByteBuf inBuf);
    }
}
