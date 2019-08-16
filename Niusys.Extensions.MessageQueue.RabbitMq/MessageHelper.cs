using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Niusys.Extensions.MessageQueue.RabbitMq
{
    /// <summary>
    ///
    /// </summary>
    public static class MessageHelper
    {
        public static byte[] GetMessage(string message, Encoding _Encoding)
        {
            return _Encoding.GetBytes(message);
        }

        public static byte[] CompressMessage(byte[] messageBytes, CompressionTypes Compression)
        {
            switch (Compression)
            {
                case CompressionTypes.None:
                    return messageBytes;
                case CompressionTypes.GZip:
                    return CompressMessageGZip(messageBytes);
                default:
                    throw new Exception($"Compression type '{Compression}' not supported.");
            }
        }

        /// <summary>
        /// Compresses bytes using GZip data format
        /// </summary>
        /// <param name="messageBytes"></param>
        /// <returns></returns>
        private static byte[] CompressMessageGZip(byte[] messageBytes)
        {
            var gzipCompressedMemStream = new MemoryStream();
            using (var gzipStream = new GZipStream(gzipCompressedMemStream, CompressionMode.Compress))
            {
                gzipStream.Write(messageBytes, 0, messageBytes.Length);
            }

            return gzipCompressedMemStream.ToArray();
        }
    }
}
