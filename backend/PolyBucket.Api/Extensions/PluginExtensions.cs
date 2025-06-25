using PolyBucket.Api.Common.Plugins;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.Plugins;

namespace PolyBucket.Api.Extensions
{
    public static class PluginExtensions
    {
        public static IServiceCollection AddPlugins(this IServiceCollection services)
        {
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