using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace PolyBucket.Api.Extensions;

public static class OpenTelemetryServiceCollectionExtensions
{
    public static IServiceCollection AddPolyBucketOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var otelConfig = configuration.GetSection("OpenTelemetry");
        var serviceName = otelConfig["ServiceName"] ?? "polybucket-api";
        var serviceVersion = otelConfig["ServiceVersion"] ?? "1.0.0";
        var otlpEndpoint = otelConfig["OtlpEndpoint"] 
            ?? Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") 
            ?? "http://localhost:4317";
        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion,
                    serviceInstanceId: Environment.MachineName)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = environment
                }))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.EnrichWithHttpRequest = (activity, request) =>
                    {
                        activity.SetTag("http.request.method", request.Method);
                        activity.SetTag("http.request.path", request.Path);
                    };
                    options.EnrichWithHttpResponse = (activity, response) =>
                    {
                        activity.SetTag("http.response.status_code", response.StatusCode);
                    };
                    options.EnrichWithException = (activity, exception) =>
                    {
                        activity.SetTag("exception.type", exception.GetType().Name);
                        activity.SetTag("exception.message", exception.Message);
                    };
                })
                .AddEntityFrameworkCoreInstrumentation(options =>
                {
                    options.SetDbStatementForText = true;
                    options.SetDbStatementForStoredProcedure = true;
                })
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                    options.Protocol = OtlpExportProtocol.Grpc;
                }))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                }));

        return services;
    }
}
