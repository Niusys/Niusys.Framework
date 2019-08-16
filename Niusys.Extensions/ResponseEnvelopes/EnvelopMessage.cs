using Newtonsoft.Json;

namespace Niusys.Extensions.ResponseEnvelopes
{
    public class EnvelopMessage
    {
        [JsonIgnore]
        public const string DEFAULT_ERR_MSG = "后端请求处理异常";

        public EnvelopMessage() : this(200, null, null)
        {
        }

        public EnvelopMessage(int code, string errorMessage = null, string friendlyMessage = null)
        {
            Code = code;
            //todo: 后续增加code安全胡强制转换
            ErrorMessage = errorMessage;
            FriendlyMessage = friendlyMessage;
        }

        /// <summary>
        /// 处理状态代码
        /// </summary>
        [JsonProperty("code")]
        public int Code { get; set; }

        /// <summary>
        /// 处理状态代码对应描述，可展示给最终用户
        /// </summary>
        [JsonProperty("msg")]
        public string FriendlyMessage { get; set; }

        /// <summary>
        /// 程序详细异常信息，不可展示给最终用户
        /// </summary>
        [JsonProperty("err_msg")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 1请求跟踪代码
        /// </summary>
        [JsonProperty("tid")]
        public string Tid { get; set; }

        /// <summary>
        /// 固定传string.Empty(根据接口约定)
        /// </summary>
        [JsonProperty("data")]
        public string Data { get; set; } = string.Empty;
    }
}
