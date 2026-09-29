using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Features.ModelModeration.ApproveModel.Domain;
using PolyBucket.Api.Features.ModelModeration.ApproveModel.Repository;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.ModelModeration.GetModelsAwaitingModeration.Domain;
using PolyBucket.Api.Features.ModelModeration.GetModelsAwaitingModeration.Repository;
using PolyBucket.Api.Features.ModelModeration.GetModerationSettings.Domain;
using PolyBucket.Api.Features.ModelModeration.RejectModel.Domain;
using PolyBucket.Api.Features.ModelModeration.RejectModel.Repository;
using PolyBucket.Api.Features.ModelModeration.Repository;
using PolyBucket.Api.Features.ModelModeration.UpdateModerationSettings.Domain;
using PolyBucket.Api.Features.ModelModeration.UpdateModerationSettings.Repository;
using PolyBucket.Api.Features.ModelModeration.ModeratorEditModel.Domain;
using PolyBucket.Api.Features.ModelModeration.ModeratorEditModel.Repository;

namespace PolyBucket.Api.Features.ModelModeration;

public static class DependencyInjectionMapping
{
    public static IServiceCollection AddModelModerationFeature(this IServiceCollection services)
    {
        services.AddTransient<IModelModerationPolicy, ModelModerationPolicy>();
        services.AddTransient<IModelModerationSettingsProvider, ModelModerationSettingsProvider>();
        services.AddTransient<IModelModerationRecordRepository, ModelModerationRecordRepository>();
        services.AddTransient<IModelModerationEnqueueService, ModelModerationEnqueueService>();
        services.AddTransient<IModerationAuditLogWriter, ModerationAuditLogWriter>();

        services.AddTransient<IGetModelsAwaitingModerationRepository, GetModelsAwaitingModerationRepository>();
        services.AddTransient<IGetModelsAwaitingModerationService, GetModelsAwaitingModerationService>();

        services.AddTransient<IApproveModelRepository, ApproveModelRepository>();
        services.AddTransient<IApproveModelService, ApproveModelService>();

        services.AddTransient<IRejectModelRepository, RejectModelRepository>();
        services.AddTransient<IRejectModelService, RejectModelService>();

        services.AddTransient<IGetModerationSettingsService, GetModerationSettingsService>();
        services.AddTransient<IUpdateModerationSettingsRepository, UpdateModerationSettingsRepository>();
        services.AddTransient<IUpdateModerationSettingsService, UpdateModerationSettingsService>();

        services.AddTransient<IModeratorEditModelRepository, ModeratorEditModelRepository>();
        services.AddTransient<IModeratorEditModelService, ModeratorEditModelService>();

        return services;
    }
}
