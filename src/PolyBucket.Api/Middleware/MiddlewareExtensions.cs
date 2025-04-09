using Microsoft.AspNetCore.Builder;

namespace PolyBucket.Api.Middleware
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseDatabaseConnectionTest(this IApplicationBuilder app)
        {
            return app.UseMiddleware<DatabaseConnectionTestMiddleware>();
        }
    }
} 