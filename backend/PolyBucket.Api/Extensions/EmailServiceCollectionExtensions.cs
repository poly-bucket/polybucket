using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Common.Email.Templates;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Email.Domain;
using PolyBucket.Api.Features.Email.Repository;

namespace PolyBucket.Api.Extensions;

public static class EmailServiceCollectionExtensions
{
    public static IServiceCollection AddEmail(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<EmailOptions>, EmailOptionsValidator>();
        services.AddOptions<EmailOptions>()
            .BindConfiguration(EmailOptions.SectionName)
            .ValidateOnStart();

        services.AddDataProtection()
            .SetApplicationName("PolyBucket")
            .PersistKeysToDbContext<PolyBucketDbContext>();

        services.AddMemoryCache();
        services.TryAddSingleton(TimeProvider.System);

        services.AddSingleton<ISmtpPasswordProtector, SmtpPasswordProtector>();
        services.AddScoped<IEmailSettingsResolver, EmailSettingsResolver>();
        services.AddSingleton<IEmailTransport, LogEmailTransport>();
        services.AddSingleton<IEmailTransport, SmtpEmailTransport>();
        services.AddSingleton<IEmailTransportFactory, EmailTransportFactory>();
        services.AddSingleton<IEmailTemplateRenderer, EmailTemplateRenderer>();
        services.AddScoped<IEmailBrandingProvider, EmailBrandingProvider>();

        services.AddSingleton<IEmailDispatchSignal, EmailDispatchSignal>();
        services.AddScoped<IEmailOutboxRepository, EmailOutboxRepository>();
        services.AddScoped<IEmailQueue, EmailQueue>();
        services.AddScoped<IAccountEmailService, AccountEmailService>();
        services.AddScoped<IEmailDispatcher, EmailDispatcher>();
        services.AddHostedService<EmailDispatcherHostedService>();

        services.AddHealthChecks()
            .AddCheck<EmailTransportHealthCheck>("email", HealthStatus.Degraded, ["email"]);

        return services;
    }
}
