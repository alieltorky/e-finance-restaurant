using Serilog;
using Serilog.Events;

namespace Online_Restaurant.Extensions
{
    public static class LoggingExtensions
    {
        public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
        {
            Serilog.Debugging.SelfLog.Enable(msg => Console.Error.WriteLine($"SERILOG ERROR: {msg}"));
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .WriteTo.Console()
                .WriteTo.File(
                    path: "Logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Message:lj}-{Timestamp:yyyy-MM-dd HH:mm:ss}{NewLine}{Exception}"
                )
                .CreateLogger();

            builder.Host.UseSerilog();
            return builder;
        }

        public static WebApplication UseCostumeSerilogRequestLogging(this WebApplication app)
        {
            // Logs the "Request" line the moment a request arrives - but only
            // for routes worth seeing in the log
            app.Use(async (context, next) =>
            {
                string path = context.Request.Path.Value ?? string.Empty;

                if (!IsNoisyPath(path))
                {
                    string user = context.User.Identity?.Name ?? "Guest";
                    Log.Information("{Method}-{User}-{Type}", context.Request.Method, user, "Request");
                }

                await next();
            });

            app.UseSerilogRequestLogging(options =>
            {
                // Skip static assets and other low-value noise
                options.GetLevel = (httpContext, elapsed, ex) =>
                    IsNoisyPath(httpContext.Request.Path.Value ?? string.Empty)
                        ? LogEventLevel.Verbose
                        : LogEventLevel.Information;

                // Extract user identity
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("User", httpContext.User.Identity?.Name ?? "Guest");
                };

                // Logs the "Response" line once the request has finished
                options.MessageTemplate = "{RequestMethod}-{User}-Response";
            });

            return app;
        }

        // Shared by both the Request and Response loggers, so "important" means
        // the same thing on the way in as it does on the way out.
        private static bool IsNoisyPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            // Static assets: css, js, images, fonts, favicon, etc.
            if (path.Contains('.'))
            {
                return true;
            }

            // Frequently-polled, low-value application endpoints
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
//verbose - debug - info - warning - error - fetal