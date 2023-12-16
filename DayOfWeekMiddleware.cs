using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace DITwo
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class DayOfWeekMiddleware
    {
        private readonly RequestDelegate _next;
        private IDayOfWeek dayOfWeekService;

        
        public DayOfWeekMiddleware (RequestDelegate next, IDayOfWeek dayOfWeekService)
        {
            this._next = next;
            this.dayOfWeekService = dayOfWeekService;
        }

        public async Task Invoke (HttpContext httpContext)
        {

            httpContext.Response.Headers.ContentType = "text/html; charset=utf8";
            await httpContext.Response.WriteAsync(dayOfWeekService.GetDayOfWeek());
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class DayOfWeekMiddlewareExtensions
    {
        public static IApplicationBuilder UseDayOfWeekMiddleware (this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DayOfWeekMiddleware>();
        }
    }
}
