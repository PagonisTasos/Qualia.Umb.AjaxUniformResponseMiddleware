using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.IO;

namespace Qualia.Umb.AjaxUniformResponseMiddleware
{
    internal class ResponseWrapperMiddleware
    {
        private readonly RequestDelegate _next;

        public ResponseWrapperMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.IsAjaxRequest())
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
            Exception exception = null;
            using var buffer = new MemoryStream();
            var stream = context.Response.Body;
            context.Response.Body = buffer;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var bufferReader = new StreamReader(context.Response.Body);
            if (exception == null)
            {
                await RewriteBody(context, bufferReader);
            }
            else
            {
                await RewriteBody_wError(context, exception);
            }
            context.Response.ContentType = System.Net.Mime.MediaTypeNames.Application.Json;

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            await context.Response.Body.CopyToAsync(stream);
            context.Response.Body = stream;
        }

        private async Task RewriteBody_wError(HttpContext context, Exception ex)
        {
            var r = new ResponseWrapper
            {
                content = $"{(ex is UserFriendlyException x ? x.UserFriendlyMessage : "Internal Server Error")}",
                statusCode = ex is UserFriendlyException y ? (int)y.StatusCode : (int)System.Net.HttpStatusCode.InternalServerError
            };

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            await context.Response.WriteAsync(JsonConvert.SerializeObject(r));
        }

        private async Task RewriteBody(HttpContext context, StreamReader bufferReader)
        {
            var statusCode = context.Response.StatusCode;
            string body = await bufferReader.ReadToEndAsync();
            var r = new ResponseWrapper { content = body, statusCode = statusCode };

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            await context.Response.WriteAsync(JsonConvert.SerializeObject(r));
        }
    }
}
