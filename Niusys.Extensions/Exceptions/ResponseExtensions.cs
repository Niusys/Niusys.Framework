using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;

namespace Niusys
{
    public static class ResponseExtensions
    {
        public static async Task WriteContent(this Stream stream, string jsonContent)
        {
            var sw = new StreamWriter(stream);
            await sw.WriteAsync(jsonContent);
            await sw.FlushAsync();
            stream.SafeSeekToBegin();
        }

        public static async Task WriteContent(this Stream stream, JObject jObject, JsonSerializerSettings jsonSerializerSettings)
        {
            await WriteContent(stream, JsonConvert.SerializeObject(jObject, jsonSerializerSettings));
        }

        public static async Task WriteContent(this Stream stream, object obj, JsonSerializerSettings jsonSerializerSettings)
        {
            await WriteContent(stream, JsonConvert.SerializeObject(obj, jsonSerializerSettings));
        }
    }
}
