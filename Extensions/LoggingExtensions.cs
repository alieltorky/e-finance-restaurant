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

        public static WebApplication UseCustomRequestLogging(this WebApplication app)
        {
            app.UseMiddleware<Online_Restaurant.Middleware.RequestLoggingMiddleware>();
            return app;
        }
    }
}
//verbose - debug - info - warning - error - fetal