using Microsoft.Extensions.DependencyInjection;

namespace PolyBucket.Api.Features.Search;

public static class SearchDependencyInjectionMapping
{
    public static IServiceCollection AddSearchFeature(this IServiceCollection services)
    {
        services.AddScoped<Search.Domain.ISearchService, Search.Domain.SearchService>();
        services.AddScoped<SearchModels.Domain.ISearchModelsService, SearchModels.Domain.SearchModelsService>();
        services.AddTransient<SearchModels.Repository.ISearchModelsRepository, SearchModels.Repository.SearchModelsRepository>();
        services.AddTransient<Repository.ISearchRepository, Repository.SearchRepository>();
        services.AddSingleton<Repository.ISearchCapabilities, Repository.SearchCapabilities>();
        services.AddHostedService<Repository.SearchCapabilityStartupCheck>();
        return services;
    }
}
