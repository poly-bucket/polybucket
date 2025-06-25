using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Features.Models.Queries;
using PolyBucket.Api.Features.Models.Repository;
using PolyBucket.Api.Features.Users.Queries;
using PolyBucket.Api.Features.Users.Commands;
using PolyBucket.Api.Features.Users.Repository;
using MediatR;

namespace PolyBucket.Api.Extensions
{
    public static class FeatureExtensions
    {
        public static IServiceCollection AddFeatures(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
        
            // Models
            services.AddTransient<GetModelsQueryHandler>();
            services.AddTransient<GetModelByIdQueryHandler>();
            services.AddTransient<IModelsRepository, ModelsRepository>();

            // Users
            services.AddTransient<GetUserByIdQueryHandler>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<GetUserSettingsQueryHandler>();
            services.AddTransient<UpdateUserSettingsCommandHandler>();

            return services;
        }
    }
} 