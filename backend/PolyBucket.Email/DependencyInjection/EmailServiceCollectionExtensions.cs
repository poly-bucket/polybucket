using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using PolyBucket.Email;
using PolyBucket.Email.Templates;
using PolyBucket.Database.Persistence;
using PolyBucket.Email.Domain;
using PolyBucket.Email.Repository;

namespace PolyBucket.Email.DependencyInjection;

public static class EmailServiceCollectionExtensions
{
    public static IServiceCollection AddPolyBucketEmail(this IServiceCollection services, IConfiguration configuration)
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
