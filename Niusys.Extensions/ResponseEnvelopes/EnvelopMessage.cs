using Newtonsoft.Json;

namespace Niusys
{
    public class EnvelopMessage : EnvelopMessageAbstract
    {
        public EnvelopMessage()
        {
        }

        public EnvelopMessage(int code, string hintMessage = null, string debugMessage = null) 
            : base(code, hintMessage, debugMessage)
        {
        }

        /// <summary>
        /// 固定传string.Empty(根据接口约定)
        /// </summary>
        public string Data { get; set; } = string.Empty;
    }
}
