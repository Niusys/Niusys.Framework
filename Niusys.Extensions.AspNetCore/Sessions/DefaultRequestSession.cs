using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Niusys.Extensions.AspNetCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Niusys.Extensions.AspNetCore.Sessions
{
    /// <summary>
    /// 根据请求上下文获取请求相关信息
    /// </summary>
    public class DefaultRequestSession : IRequestSession
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostEnvironment _hostingEnvironment;

        public DefaultRequestSession(IHttpContextAccessor httpContextAccessor, IHostEnvironment hostingEnvironment)
        {
            this._httpContextAccessor = httpContextAccessor;
            this._hostingEnvironment = hostingEnvironment;
        }

        public string Tid => GetContextItemValueAs<string>(ContextItemsNames.Tid);

        public bool IsApiRequest => GetContextItemValueAs<bool>(ContextItemsNames.IsApiRequest);

        public string ClientIp => GetClientIp(true);

        #region 解析客户端真实IP
        private string GetClientIp(bool tryUseXForwardHeader = true)
        {
            string ip = null;

            // todo support new "Forwarded" header (2014) https://en.wikipedia.org/wiki/X-Forwarded-For

            // X-Forwarded-For (csv list):  Using the First entry in the list seems to work
            // for 99% of cases however it has been suggested that a better (although tedious)
            // approach might be to read each IP from right to left and use the first public IP.
            // http://stackoverflow.com/a/43554000/538763
            //
            if (tryUseXForwardHeader)
            {
                ip = _httpContextAccessor.HttpContext.Request.GetHeaderValueAs<string>("X-Forwarded-For").SplitCsv().FirstOrDefault();
            }

            // RemoteIpAddress is always null in DNX RC1 Update1 (bug).
            if (ip.IsNullOrWhitespace() && _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress != null)
            {
                ip = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }

            if (ip.IsNullOrWhitespace())
            {
                ip = _httpContextAccessor.HttpContext.Request.GetHeaderValueAs<string>("REMOTE_ADDR");
            }

            // _httpContextAccessor.HttpContext?.Request?.Host this is the local host.
            if (ip.IsNullOrWhitespace() && (_hostingEnvironment.IsEnvironment("UnitTesting") || _hostingEnvironment.IsEnvironment("IntegrationTesting")))
            {
                ip = "127.0.0.1";
            }

            if (ip.IsNullOrWhitespace())
            {
                const string message = "Unable to determine caller's IP.";
#pragma warning disable CA1303 // 请不要将文本作为本地化参数传递
                throw new Exception(message);
#pragma warning restore CA1303 // 请不要将文本作为本地化参数传递
            }

            return ip;
        }
        #endregion

        public string Schema
        {
            get
            {
                var schema = _httpContextAccessor.HttpContext.Request.GetHeaderValueAs<string>("X-Forwarded-Proto");
                if (schema.IsNullOrWhitespace())
                    schema = _httpContextAccessor.HttpContext.Request.Scheme;
                return schema;
            }
        }

        public string Host => $"{Schema}://{_httpContextAccessor.HttpContext.Request.Host}";

        #region Http Header & Context Items解析方法


        private T GetContextItemValueAs<T>(string itemName)
        {
            object values = null;
            if (_httpContextAccessor.HttpContext?.Items?.TryGetValue(itemName, out values) ?? false)
            {
                if (values != null)
                {
                    return (T)Convert.ChangeType(values, typeof(T));
                }
            }
            return default(T);
        }
        #endregion
    }
}
