using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Features.Email.GetEmailOutbox.Domain;
using PolyBucket.Api.Features.Email.GetEmailSettings.Domain;
using PolyBucket.Api.Features.Email.PreviewEmailTemplate.Domain;
using PolyBucket.Api.Features.Email.RetryEmailMessage.Domain;
using PolyBucket.Api.Features.Email.TestEmailConfiguration.Domain;
using PolyBucket.Api.Features.Email.TestEmailConfiguration.Repository;
using PolyBucket.Api.Features.Email.UpdateEmailSettings.Domain;
using PolyBucket.Api.Features.Email.UpdateEmailSettings.Repository;

namespace PolyBucket.Api.Features.Email;

public static class DependencyInjectionMapping
{
    public static IServiceCollection AddEmailFeature(this IServiceCollection services)
    {
        services.AddTransient<IGetEmailSettingsService, GetEmailSettingsService>();

        services.AddTransient<IUpdateEmailSettingsRepository, UpdateEmailSettingsRepository>();
        services.AddTransient<IUpdateEmailSettingsService, UpdateEmailSettingsService>();

        services.AddTransient<ITestEmailConfigurationRepository, TestEmailConfigurationRepository>();
        services.AddTransient<ITestEmailConfigurationService, TestEmailConfigurationService>();

        services.AddTransient<IGetEmailOutboxService, GetEmailOutboxService>();
        services.AddTransient<IRetryEmailMessageService, RetryEmailMessageService>();
        services.AddTransient<IPreviewEmailTemplateService, PreviewEmailTemplateService>();

        return services;
    }
}
