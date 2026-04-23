using Microsoft.Extensions.DependencyInjection;

namespace PolyBucket.Api.Features.Models;

public static class ModelsDependencyInjectionMapping
{
    public static IServiceCollection AddModelsFeature(this IServiceCollection services)
    {
        services.AddTransient<
            GetModels.Repository.IGetModelsRepository,
            GetModels.Repository.GetModelsRepository>();
        services.AddTransient<
            GetModelById.Repository.IGetModelByIdRepository,
            GetModelById.Repository.GetModelByIdRepository>();
        services.AddTransient<
            GetModelById.Services.IGetModelByIdService,
            GetModelById.Services.GetModelByIdService>();

        services.AddScoped<CreateModel.Domain.CreateModelService>();
        services.AddTransient<
            CreateModel.Repository.ICreateModelRepository,
            CreateModel.Repository.CreateModelRepository>();

        services.AddTransient<
            DeleteAllModels.Domain.IDeleteAllModelsService,
            DeleteAllModels.Domain.DeleteAllModelsService>();
        services.AddTransient<
            DeleteAllModels.Repository.IDeleteAllModelsRepository,
            DeleteAllModels.Repository.DeleteAllModelsRepository>();
        services.AddTransient<
            DeleteAllModels.Repository.IDeleteAllModelsUserRepository,
            DeleteAllModels.Repository.DeleteAllModelsUserRepository>();

        services.AddTransient<
            CreateModelVersion.Domain.ICreateModelVersionService,
            CreateModelVersion.Domain.CreateModelVersionService>();
        services.AddTransient<
            CreateModelVersion.Repository.ICreateModelVersionRepository,
            CreateModelVersion.Repository.CreateModelVersionRepository>();

        services.AddTransient<
            UpdateModel.Domain.IUpdateModelService,
            UpdateModel.Domain.UpdateModelService>();
        services.AddTransient<
            UpdateModel.Repository.IUpdateModelRepository,
            UpdateModel.Repository.UpdateModelRepository>();

        services.AddTransient<
            UpdateModelVersion.Domain.IUpdateModelVersionService,
            UpdateModelVersion.Domain.UpdateModelVersionService>();
        services.AddTransient<
            UpdateModelVersion.Repository.IUpdateModelVersionRepository,
            UpdateModelVersion.Repository.UpdateModelVersionRepository>();

        services.AddTransient<
            DeleteModel.Domain.IDeleteModelService,
            DeleteModel.Domain.DeleteModelService>();
        services.AddTransient<
            DeleteModel.Repository.IDeleteModelRepository,
            DeleteModel.Repository.DeleteModelRepository>();

        services.AddTransient<
            DeleteModelVersion.Domain.IDeleteModelVersionService,
            DeleteModelVersion.Domain.DeleteModelVersionService>();
        services.AddTransient<
            DeleteModelVersion.Repository.IDeleteModelVersionRepository,
            DeleteModelVersion.Repository.DeleteModelVersionRepository>();

        services.AddTransient<
            GetModelByUserId.Domain.IGetModelByUserIdService,
            GetModelByUserId.Domain.GetModelByUserIdService>();
        services.AddTransient<
            GetModelByUserId.Repository.IGetModelByUserIdRepository,
            GetModelByUserId.Repository.GetModelByUserIdRepository>();

        services.AddTransient<
            GetModelPreview.Repository.IModelPreviewRepository,
            GetModelPreview.Repository.ModelPreviewRepository>();
        services.AddTransient<
            GenerateModelPreview.Repository.IGenerateModelPreviewRepository,
            GenerateModelPreview.Repository.GenerateModelPreviewRepository>();
        services.AddTransient<
            GenerateModelPreview.Services.IModelPreviewGenerationService,
            GenerateModelPreview.Services.ModelPreviewGenerationService>();

        services.AddTransient<
            DownloadModel.Repository.IDownloadModelRepository,
            DownloadModel.Repository.DownloadModelRepository>();
        services.AddTransient<
            DownloadModel.Domain.IDownloadModelService,
            DownloadModel.Domain.DownloadModelService>();

        return services;
    }
}
