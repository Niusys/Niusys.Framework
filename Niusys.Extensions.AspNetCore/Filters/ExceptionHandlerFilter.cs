using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Niusys.Extensions.AspNetCore.Exceptions;
using Niusys.Extensions.AspNetCore.Sessions;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Niusys.Extensions.AspNetCore.Filters
{
    /// <summary>
    /// API所有异常截拦器, 所有在执行过程中出现的Exception由这里处理
    /// </summary>
    public class ExceptionHandlerFilter : ExceptionFilterAttribute, IExceptionFilter, IAsyncActionFilter
    {
        //https://weblog.west-wind.com/posts/2016/oct/16/error-handling-and-exceptionfilter-dependency-injection-for-aspnet-core-apis
        public override void OnException(ExceptionContext context)
        {
            var logger = context.HttpContext.RequestServices.GetService<ILogger<ExceptionHandlerFilter>>();
            var requestSession = context.HttpContext.RequestServices.GetRequiredService<IRequestSession>();
            if (!requestSession.IsApiRequest)
                return;

            context.HttpContext.Response.StatusCode = 200;
            EnvelopMessage messageEnvelop = null;
            if (context.Exception is OperationCanceledException)
            {
                //logger.LogError(context.Exception.FullMessage());
                messageEnvelop = new EnvelopMessage((int)ApiStatusCode.RequestCancelled);
                context.ExceptionHandled = true;
            }
            //else if (context.Exception is ApiAuthenticationException)
            //{
            //    //logger.LogError(context.Exception.FullMessage());
            //    var ex = context.Exception as ApiAuthenticationException;
            //    messageEnvelop = new EnvelopMessage(ex.Code, ex.Message, ex.FriendlyMessage);
            //}
            else if (context.Exception is ModelValidateException)
            {
                //logger.LogError(context.Exception.FullMessage());
                var modelValidationException = (ModelValidateException)context.Exception;
                //只拿到有ErrorMessage
                var allInvalidItems = modelValidationException.ValidateMessage.Where(x => x.Value.ErrorMessage.IsNotNullOrWhitespace()).Select(x => x.Value.ErrorMessage);
                var errorMessage = modelValidationException.ValidateMessage.Select(x => new KeyValuePair<string, string>(x.Key, x.Value.ErrorMessage.IsNullOrWhitespace() ? x.Value.Exception.FullMessage() : x.Value.ErrorMessage));

                messageEnvelop = new EnvelopMessage(modelValidationException.Code,
                     debugMessage: errorMessage.Any() ? string.Join("|", errorMessage.Select(x => $"{x.Key}:{x.Value}")) : null,
                     hintMessage: allInvalidItems.Any() ? string.Join(",", allInvalidItems) : null);
            }
            else if (context.Exception is ApiException)
            {
                //logger.LogError(context.Exception.FullMessage());
                // handle explicit 'known' API errors
                var ex = context.Exception as ApiException;
                messageEnvelop = new EnvelopMessage(ex.Code, ex.HintMessage, ex.Message);
            }
            else if (context.Exception is DbException)
            {
                //logger.LogError(context.Exception.FullMessage());
                var ex = context.Exception as DbException;
                messageEnvelop = new EnvelopMessage((int)ApiStatusCode.DatabaseOperationFail,
                    debugMessage: $@"数据库访问异常
{ex.FullMessage()}
{ex.FullStacktrace()}
{ex.ToString()}");
            }
            else
            {
                logger.LogError($"Tid:{requestSession.Tid} ErrorMessage:{context.Exception.FullMessage()}, ExceptionType:{context.Exception.GetType().FullName},StackTrace:{context.Exception.FullStacktrace()}");
                messageEnvelop = new EnvelopMessage((int)ApiStatusCode.GeneralError, context.Exception.FullMessage(), "服务器处理异常");
            }

            messageEnvelop.Tid = requestSession.Tid;

            // always return a JSON result
            context.Result = new JsonResult(messageEnvelop);
            context.ExceptionHandled = true;
        }

        /// <summary>
        /// 处理验证, 对于所有验证失败的请求，直接抛出ModelValidateException
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var modelState = context.ModelState;
            if (!modelState.IsValid)
            {
                throw new ModelValidateException(modelState);
            }
            await next();
        }
    }
}
