using System.Collections.Generic;
using System.Linq;

namespace Niusys.Security.Tokens
{
    public class ClaimContent : Dictionary<string, string>, IPackable
    {
        public ClaimContent()
        {

        }
        public ByteBuf marshal(ByteBuf outBuf)
        {
            var bytesDict = this.ToDictionary(key => key.Key.GetByteArray(), value => value.Value.GetByteArray());
            return outBuf.putBytesMap(bytesDict);
        }

        public void unmarshal(ByteBuf inBuf)
        {
            var buytesDict = inBuf.readBytesMap();
            foreach (var item in buytesDict)
            {
                this.Add(item.Key.FromByteArray(), item.Value.FromByteArray());
            }
        }
    }
}
