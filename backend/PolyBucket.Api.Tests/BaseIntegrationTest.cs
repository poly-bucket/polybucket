using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api;
using PolyBucket.Database.Persistence;
using PolyBucket.Api.Features.Authentication.Login.Domain;
using PolyBucket.Api.Tests.Factories;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using PolyBucket.Database.Entities.Models;
using System.Net.Http.Headers;
using PolyBucket.Storage;
using Xunit;

namespace PolyBucket.Api.Tests
{
    [Trait("Category", "Integration")]
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
            Factory = testFixture.Factory;
        }

        protected virtual async Task ResetStateAsync()
        {
            Factory.EmailTransport.Clear();
            Factory.PreviewGenerator.Reset();
            await CleanupDatabaseAsync();
        }

        private async Task CleanupDatabaseAsync()
        {
            try
            {
                EnsureTestHost();
                var ctx = _dbContext!;
                await ctx.Database.ExecuteSqlRawAsync(IntegrationTestDatabaseReset.TruncateMutableDataSql);

                await TestDatabaseManager.ReseedRolePermissionsIfEmptyAsync(ctx);
                await TestStorageSettingsSeeder.SeedAsync(ctx);
                var storageResolver = _serviceScope!.ServiceProvider.GetService<IStorageSettingsResolver>();
                storageResolver?.Invalidate();
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

        public ValueTask DisposeAsync()
        {
            _httpClient?.Dispose();
            _serviceScope?.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
