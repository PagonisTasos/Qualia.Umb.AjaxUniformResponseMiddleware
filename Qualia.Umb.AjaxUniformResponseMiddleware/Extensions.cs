using System;
using Microsoft.AspNetCore.Http;

namespace Qualia.Umb.AjaxUniformResponseMiddleware
{
    internal static partial class Extensions
    {
        internal static bool IsUniformRequest(this HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Headers == null)
                return false;

            return request.Headers.ContainsKey("X-UMB-UNIFORM");
        }
    }
}
