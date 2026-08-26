using Serilog;

namespace Online_Restaurant.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string path = context.Request.Path.Value ?? string.Empty;

            if (IsNoisyPath(path))
            {
                await _next(context);
                return;
            }

            string method = context.Request.Method;

            string requestUser = context.User.Identity?.Name ?? "Guest";
            Log.Information("{Method}-{User}-{Type}", method, requestUser, "Request");

            await _next(context);

            // Read the identity again after the pipeline runs — a fresh
            // sign-in during this request could have changed it.
            string responseUser = context.User.Identity?.Name ?? "Guest";
            Log.Information("{Method}-{User}-{Type}", method, responseUser, "Response");
        }

        // Same rules as before: skip static assets and the two
        // frequently-polled endpoints.
        private static bool IsNoisyPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (path.Contains('.'))
            {
                return true;
            }

            if (path.Equals("/Account/IsAuthenticated", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (path.Equals("/Home/GetBestSellers", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}