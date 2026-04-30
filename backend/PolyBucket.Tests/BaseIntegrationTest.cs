using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Authentication.Login.Domain;
using PolyBucket.Tests.Factories;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Models;
using System.Net.Http.Headers;
using Xunit;

namespace PolyBucket.Tests
{
    public abstract class BaseIntegrationTest : IAsyncDisposable
    {
        protected TestWebApplicationFactory Factory { get; }
        protected TestCollectionFixture TestFixture { get; }
        private IServiceScope? _serviceScope;
        private bool _hostInitialized;
        private PolyBucketDbContext? _dbContext;
        private HttpClient? _httpClient;
        private TestUserFactory? _userFactory;
        private TestModelFactory? _modelFactory;

        private void EnsureTestHost()
        {
            if (_hostInitialized) return;
            _hostInitialized = true;
            _serviceScope = Factory.Services.CreateScope();
            _dbContext = _serviceScope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
            _userFactory = new TestUserFactory(_dbContext);
            _modelFactory = new TestModelFactory(_dbContext);
            _httpClient = Factory.CreateClient();
        }

        protected IServiceScope ServiceScope
        {
            get
            {
                EnsureTestHost();
                return _serviceScope!;
            }
        }

        protected HttpClient Client
        {
            get
            {
                EnsureTestHost();
                return _httpClient!;
            }
        }

        protected PolyBucketDbContext DbContext
        {
            get
            {
                EnsureTestHost();
                return _dbContext!;
            }
        }

        protected TestUserFactory UserFactory
        {
            get
            {
                EnsureTestHost();
                return _userFactory!;
            }
        }

        protected TestModelFactory ModelFactory
        {
            get
            {
                EnsureTestHost();
                return _modelFactory!;
            }
        }

        protected BaseIntegrationTest(TestCollectionFixture testFixture)
        {
            TestFixture = testFixture;
            Factory = new TestWebApplicationFactory();
        }

        protected virtual async Task ResetStateAsync()
        {
            await CleanupDatabaseAsync();
        }

        private async Task CleanupDatabaseAsync()
        {
            try
            {
                EnsureTestHost();
                var ctx = _dbContext!;
                await ctx.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Users\" CASCADE");
                await ctx.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Models\" CASCADE");
                await ctx.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"ModelVersions\" CASCADE");
                await ctx.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"ModelFiles\" CASCADE");
                await ctx.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Collections\" CASCADE");
                await ctx.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"CollectionModels\" CASCADE");
                await ctx.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Comments\" CASCADE");
                await ctx.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"UserSettings\" CASCADE");
                await ctx.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"SystemSettings\" CASCADE");
                
                try { await ctx.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Filaments\" CASCADE"); } catch { }
                try { await ctx.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Printers\" CASCADE"); } catch { }
                try { await ctx.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Reports\" CASCADE"); } catch { }
                try { await ctx.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"UserRoles\" CASCADE"); } catch { }
                try { await ctx.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"RolePermissions\" CASCADE"); } catch { }
                try { await ctx.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"UserPermissions\" CASCADE"); } catch { }

                try { await ctx.Database.ExecuteSqlRawAsync("ALTER SEQUENCE IF EXISTS \"Users_Id_seq\" RESTART WITH 1"); } catch { }
                try { await ctx.Database.ExecuteSqlRawAsync("ALTER SEQUENCE IF EXISTS \"Models_Id_seq\" RESTART WITH 1"); } catch { }
                try { await ctx.Database.ExecuteSqlRawAsync("ALTER SEQUENCE IF EXISTS \"ModelVersions_Id_seq\" RESTART WITH 1"); } catch { }
                try { await ctx.Database.ExecuteSqlRawAsync("ALTER SEQUENCE IF EXISTS \"ModelFiles_Id_seq\" RESTART WITH 1"); } catch { }
                try { await ctx.Database.ExecuteSqlRawAsync("ALTER SEQUENCE IF EXISTS \"Collections_Id_seq\" RESTART WITH 1"); } catch { }
                try { await ctx.Database.ExecuteSqlRawAsync("ALTER SEQUENCE IF EXISTS \"Comments_Id_seq\" RESTART WITH 1"); } catch { }
                try { await ctx.Database.ExecuteSqlRawAsync("ALTER SEQUENCE IF EXISTS \"Filaments_Id_seq\" RESTART WITH 1"); } catch { }
                try { await ctx.Database.ExecuteSqlRawAsync("ALTER SEQUENCE IF EXISTS \"Printers_Id_seq\" RESTART WITH 1"); } catch { }
                try { await ctx.Database.ExecuteSqlRawAsync("ALTER SEQUENCE IF EXISTS \"Reports_Id_seq\" RESTART WITH 1"); } catch { }
                try { await ctx.Database.ExecuteSqlRawAsync("ALTER SEQUENCE IF EXISTS \"UserSettings_Id_seq\" RESTART WITH 1"); } catch { }
                try { await ctx.Database.ExecuteSqlRawAsync("ALTER SEQUENCE IF EXISTS \"SystemSettings_Id_seq\" RESTART WITH 1"); } catch { }

                await TestDatabaseManager.ReseedRolePermissionsIfEmptyAsync(ctx);
                ctx.ChangeTracker.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Database cleanup failed: {ex.Message}");
            }
        }

        protected async Task<User> CreateTestUser(string? email = null, string password = "TestPassword123!")
        {
            return await UserFactory.CreateTestUser(email, password);
        }

        protected async Task<string> GetAuthToken(string email, string password)
        {
            Client.DefaultRequestHeaders.Authorization = null;

            var loginCommand = new LoginCommand
            {
                EmailOrUsername = email,
                Password = password
            };

            var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginCommand);

            if (!loginResponse.IsSuccessStatusCode)
                return string.Empty;

            var json = await loginResponse.Content.ReadAsStringAsync();
            return ExtractAccessTokenFromLoginJson(json);
        }

        protected static async Task<string> GetAuthTokenWithClient(HttpClient client, string email, string password)
        {
            client.DefaultRequestHeaders.Authorization = null;

            var loginCommand = new LoginCommand
            {
                EmailOrUsername = email,
                Password = password
            };

            var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginCommand);

            if (!loginResponse.IsSuccessStatusCode)
                return string.Empty;

            var json = await loginResponse.Content.ReadAsStringAsync();
            return ExtractAccessTokenFromLoginJson(json);
        }

        private static string ExtractAccessTokenFromLoginJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return string.Empty;

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var requires2Fa = false;
            foreach (var prop in root.EnumerateObject())
            {
                if (prop.Name.Equals("requiresTwoFactor", StringComparison.OrdinalIgnoreCase)
                    && prop.Value.ValueKind == JsonValueKind.True)
                {
                    requires2Fa = true;
                    break;
                }
            }

            if (requires2Fa)
                return string.Empty;

            foreach (var prop in root.EnumerateObject())
            {
                if (prop.Name.Equals("token", StringComparison.OrdinalIgnoreCase)
                    && prop.Value.ValueKind == JsonValueKind.String)
                {
                    var token = prop.Value.GetString();
                    return string.IsNullOrEmpty(token) ? string.Empty : token;
                }
            }

            return string.Empty;
        }

        protected HttpRequestMessage GetAuthHeaders(string token)
        {
            var request = new HttpRequestMessage();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return request;
        }

        protected void SetAuthHeaders(string token)
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async ValueTask DisposeAsync()
        {
            if (_httpClient is not null)
            {
                _httpClient.Dispose();
            }
            _serviceScope?.Dispose();
            if (Factory is IAsyncDisposable ad)
            {
                await ad.DisposeAsync();
            }
            else
            {
                Factory?.Dispose();
            }
        }
    }
}
