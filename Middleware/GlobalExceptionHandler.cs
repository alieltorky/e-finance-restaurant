using Microsoft.AspNetCore.Diagnostics;

namespace Online_Restaurant.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,Exception exception,CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Unhandled exception on {Path}", httpContext.Request.Path);

            bool wantsJson =
                httpContext.Request.Headers.Accept.ToString().Contains("application/json")
                || httpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (wantsJson)
            {
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError; //HTTP 500 Internal Server Error
                httpContext.Response.ContentType = "application/json";

                await httpContext.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "An unexpected error occurred. Please try again.",
                    requestId = httpContext.TraceIdentifier //search your logs for that request ID and find the actual exception.
                }, cancellationToken);
            }
            else
            {
                httpContext.Response.Redirect("/Home/Error");
            }

            return true; 
        }
    }
}