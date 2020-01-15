using Newtonsoft.Json;

namespace Niusys.Extensions.ResponseEnvelopes
{
    public class EnvelopMessage<T> : EnvelopMessageAbstract
    {
        public EnvelopMessage()
        {

        }

        public EnvelopMessage(int code, string msg, string errMsg, T data)
            : base(code, msg, errMsg)
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
