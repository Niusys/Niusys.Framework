using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Niusys
{
    public static class DeepCloneExtension
    {
        public static T DeepCloneBySerialize<T>(this T obj)
            where T : class
        {
            BinaryFormatter bFormatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                bFormatter.Serialize(stream, obj);
                stream.Seek(0, SeekOrigin.Begin);
                return bFormatter.Deserialize(stream) as T;
            }
        }
    }
}
