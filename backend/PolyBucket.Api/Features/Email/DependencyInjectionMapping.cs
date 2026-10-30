using Microsoft.Extensions.DependencyInjection;

namespace PolyBucket.Api.Features.Email;

public static class EmailApiDependencyInjectionMapping
{
    public static IServiceCollection AddEmailApiFeature(this IServiceCollection services)
    {
        Register<GetEmailSettings.Domain.IGetEmailSettingsService, GetEmailSettings.Domain.GetEmailSettingsService,
            GetEmailSettings.Repository.IGetEmailSettingsRepository, GetEmailSettings.Repository.GetEmailSettingsRepository>(services);
        Register<UpdateEmailSettings.Domain.IUpdateEmailSettingsService, UpdateEmailSettings.Domain.UpdateEmailSettingsService,
            UpdateEmailSettings.Repository.IUpdateEmailSettingsRepository, UpdateEmailSettings.Repository.UpdateEmailSettingsRepository>(services);
        Register<TestEmailConfiguration.Domain.ITestEmailConfigurationService, TestEmailConfiguration.Domain.TestEmailConfigurationService,
            TestEmailConfiguration.Repository.ITestEmailConfigurationRepository, TestEmailConfiguration.Repository.TestEmailConfigurationRepository>(services);
        Register<GetEmailOutbox.Domain.IGetEmailOutboxService, GetEmailOutbox.Domain.GetEmailOutboxService,
            GetEmailOutbox.Repository.IGetEmailOutboxRepository, GetEmailOutbox.Repository.GetEmailOutboxRepository>(services);
        Register<RetryEmailMessage.Domain.IRetryEmailMessageService, RetryEmailMessage.Domain.RetryEmailMessageService,
            RetryEmailMessage.Repository.IRetryEmailMessageRepository, RetryEmailMessage.Repository.RetryEmailMessageRepository>(services);
        Register<PreviewEmailTemplate.Domain.IPreviewEmailTemplateService, PreviewEmailTemplate.Domain.PreviewEmailTemplateService,
            PreviewEmailTemplate.Repository.IPreviewEmailTemplateRepository, PreviewEmailTemplate.Repository.PreviewEmailTemplateRepository>(services);
        return services;
    }

    private static void Register<TService, TServiceImpl, TRepository, TRepositoryImpl>(IServiceCollection services)
        where TService : class where TServiceImpl : class, TService
        where TRepository : class where TRepositoryImpl : class, TRepository
    {
        services.AddTransient<TService, TServiceImpl>();
        services.AddTransient<TRepository, TRepositoryImpl>();
    }
}
