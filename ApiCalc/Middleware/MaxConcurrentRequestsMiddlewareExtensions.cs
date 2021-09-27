using System;
using Microsoft.AspNetCore.Builder;

namespace ApiCalc.Middleware
{
    public static class MaxConcurrentRequestsMiddlewareExtensions
    {
        public static IApplicationBuilder UseMaxConcurrentRequests(this IApplicationBuilder app)
        {
            return app.UseMiddleware<MaxConcurrentRequestsMiddleware>();
        }
    }
}