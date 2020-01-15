using Newtonsoft.Json;

namespace Niusys.Extensions.ResponseEnvelopes
{
    public class EnvelopMessage : EnvelopMessageAbstract
    {
        public EnvelopMessage()
        {
        }

        public EnvelopMessage(int code, string msg = null, string errMsg = null) 
            : base(code, msg, errMsg)
        {
        }

        /// <summary>
        /// 固定传string.Empty(根据接口约定)
        /// </summary>
        public string Data { get; set; } = string.Empty;
    }
}
