namespace Niusys
{
    public abstract class EnvelopMessageAbstract
    {
        public EnvelopMessageAbstract() : this(200, null, null)
        {
        }

        public EnvelopMessageAbstract(int code, string hintMessage = null, string debugMessage = null)
        {
            Code = code;
            Msg = hintMessage;
            ErrMsg = debugMessage;
        }

        /// <summary>
        /// 处理状态代码
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 处理状态代码对应描述，可展示给最终用户
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 程序详细异常信息，不可展示给最终用户
        /// </summary>
        public string ErrMsg { get; set; }

        /// <summary>
        /// 1请求跟踪代码
        /// </summary>
        public string Tid { get; set; }
    }
}
