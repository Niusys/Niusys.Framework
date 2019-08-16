using System.IO;
using System.Text;

namespace Niusys.Security.Tokens
{
    internal static class MemoryStreamExtensions
    {
        public static void write(this MemoryStream obj, string data)
        {
            var array = Encoding.UTF8.GetBytes(data);
            obj.Write(array, (int)obj.Length, array.Length);
        }

        public static byte[] GetByteArray(this string obj)
        {
            return Encoding.UTF8.GetBytes(obj);
        }
        public static string FromByteArray(this byte[] byteArray)
        {
            return Encoding.UTF8.GetString(byteArray);
        }
    }
}
