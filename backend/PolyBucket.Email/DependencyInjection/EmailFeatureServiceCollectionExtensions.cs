using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Email.GetEmailOutbox;
using PolyBucket.Email.GetEmailSettings;
using PolyBucket.Email.PreviewEmailTemplate;
using PolyBucket.Email.RetryEmailMessage;
using PolyBucket.Email.TestEmailConfiguration;
using PolyBucket.Email.UpdateEmailSettings;

namespace PolyBucket.Email.DependencyInjection;

public static class EmailFeatureServiceCollectionExtensions
{
    public static IServiceCollection AddEmailFeatureServices(this IServiceCollection services)
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
