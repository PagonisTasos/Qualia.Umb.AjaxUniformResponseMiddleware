using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Qualia.Umb.AjaxUniformResponseMiddleware
{
    internal class ResponseWrapperMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ResponseWrapperMiddleware> _logger;

        public ResponseWrapperMiddleware(RequestDelegate next, ILogger<ResponseWrapperMiddleware> logger)
        {
            _next = next;
            this._logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.IsUniformRequest())
            {
                await ApplyResponseWrapper(context);
            }
            else
            {
                await DoNothing(context);
            }
        }

        private async Task DoNothing(HttpContext context)
        {
            await _next(context);
        }

        private async Task ApplyResponseWrapper(HttpContext context)
        {
            Exception? exception = null;
            //keep a body reference, and use a buffer to capture changes
            var stream = context.Response.Body;
            using var buffer = new MemoryStream();
            context.Response.Body = buffer;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                exception = ex;
                _logger.LogError(ex, "Unhandled Exception with message '{msg}'", ex.Message);
            }
            
            string oldBody = await ReadSteamContent(buffer);
            
            context.Response.Body = stream;
            context.Response.ContentType = System.Net.Mime.MediaTypeNames.Application.Json;
            context.Response.ContentLength = null;

            if (exception == null)
            {
                await RewriteBody(context, oldBody);
            }
            else
            {
                await RewriteBody_wError(context, exception);
            }
        }

        private async Task<string> ReadSteamContent(MemoryStream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        private async Task RewriteBody_wError(HttpContext context, Exception ex)
        {
            var r = new ResponseWrapper
            {
                content = $"{(ex is UserFriendlyException x ? x.UserFriendlyMessage : "Internal Server Error")}",
                statusCode = ex is UserFriendlyException y ? (int)y.StatusCode : (int)System.Net.HttpStatusCode.InternalServerError
            };

            await context.Response.WriteAsync(JsonConvert.SerializeObject(r));
        }

        private async Task RewriteBody(HttpContext context, string bodyContent)
        {
            var statusCode = context.Response.StatusCode;
            var r = new ResponseWrapper { content = bodyContent, statusCode = statusCode };

            await context.Response.WriteAsync(JsonConvert.SerializeObject(r));
        }
    }
}
