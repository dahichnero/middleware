using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace DITwo
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class SpecialDateGetterMiddleware
    {
        private readonly RequestDelegate _next;
        private ISpecialDateGetter specialDateGetter;
        public SpecialDateGetterMiddleware (RequestDelegate next, ISpecialDateGetter specialDateGetter)
        {
            this._next = next;
            this.specialDateGetter = specialDateGetter;
        }

        public async Task Invoke (HttpContext httpContext)
        {
            httpContext.Response.Headers.ContentType = "text/html; charset=utf8";
            await httpContext.Response.WriteAsync(specialDateGetter.GetSpecialDate());
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class SpecialDateGetterMiddlewareExtensions
    {
        public static IApplicationBuilder UseSpecialDateGetterMiddleware (this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SpecialDateGetterMiddleware>();
        }
    }
}
