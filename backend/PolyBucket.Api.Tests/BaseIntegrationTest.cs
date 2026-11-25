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
using Microsoft.Extensions.Logging.Abstractions;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Api.Seeders;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Entities.SystemSettings;
using System.Net.Http.Headers;
using PolyBucket.Email;
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
                await EnsureUploadFileTypesAsync(ctx);
                await new AnonymousUploadAuthorSeeder(ctx, NullLogger<AnonymousUploadAuthorSeeder>.Instance).SeedAsync();
                if (!await ctx.ModelSettings.AnyAsync())
                {
                    ctx.ModelSettings.Add(new ModelSettings
                    {
                        RequireUploadModeration = false,
                        RequireModelDescription = false,
                        RequireLicenseSelection = false,
                        RequireModeratorApproval = false,
                        RequireModelApproval = false,
                        EnableModelComments = true,
                        EnableModelCollections = true
                    });
                    await ctx.SaveChangesAsync();
                }

                var services = _serviceScope!.ServiceProvider;
                services.GetService<IStorageSettingsResolver>()?.Invalidate();
                services.GetService<ISiteModelFeaturePolicy>()?.Invalidate();
                services.GetService<IModelMetadataPolicy>()?.Invalidate();
                services.GetService<ISitePrivacyState>()?.Invalidate();
                services.GetService<ISiteModelUploadPolicy>()?.Invalidate();
                services.GetService<ISiteModelVersioningPolicy>()?.Invalidate();
                services.GetService<IEmailSettingsResolver>()?.Invalidate();
                ctx.ChangeTracker.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Database cleanup failed: {ex.Message}");
            }
        }

        private static async Task EnsureUploadFileTypesAsync(PolyBucketDbContext ctx)
        {
            if (await ctx.FileTypeSettings.AnyAsync())
                return;

            var adminRole = await ctx.Roles.FirstAsync(r => r.Name == "Admin");
            var seedUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "filetype-seed@polybucket.test",
                Username = "filetype-seed",
                PasswordHash = "seed",
                Salt = "seed",
                RoleId = adminRole.Id,
                EmailVerifiedAt = DateTime.UtcNow,
                HasCompletedFirstTimeSetup = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            ctx.Users.Add(seedUser);
            await ctx.SaveChangesAsync();

            var extensions = new[] { ".stl", ".obj", ".png", ".jpg", ".jpeg", ".gif", ".glb", ".gltf", ".webp" };
            var now = DateTime.UtcNow;
            ctx.FileTypeSettings.AddRange(extensions.Select((extension, index) => new FileTypeSettings
            {
                Id = Guid.NewGuid(),
                FileExtension = extension,
                Enabled = true,
                MaxFileSizeBytes = 500L * 1024 * 1024,
                MaxPerUpload = 10,
                DisplayName = extension,
                Description = extension,
                MimeType = "application/octet-stream",
                Category = "3D",
                Priority = index + 1,
                IsDefault = extension == ".stl",
                CreatedAt = now,
                CreatedById = seedUser.Id,
                UpdatedAt = now,
                UpdatedById = seedUser.Id
            }));
            await ctx.SaveChangesAsync();
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
            var json = await loginResponse.Content.ReadAsStringAsync();

            if (!loginResponse.IsSuccessStatusCode)
                throw new InvalidOperationException(
                    $"Login failed for '{email}' with {(int)loginResponse.StatusCode} {loginResponse.StatusCode}: {TrimBody(json)}");

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
            var json = await loginResponse.Content.ReadAsStringAsync();

            if (!loginResponse.IsSuccessStatusCode)
                throw new InvalidOperationException(
                    $"Login failed for '{email}' with {(int)loginResponse.StatusCode} {loginResponse.StatusCode}: {TrimBody(json)}");

            return ExtractAccessTokenFromLoginJson(json);
        }

        private static string TrimBody(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return "(empty body)";

            const int max = 500;
            return body.Length <= max ? body : body[..max];
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
                    if (!string.IsNullOrEmpty(token))
                        return token;
                }
            }

            throw new InvalidOperationException("Login succeeded but the response did not contain an access token.");
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
