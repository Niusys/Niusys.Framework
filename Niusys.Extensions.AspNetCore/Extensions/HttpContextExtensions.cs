using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Niusys.Extensions.AspNetCore.Extensions
{
    public static class HttpContextExtensions
    {
        private static Regex _apiRequestPattern = new Regex("^/api", RegexOptions.IgnoreCase);


        public static bool IsApiRequest(this HttpRequest httpRequest)
        {
            if (httpRequest is null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            if (!httpRequest.Path.HasValue)
            {
                return false;
            }

            return _apiRequestPattern.IsMatch(httpRequest.Path);
        }

        public static void SafeAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary is null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

        public static T GetHeaderValueAs<T>(this HttpRequest httpRequest, string headerName)
        {
            if (httpRequest?.Headers?.TryGetValue(headerName, out var values) ?? false)
            {
                string rawValues = values.ToString();   // writes out as Csv when there are multiple.

                if (!rawValues.IsNullOrEmpty())
                {
                    return (T)Convert.ChangeType(values.ToString(), typeof(T));
                }
            }
            return default(T);
        }

        public static T GetContextItemValueAs<T>(this HttpContext httpContext, string itemName)
        {
            object values = null;
            if (httpContext?.Items?.TryGetValue(itemName, out values) ?? false)
            {
                if (values != null)
                {
                    return (T)Convert.ChangeType(values, typeof(T));
                }
            }
            return default(T);
        }

        #region IsStaticResource
        private const string _staticFileNameSuffix = @"\.jpg$|\.js$|\.png$|\.woff2$|\.css$|\.ico$|\.map$";
        public static bool IsStaticResource(this HttpRequest httpRequest)
        {
            if (httpRequest is null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            return Regex.IsMatch(httpRequest.Path.Value, _staticFileNameSuffix);
        }

        public static bool IsStaticFileName(this string fileName)
        {
            return Regex.IsMatch(fileName, _staticFileNameSuffix);
        } 
        #endregion
    }
}
