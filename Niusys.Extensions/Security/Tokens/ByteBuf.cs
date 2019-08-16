using System.Collections.Generic;

namespace Niusys.Security.Tokens
{
    public class ByteBuf
    {
        ByteBuffer buffer = new ByteBuffer();
        public ByteBuf()
        {

        }

        public ByteBuf(byte[] source)
        {
            buffer.PushByteArray(source);
        }

        public byte[] asBytes()
        {
            //byte[] outBuf = new byte[buffer.Position];
            //buffer.rewind();
            //buffer.get(outBuf, 0, out.length);
            //return outBuf;
            return buffer.ToByteArray();
        }

        // packUint16
        public ByteBuf put(ushort v)
        {
            buffer.PushUInt16(v);
            return this;
        }

        // packUint32
        public ByteBuf put(uint v)
        {
            buffer.PushLong(v);
            return this;
        }

        public ByteBuf put(byte[] v)
        {
            put((ushort)v.Length);
            buffer.PushByteArray(v);
            return this;
        }

        public ByteBuf putIntMap(Dictionary<ushort, uint> extra)
        {
            put((ushort)extra.Count);

            foreach (var item in extra)
            {
                put(item.Key);
                put(item.Value);
            }
            return this;
        }

        public ByteBuf putBytesMap(Dictionary<byte[], byte[]> extra)
        {
            put((ushort)extra.Count);

            foreach (var item in extra)
            {
                put(item.Key);
                put(item.Value);
            }
            return this;
        }

        public ushort readShort()
        {
            return buffer.PopUInt16();
        }

        public uint readInt()
        {
            return buffer.PopUInt();
        }

        public byte[] readBytes()
        {
            ushort length = readShort();
            byte[] bytes = new byte[length];
            //buffer.get(bytes);
            //return bytes;
            return buffer.PopByteArray(length);
        }

        public Dictionary<ushort, uint> readIntMap()
        {
            Dictionary<ushort, uint> map = new Dictionary<ushort, uint>();

            ushort length = readShort();

            for (short i = 0; i < length; ++i)
            {
                ushort k = readShort();
                uint v = readInt();
                map.Add(k, v);
            }

            return map;
        }

        public Dictionary<byte[], byte[]> readBytesMap()
        {
            Dictionary<byte[], byte[]> map = new Dictionary<byte[], byte[]>();

            ushort length = readShort();

            for (short i = 0; i < length; ++i)
            {
                byte[] k = readBytes();
                byte[] v = readBytes();
                map.Add(k, v);
            }

            return map;
        }
    }
}
