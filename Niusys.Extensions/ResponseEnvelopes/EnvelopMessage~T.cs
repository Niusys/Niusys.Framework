using Newtonsoft.Json;

namespace Niusys
{
    public class EnvelopMessage<T> : EnvelopMessageAbstract
    {
        public EnvelopMessage()
        {

        }

        public EnvelopMessage(int code, string hintMessage, string debugMessage, T data)
            : base(code, hintMessage, debugMessage)
        {
            Data = data;
        }

        public EnvelopMessage(T data)
        {
            Data = data;
        }

        public T Data { get; set; }
    }
}
