namespace Niusys.Security.Tokens
{
    public class TokenPackContent : IPackable
    {
        public byte[] signature;
        public byte[] uid;
        public byte[] uname;
        public ushort utype;
        public uint ts;
        public byte[] _messageRawContent;

        public TokenPackContent()
        {
        }

        public TokenPackContent(byte[] signature, byte[] uid, byte[] uname, ushort utype, uint ts, byte[] messageRawContent)
        {
            this.signature = signature;
            this.uid = uid;
            this.uname = uname;
            this.utype = utype;
            this.ts = ts;
            this._messageRawContent = messageRawContent;
        }


        public ByteBuf marshal(ByteBuf outBuf)
        {
            return outBuf.put(signature).put(uid).put(uname).put(utype).put(ts).put(_messageRawContent);
        }

        public void unmarshal(ByteBuf inBuf)
        {
            this.signature = inBuf.readBytes();
            this.uid = inBuf.readBytes();
            this.uname = inBuf.readBytes();
            this.utype = inBuf.readShort();
            this.ts = inBuf.readInt();
            this._messageRawContent = inBuf.readBytes();
        }
    }
}
