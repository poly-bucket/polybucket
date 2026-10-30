using Microsoft.Extensions.DependencyInjection;

namespace PolyBucket.Api.Features.Federation;

public static class FederationDependencyInjectionMapping
{
    public static IServiceCollection AddFederationFeature(this IServiceCollection services)
    {
        services.AddTransient<Repository.IFederationRepository, Repository.FederationRepository>();
        services.AddTransient<Services.IFederationImportService, Services.FederationImportService>();
        services.AddSingleton<Services.IFederationTokenService, Services.FederationTokenService>();

        Register<CreateFederatedInstance.Domain.ICreateFederatedInstanceService, CreateFederatedInstance.Domain.CreateFederatedInstanceService,
            CreateFederatedInstance.Repository.ICreateFederatedInstanceRepository, CreateFederatedInstance.Repository.CreateFederatedInstanceRepository>(services);
        Register<DeleteFederatedInstance.Domain.IDeleteFederatedInstanceService, DeleteFederatedInstance.Domain.DeleteFederatedInstanceService,
            DeleteFederatedInstance.Repository.IDeleteFederatedInstanceRepository, DeleteFederatedInstance.Repository.DeleteFederatedInstanceRepository>(services);
        Register<GetFederatedInstance.Domain.IGetFederatedInstanceService, GetFederatedInstance.Domain.GetFederatedInstanceService,
            GetFederatedInstance.Repository.IGetFederatedInstanceRepository, GetFederatedInstance.Repository.GetFederatedInstanceRepository>(services);
        Register<GetFederatedInstances.Domain.IGetFederatedInstancesService, GetFederatedInstances.Domain.GetFederatedInstancesService,
            GetFederatedInstances.Repository.IGetFederatedInstancesRepository, GetFederatedInstances.Repository.GetFederatedInstancesRepository>(services);
        Register<UpdateFederatedInstance.Domain.IUpdateFederatedInstanceService, UpdateFederatedInstance.Domain.UpdateFederatedInstanceService,
            UpdateFederatedInstance.Repository.IUpdateFederatedInstanceRepository, UpdateFederatedInstance.Repository.UpdateFederatedInstanceRepository>(services);
        Register<GetFederationHealth.Domain.IGetFederationHealthService, GetFederationHealth.Domain.GetFederationHealthService,
            GetFederationHealth.Repository.IGetFederationHealthRepository, GetFederationHealth.Repository.GetFederationHealthRepository>(services);
        Register<ImportFederatedModel.Domain.IImportFederatedModelService, ImportFederatedModel.Domain.ImportFederatedModelService,
            ImportFederatedModel.Repository.IImportFederatedModelRepository, ImportFederatedModel.Repository.ImportFederatedModelRepository>(services);
        Register<GetFederatedModels.Domain.IGetFederatedModelsService, GetFederatedModels.Domain.GetFederatedModelsService,
            GetFederatedModels.Repository.IGetFederatedModelsRepository, GetFederatedModels.Repository.GetFederatedModelsRepository>(services);
        Register<GetCatalog.Domain.IGetCatalogService, GetCatalog.Domain.GetCatalogService,
            GetCatalog.Repository.IGetCatalogRepository, GetCatalog.Repository.GetCatalogRepository>(services);
        Register<GetCatalogChanges.Domain.IGetCatalogChangesService, GetCatalogChanges.Domain.GetCatalogChangesService,
            GetCatalogChanges.Repository.IGetCatalogChangesRepository, GetCatalogChanges.Repository.GetCatalogChangesRepository>(services);
        Register<InitiateHandshake.Domain.IInitiateHandshakeService, InitiateHandshake.Domain.InitiateHandshakeService,
            InitiateHandshake.Repository.IInitiateHandshakeRepository, InitiateHandshake.Repository.InitiateHandshakeRepository>(services);
        Register<ExchangeToken.Domain.IExchangeTokenService, ExchangeToken.Domain.ExchangeTokenService,
            ExchangeToken.Repository.IExchangeTokenRepository, ExchangeToken.Repository.ExchangeTokenRepository>(services);

        return services;
    }

    private static void Register<TService, TServiceImpl, TRepository, TRepositoryImpl>(IServiceCollection services)
        where TService : class
        where TServiceImpl : class, TService
        where TRepository : class
        where TRepositoryImpl : class, TRepository
    {
        services.AddScoped<TService, TServiceImpl>();
        services.AddTransient<TRepository, TRepositoryImpl>();
    }
}
