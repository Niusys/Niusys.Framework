using System;
using System.Reflection;

namespace Niusys
{
    public static class ApiStatusCodeExtensions
    {
        public static string GetSuggestionHint<T>(this T code)
            where T : struct
        {
            var type = code.GetType();
            var field = type.GetField(Enum.GetName(type, code));
            return field.GetCustomAttribute<StatusCodeDescriptionAttribute>(false)?.SuggestionHint ?? "未说明";
        }

        public static string GetDescription<T>(this T code)
            where T : struct
        {
            var type = code.GetType();
            var field = type.GetField(Enum.GetName(type, code));
            return field.GetCustomAttribute<StatusCodeDescriptionAttribute>(false)?.Description ?? "未说明";
        }
    }
}
