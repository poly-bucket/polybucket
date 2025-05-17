using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Middleware
{
    public class DatabaseConnectionTestMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<DatabaseConnectionTestMiddleware> _logger;

        public DatabaseConnectionTestMiddleware(RequestDelegate next, ILogger<DatabaseConnectionTestMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, Infrastructure.Data.DatabaseContext dbContext)
        {
            // Only trigger on a specific diagnostic endpoint
            if (context.Request.Path.StartsWithSegments("/api/diagnostics/db-test"))
            {
                try
                {
                    // Try to connect to the database and execute a simple query
                    var canConnect = await dbContext.Database.CanConnectAsync();
                    var usersCount = await dbContext.Users.CountAsync();
                    
                    // Return diagnostic information
                    var response = new
                    {
                        DatabaseConnectionSuccessful = canConnect,
                        UsersCount = usersCount,
                        DatabaseProvider = dbContext.Database.ProviderName,
                        Timestamp = DateTime.UtcNow
                    };
                    
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(response);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Database connection test failed");
                    
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new 
                    { 
                        Error = "Database connection test failed",
                        Message = ex.Message,
                        InnerException = ex.InnerException?.Message,
                        ConnectionString = "(masked for security)"
                    });
                    return;
                }
            }

            // Continue the middleware pipeline for other requests
            await _next(context);
        }
    }
} 