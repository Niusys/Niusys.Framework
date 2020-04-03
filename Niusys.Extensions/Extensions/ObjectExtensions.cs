using Newtonsoft.Json;
using System.IO;

namespace Niusys
{
    public static class ObjectExtensions
    {
        public static Stream CreateStreamFromObject(this object obj)
        {
            obj = obj ?? throw new System.ArgumentNullException(nameof(obj));

            MemoryStream ms = new MemoryStream();
            StreamWriter writer = new StreamWriter(ms);
            writer.Write(JsonConvert.SerializeObject(obj));
            writer.Flush();
            ms.SafeSeekToBegin();
            return ms;
        }

        public static Stream SafeSeekToBegin(this Stream obj)
        {
            if (obj.CanSeek && obj.Position != 0)
            {
                obj.Seek(0, SeekOrigin.Begin);
            }

            return obj;
        }
    }
}
