using Serilog.Events;

namespace Api.Extensions
{
    public class Logging
    {
        public static LogEventLevel SetLogLevel(string logLevel)
        {
            return logLevel.ToLower() switch
            {
                "debug" => LogEventLevel.Debug,
                "information" => LogEventLevel.Information,
                "warning" => LogEventLevel.Warning,
                "error" => LogEventLevel.Error,
                "fatal" => LogEventLevel.Fatal,
                _ => throw new ArgumentException("Invalid log level", nameof(logLevel))
            };
        }
    }
}