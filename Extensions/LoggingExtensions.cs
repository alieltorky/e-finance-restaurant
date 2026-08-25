using Serilog;

namespace Online_Restaurant.Extensions
{
    public static class LoggingExtensions
    {
        public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File(
                path: "Logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss}[{Level:u3}]{Message:lj}{NewLine}{Exception}"
                ).CreateLogger();
            builder.Host.UseSerilog();
            return builder;

        }
        public static WebApplication UseCostumeSerilogRequestLogging(this WebApplication app)
        {
            app.UseSerilogRequestLogging();
            return app;
        }


    }
}
