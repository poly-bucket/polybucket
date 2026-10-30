using Microsoft.Extensions.DependencyInjection;

namespace PolyBucket.Api.Features.ThemeManagement;

public static class ThemeManagementDependencyInjectionMapping
{
    public static IServiceCollection AddThemeManagementFeature(this IServiceCollection services)
    {
        Register<CreateTheme.Domain.ICreateThemeService, CreateTheme.Domain.CreateThemeService,
            CreateTheme.Repository.ICreateThemeRepository, CreateTheme.Repository.CreateThemeRepository>(services);
        Register<GetActiveTheme.Domain.IGetActiveThemeService, GetActiveTheme.Domain.GetActiveThemeService,
            GetActiveTheme.Repository.IGetActiveThemeRepository, GetActiveTheme.Repository.GetActiveThemeRepository>(services);
        Register<GetThemes.Domain.IGetThemesService, GetThemes.Domain.GetThemesService,
            GetThemes.Repository.IGetThemesRepository, GetThemes.Repository.GetThemesRepository>(services);
        Register<SetActiveTheme.Domain.ISetActiveThemeService, SetActiveTheme.Domain.SetActiveThemeService,
            SetActiveTheme.Repository.ISetActiveThemeRepository, SetActiveTheme.Repository.SetActiveThemeRepository>(services);
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
