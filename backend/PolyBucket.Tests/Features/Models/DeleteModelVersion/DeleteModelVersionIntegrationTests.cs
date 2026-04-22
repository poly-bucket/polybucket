using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.Shared.Domain;
using PolyBucket.Tests.Factories;
using Shouldly;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PolyBucket.Tests.Features.Models.DeleteModelVersion;

public class DeleteModelVersionIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly TestUserFactory _userFactory;
    private readonly TestModelFactory _modelFactory;

    public DeleteModelVersionIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<PolyBucketDbContext>));

                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<PolyBucketDbContext>(options =>
                {
                    options.UseInMemoryDatabase("DeleteModelVersionTestDb");
                });
            });
        });

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
        _userFactory = new TestUserFactory(dbContext);
        _modelFactory = new TestModelFactory(dbContext);
    }

    [Fact]
    public async Task DeleteModelVersion_WithValidRequest_ReturnsNoContent()
    {
        // Arrange
        var client = _factory.CreateClient();
        var user = await _userFactory.CreateTestUser();
        var model = await _modelFactory.CreateTestModel(user.Id);
        var version = await _modelFactory.CreateTestModelVersion(model.Id);
        
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
        dbContext.Models.Add(model);
        dbContext.ModelVersions.Add(version);
        await dbContext.SaveChangesAsync();

        var token = await GetAuthTokenAsync(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.DeleteAsync($"/api/models/{model.Id}/versions/{version.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteModelVersion_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync("/api/models/1/versions/1");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteModelVersion_WithNonExistentVersion_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var user = await _userFactory.CreateTestUser();
        
        var token = await GetAuthTokenAsync(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.DeleteAsync("/api/models/1/versions/999");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteModelVersion_WithUnauthorizedUser_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateClient();
        var owner = await _userFactory.CreateTestUser("owner@test.com");
        var unauthorizedUser = await _userFactory.CreateTestUser("unauthorized@test.com");
        var model = await _modelFactory.CreateTestModel(owner.Id);
        var version = await _modelFactory.CreateTestModelVersion(model.Id);
        
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
        dbContext.Models.Add(model);
        dbContext.ModelVersions.Add(version);
        await dbContext.SaveChangesAsync();

        var token = await GetAuthTokenAsync(client, unauthorizedUser.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.DeleteAsync($"/api/models/{model.Id}/versions/{version.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteModelVersion_WithWrongModelId_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var user = await _userFactory.CreateTestUser();
        var model1 = await _modelFactory.CreateTestModel(user.Id);
        var model2 = await _modelFactory.CreateTestModel(user.Id);
        var version = await _modelFactory.CreateTestModelVersion(model1.Id);
        
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
        dbContext.Models.AddRange(model1, model2);
        dbContext.ModelVersions.Add(version);
        await dbContext.SaveChangesAsync();

        var token = await GetAuthTokenAsync(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.DeleteAsync($"/api/models/{model2.Id}/versions/{version.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteModelVersion_WithAlreadyDeletedVersion_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var user = await _userFactory.CreateTestUser();
        var model = await _modelFactory.CreateTestModel(user.Id);
        var version = await _modelFactory.CreateTestModelVersion(model.Id);
        version.DeletedAt = DateTime.UtcNow;
        version.DeletedById = user.Id;
        
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
        dbContext.Models.Add(model);
        dbContext.ModelVersions.Add(version);
        await dbContext.SaveChangesAsync();

        var token = await GetAuthTokenAsync(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.DeleteAsync($"/api/models/{model.Id}/versions/{version.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private async Task<string> GetAuthTokenAsync(HttpClient client, string email, string password)
    {
        var loginRequest = new
        {
            Email = email,
            Password = password
        };

        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");

        var loginResponse = await client.PostAsync("/api/authentication/login", loginContent);
        
        if (loginResponse.IsSuccessStatusCode)
        {
            var loginResult = await loginResponse.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<LoginResponse>(loginResult, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return tokenResponse?.AccessToken ?? string.Empty;
        }

        return string.Empty;
    }

    private class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
    }
} 