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

namespace PolyBucket.Tests.Features.Models.UpdateModelVersion;

public class UpdateModelVersionIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly TestUserFactory _userFactory;
    private readonly TestModelFactory _modelFactory;

    public UpdateModelVersionIntegrationTests(WebApplicationFactory<Program> factory)
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
                    options.UseInMemoryDatabase("UpdateModelVersionTestDb");
                });
            });
        });

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
        _userFactory = new TestUserFactory(dbContext);
        _modelFactory = new TestModelFactory(dbContext);
    }

    [Fact]
    public async Task UpdateModelVersion_WithValidRequest_ReturnsOk()
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

        var updateRequest = new
        {
            VersionNumber = "2.1",
            Notes = "Updated version notes"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(updateRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await client.PutAsync($"/api/models/{model.Id}/versions/{version.Id}", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<UpdateModelVersionResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.ShouldNotBeNull();
        result.Id.ShouldBe(version.Id);
        result.VersionNumber.ShouldBe("2.1");
        result.Notes.ShouldBe("Updated version notes");
        result.ModelId.ShouldBe(model.Id);
    }

    [Fact]
    public async Task UpdateModelVersion_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var updateRequest = new { VersionNumber = "2.1" };
        var content = new StringContent(
            JsonSerializer.Serialize(updateRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await client.PutAsync("/api/models/1/versions/1", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateModelVersion_WithNonExistentVersion_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var user = await _userFactory.CreateTestUser();
        
        var token = await GetAuthTokenAsync(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new { VersionNumber = "2.1" };
        var content = new StringContent(
            JsonSerializer.Serialize(updateRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await client.PutAsync("/api/models/1/versions/999", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateModelVersion_WithUnauthorizedUser_ReturnsForbidden()
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

        var updateRequest = new { VersionNumber = "2.1" };
        var content = new StringContent(
            JsonSerializer.Serialize(updateRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await client.PutAsync($"/api/models/{model.Id}/versions/{version.Id}", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateModelVersion_WithWrongModelId_ReturnsNotFound()
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

        var updateRequest = new { VersionNumber = "2.1" };
        var content = new StringContent(
            JsonSerializer.Serialize(updateRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await client.PutAsync($"/api/models/{model2.Id}/versions/{version.Id}", content);

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

    private class UpdateModelVersionResponse
    {
        public Guid Id { get; set; }
        public string VersionNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public Guid ModelId { get; set; }
    }
} 