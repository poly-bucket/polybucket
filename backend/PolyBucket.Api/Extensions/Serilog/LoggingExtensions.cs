using Serilog.Events;

namespace PolyBucket.Api.Extensions.Serilog;

public static class LoggingExtensions
{
    public static LogEventLevel SetLogLevel(string logLevel)
    {
        return logLevel.ToLower() switch
        {
            "trace" => LogEventLevel.Verbose,
            "debug" => LogEventLevel.Debug,
            "information" => LogEventLevel.Information,
            "warning" => LogEventLevel.Warning,
            "error" => LogEventLevel.Error,
            "critical" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }
} 