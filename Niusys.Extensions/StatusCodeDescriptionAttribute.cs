using System;

namespace Niusys
{
    /// <summary>
    /// 状态码描述标记
    /// </summary>
    public class StatusCodeDescriptionAttribute : Attribute
    {
        public StatusCodeDescriptionAttribute(string description, string suggestionHint = "")
        {
            Description = description;
            SuggestionHint = suggestionHint;
        }
        /// <summary>
        /// 后端说明，需详细说明Code的适用场景
        /// 这个属性有别于ErrorMessage,
        /// 对于用户名错误的场景: 会提示${UserName}错误, 而不是Description中的信息，如果不指定，会显示Description中的信息
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 前端建议提示语
        /// </summary>
        public string SuggestionHint { get; set; }
    }
}
