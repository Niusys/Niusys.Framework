using System;
using System.Reflection;

namespace Niusys.Extensions.AspNetCore
{
    public static class ApiStatusCodeExtensions
    {
        public static string GetSuggestionHint(this ApiStatusCode code)
        {
            var type = code.GetType();
            var field = type.GetField(Enum.GetName(type, code));
            return field.GetCustomAttribute<StatusCodeDescriptionAttribute>(false)?.SuggestionHint ?? "未说明";
        }

        public static string GetDescription(this ApiStatusCode code)
        {
            var type = code.GetType();
            var field = type.GetField(Enum.GetName(type, code));
            return field.GetCustomAttribute<StatusCodeDescriptionAttribute>(false)?.Description ?? "未说明";
        }
    }
}
