using Microsoft.Extensions.DependencyInjection;

        namespace PolyBucket.Api.Features.Printers;

        public static class PrintersDependencyInjectionMapping
        {
            public static IServiceCollection AddPrintersFeature(this IServiceCollection services)
            {
                services.AddScoped<GetPrinters.Domain.IGetPrintersService, GetPrinters.Domain.GetPrintersService>();
                services.AddTransient<GetPrinters.Repository.IGetPrintersRepository, GetPrinters.Repository.GetPrintersRepository>();
                return services;
            }
        }
