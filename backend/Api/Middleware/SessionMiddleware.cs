using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ToolLendingPlatform.Api.Middleware
{
    public class SessionMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // ASP.NET Core session middleware is already configured in Program.cs
            // This placeholder can be extended for custom session logic in future tasks
            await _next(context);
        }
    }
}
