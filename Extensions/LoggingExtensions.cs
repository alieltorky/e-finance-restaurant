using Serilog;
using Serilog.Events;

namespace Online_Restaurant.Extensions
{
    public static class LoggingExtensions
    {
        public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .WriteTo.File(
                    path: "Logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();

            builder.Host.UseSerilog();
            return builder;
        }

        public static WebApplication UseCostumeSerilogRequestLogging(this WebApplication app)
        {
            app.UseSerilogRequestLogging(options =>
            {
                // Skip static assets
                options.GetLevel = (httpContext, elapsed, ex) =>
                    (httpContext.Request.Path.Value ?? string.Empty).Contains('.')
                        ? LogEventLevel.Verbose
                        : LogEventLevel.Information;

                // Extract user identity
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("User", httpContext.User.Identity?.Name ?? "Guest");
                };

                options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} | User: {User} | Status: {StatusCode} in {Elapsed:0.00} ms";
            });

            return app;
        }
    }
}