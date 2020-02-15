using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Niusys.Extensions.AspNetCore.Extensions;
using Niusys.Extensions.AspNetCore.Sessions;
using Niusys.Utils;
using System.Threading.Tasks;

namespace Niusys.Extensions.AspNetCore.Middlewares
{
    public class WebRequestSessionMiddleware
    {
        private readonly JsonSerializerSettings _jsonSetting;

        private RequestDelegate Next { get; }
        private ILogger<WebRequestSessionMiddleware> _logger;

        public WebRequestSessionMiddleware(RequestDelegate next, ILogger<WebRequestSessionMiddleware> logger)
        {
            Next = next;
            _logger = logger;
            _jsonSetting = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
        }

        public async Task Invoke(HttpContext context, IRequestSession requestSession)
        {
            if (!context.Request.IsApiRequest())
            {
                await Next(context);
                return;
            }

            //if (context.Request.Headers.ContainsKey(EnvelopDefaults.IgnoreEnvelopKey))
            //{
            //    context.Items.Add(EnvelopDefaults.IgnoreEnvelopKey, context.Request.Headers[EnvelopDefaults.IgnoreEnvelopKey].ToString() ?? false.ToString());
            //}
            //标记当前请求是API请求
            context.Items[ContextItemsNames.IsApiRequest] = true;
            _logger.LogInformation($"API开始处理");

            //处理TID
            _logger.LogDebug($"处理TID");
            var tid = requestSession.Tid;
            if (tid.IsNullOrWhitespace())
            {
                tid = GuidGenerator.GenerateDigitalUUID();
                context.Items[ContextItemsNames.Tid] = tid;
            }

            await Next(context);
        }
    }
}
