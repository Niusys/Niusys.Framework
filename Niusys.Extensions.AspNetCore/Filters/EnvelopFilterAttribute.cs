using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Niusys.Extensions.AspNetCore.Sessions;
using Niusys.Extensions.ResponseEnvelopes;
using System.Text.Json;

namespace Niusys.Extensions.AspNetCore.Filters
{
    /// <summary>
    /// 消息封包拦截器, 正常处理消息(未发生Exception)由这里出去
    /// </summary>
    public class EnvelopFilterAttribute : ActionFilterAttribute, IActionFilter, IAsyncActionFilter, IResultFilter
    {
        public override void OnActionExecuted(ActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception != null)    //如果有Exception发生，直接跳过处理
            {
                return;
            }

            var requestSession = actionExecutedContext.HttpContext.RequestServices.GetService<IRequestSession>();
            if (!requestSession.IsApiRequest)
                return;

            if (actionExecutedContext.Result is JsonResult jsonResultContent)
            {
                var envelopMessage = new EnvelopMessage<object>(jsonResultContent.Value);
                actionExecutedContext.Result = new OkObjectResult(envelopMessage);
            }
            else if (actionExecutedContext.Result is ObjectResult content)
            {
                if (content.Value is JsonDocument)
                {
                    var jsonDocument = content.Value as JsonDocument;
                    var envelopMessage = new EnvelopMessage<JsonElement>(jsonDocument.RootElement);
                    actionExecutedContext.Result = new OkObjectResult(envelopMessage);
                }
                else
                {
                    var envelopMessage = new EnvelopMessage<object>(content.Value);
                    actionExecutedContext.Result = new OkObjectResult(envelopMessage);
                }
            }
            else if (actionExecutedContext.Result is EmptyResult)
            {
                var envelopMessage = new EnvelopMessage((int)ApiStatusCode.Success);
                actionExecutedContext.Result = new OkObjectResult(envelopMessage);
            }
        }
    }
}
