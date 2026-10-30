using Microsoft.Extensions.DependencyInjection;

        namespace PolyBucket.Api.Features.Files;

        public static class FilesDependencyInjectionMapping
        {
            public static IServiceCollection AddFilesFeature(this IServiceCollection services)
            {
                Register<GetFileConfig.Domain.IGetFileConfigService, GetFileConfig.Domain.GetFileConfigService,
                    GetFileConfig.Repository.IGetFileConfigRepository, GetFileConfig.Repository.GetFileConfigRepository>(services);
                Register<GetSupportedExtensions.Domain.IGetSupportedExtensionsService, GetSupportedExtensions.Domain.GetSupportedExtensionsService,
                    GetSupportedExtensions.Repository.IGetSupportedExtensionsRepository, GetSupportedExtensions.Repository.GetSupportedExtensionsRepository>(services);
                Register<GetSupportedExtensionsByType.Domain.IGetSupportedExtensionsByTypeService, GetSupportedExtensionsByType.Domain.GetSupportedExtensionsByTypeService,
                    GetSupportedExtensionsByType.Repository.IGetSupportedExtensionsByTypeRepository, GetSupportedExtensionsByType.Repository.GetSupportedExtensionsByTypeRepository>(services);
                Register<StreamFileById.Domain.IStreamFileByIdService, StreamFileById.Domain.StreamFileByIdService,
                    StreamFileById.Repository.IStreamFileByIdRepository, StreamFileById.Repository.StreamFileByIdRepository>(services);
                Register<StreamModelFile.Domain.IStreamModelFileService, StreamModelFile.Domain.StreamModelFileService,
                    StreamModelFile.Repository.IStreamModelFileRepository, StreamModelFile.Repository.StreamModelFileRepository>(services);
                services.AddScoped<Domain.IModelFileAccessEvaluator, Domain.ModelFileAccessEvaluator>();
                services.AddScoped<StreamModelFile.Http.PublicModelAuthorizationFilter>();
                return services;
            }

            private static void Register<TService, TServiceImpl, TRepository, TRepositoryImpl>(IServiceCollection services)
                where TService : class where TServiceImpl : class, TService
                where TRepository : class where TRepositoryImpl : class, TRepository
            {
                services.AddScoped<TService, TServiceImpl>();
                services.AddTransient<TRepository, TRepositoryImpl>();
            }
        }
