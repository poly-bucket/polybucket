using Microsoft.Extensions.DependencyInjection;

namespace PolyBucket.Api.Features.Filaments;

public static class FilamentsDependencyInjectionMapping
{
    public static IServiceCollection AddFilamentsFeature(this IServiceCollection services)
    {
        Register<CreateFilament.Domain.ICreateFilamentService, CreateFilament.Domain.CreateFilamentService,
            CreateFilament.Repository.ICreateFilamentRepository, CreateFilament.Repository.CreateFilamentRepository>(services);
        Register<UpdateFilament.Domain.IUpdateFilamentService, UpdateFilament.Domain.UpdateFilamentService,
            UpdateFilament.Repository.IUpdateFilamentRepository, UpdateFilament.Repository.UpdateFilamentRepository>(services);
        Register<DeleteFilament.Domain.IDeleteFilamentService, DeleteFilament.Domain.DeleteFilamentService,
            DeleteFilament.Repository.IDeleteFilamentRepository, DeleteFilament.Repository.DeleteFilamentRepository>(services);
        Register<GetAllFilaments.Domain.IGetAllFilamentsService, GetAllFilaments.Domain.GetAllFilamentsService,
            GetAllFilaments.Repository.IGetAllFilamentsRepository, GetAllFilaments.Repository.GetAllFilamentsRepository>(services);
        Register<GetFilamentById.Domain.IGetFilamentByIdService, GetFilamentById.Domain.GetFilamentByIdService,
            GetFilamentById.Repository.IGetFilamentByIdRepository, GetFilamentById.Repository.GetFilamentByIdRepository>(services);
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
