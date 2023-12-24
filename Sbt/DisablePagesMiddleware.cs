using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Sbt;

public class DisablePagesMiddleware
{
    private readonly RequestDelegate _next;

    public DisablePagesMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if the request is for a disabled page
        if (IsDisabledPage(context.Request.Path))
        {
            // Return a 404 Not Found response
            context.Response.StatusCode = 404;
            return;
        }

        // Continue with the request pipeline
        await _next(context);
    }

    private bool IsDisabledPage(PathString path)
    {
        // only allow home page
        var isHomePage = path.HasValue && path.Value.Equals("/", StringComparison.OrdinalIgnoreCase);

        return !isHomePage;
    }
}
