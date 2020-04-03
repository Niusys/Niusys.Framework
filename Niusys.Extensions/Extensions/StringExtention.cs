using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Niusys
{
    public static class StringExtention
    {
        public static string[] GetSubString(this string thisValue, string regexPattern)
        {
            string[] result = null;
            MatchCollection collection = Regex.Matches(thisValue, regexPattern, RegexOptions.IgnoreCase);
            if (collection.Count > 0)
            {
                result = new string[collection.Count];
                for (int i = 0; i < collection.Count; i++)
                {

                    result[i] = collection[i].Value;
                }
            }
            return result;
        }

        public static string ReplaceString(this string thisValue, string regexPattern, string replacement)
        {
            if (!string.IsNullOrEmpty(thisValue))
            {
                return Regex.Replace(thisValue, regexPattern, replacement, RegexOptions.IgnoreCase);
            }
            return string.Empty;
        }

        public static string GetLimitString(this string thisValue, out bool isTrunked, int frontLength = 512, int endLength = 512)
        {
            isTrunked = false;
            StringBuilder sbString = new StringBuilder(frontLength + endLength + 50);
            if (thisValue.Length <= (frontLength + endLength))
            {
                return thisValue.ToString();

            }
            sbString.Append(thisValue.Substring(0, frontLength));
            sbString.AppendFormat("--trunked data(size:{0})--", thisValue.Length - frontLength - endLength);
            sbString.Append(thisValue.Substring(thisValue.Length - endLength));
            isTrunked = true;
            return sbString.ToString();
        }

        public static string SafeLimitString(this string thisValue, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(thisValue))
                return string.Empty;

            if (thisValue.Length <= maxLength)
                return thisValue;

            return thisValue.Substring(0, maxLength);
        }

        public static bool IsJson(this string input)
        {
            input = input.Trim();
            Predicate<string> IsWellFormed = (cotnent) =>
            {
                try
                {
                    JToken.Parse(input);
                }
                catch
                {
                    return false;
                }
                return true;
            };

            return (input.StartsWith("{") && input.EndsWith("}"))
                    || (input.StartsWith("[") && input.EndsWith("]"))
                   && IsWellFormed(input);
        }

        public static List<string> SplitCsv(this string csvList, bool nullOrWhitespaceInputReturnsNull = false)
        {
            if (string.IsNullOrWhiteSpace(csvList))
                return nullOrWhitespaceInputReturnsNull ? null : new List<string>();

            return csvList
                .TrimEnd(',')
                .Split(',')
                .AsEnumerable<string>()
                .Select(s => s.Trim())
                .ToList();
        }

        public static bool IsNullOrWhitespace(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }
        public static bool IsNotNullOrWhitespace(this string s)
        {
            return !string.IsNullOrWhiteSpace(s);
        }
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }


        public static Regex CommonIdsValidationRegex = new Regex(@"^((\d?)|(([-+]?\d+\.?\d*)|([-+]?\d*\.?\d+))|(([-+]?\d+\.?\d*\,\ ?)*([-+]?\d+\.?\d*))|(([-+]?\d*\.?\d+\,\ ?)*([-+]?\d*\.?\d+))|(([-+]?\d+\.?\d*\,\ ?)*([-+]?\d*\.?\d+))|(([-+]?\d*\.?\d+\,\ ?)*([-+]?\d+\.?\d*)))$");
        public static string CommaDelimiterStringValidation(this string obj)
        {
            if (!CommonIdsValidationRegex.IsMatch(obj))
            {
                throw new Exception("Wrong format of Comma Sparated string");
            }
            return obj;
        }

        /// <summary>
        /// 判断是否是一个合法的整数型逗点分隔符
        /// 比如 "1,2,3,100,280"
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsValidCommaDelimiterString(this string obj)
        {
            return CommonIdsValidationRegex.IsMatch(obj);
        }
    }
}
