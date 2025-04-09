using Core.Plugins;
using Core.Plugins.Comments;
using Database.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPlugins(this IServiceCollection services)
        {
            // Register plugin manager as singleton
            services.AddSingleton<PluginManager>(provider =>
            {
                var pluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "plugins");
                return new PluginManager(pluginsPath);
            });

            // Register default plugins
            services.AddScoped<ICommentsPlugin, DefaultCommentsPlugin>();

            return services;
        }
    }
}