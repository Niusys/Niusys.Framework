using Newtonsoft.Json;

namespace Niusys.Extensions.ResponseEnvelopes
{
    public class EnvelopMessage<T> : EnvelopMessage
    {
        public EnvelopMessage()
        {

        }

        public EnvelopMessage(int code, string errorMessage, string friendlyMessage, T data)
            : base(code, errorMessage, friendlyMessage)
        {
            Data = data;
        }

        public EnvelopMessage(T data)
        {
            Data = data;
        }

        [JsonProperty(PropertyName = "data")]
        public new T Data { get; set; }
    }
}
