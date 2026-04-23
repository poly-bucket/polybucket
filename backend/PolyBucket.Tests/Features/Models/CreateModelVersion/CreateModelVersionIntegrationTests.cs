using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api;
using PolyBucket.Api.Data;
using PolyBucket.Api.Common.Models;
using PolyBucket.Tests.Factories;
using Shouldly;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PolyBucket.Tests.Features.Models.CreateModelVersion;

public class CreateModelVersionIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly TestUserFactory _userFactory;
    private readonly TestModelFactory _modelFactory;

    public CreateModelVersionIntegrationTests(WebApplicationFactory<Program> factory)
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
                    options.UseInMemoryDatabase("CreateModelVersionTestDb");
                });
            });
        });

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
        _userFactory = new TestUserFactory(dbContext);
        _modelFactory = new TestModelFactory(dbContext);
    }

    [Fact]
    public async Task CreateModelVersion_WithValidRequest_ReturnsCreated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var user = await _userFactory.CreateTestUser();
        var model = await _modelFactory.CreateTestModel(user.Id);
        
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
        dbContext.Models.Add(model);
        await dbContext.SaveChangesAsync();

        var token = await GetAuthTokenAsync(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent();
        content.Add(new StringContent("2.0"), "VersionNumber");
        content.Add(new StringContent("Updated version"), "Notes");

        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("Test file content"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "Files", "test-v2.stl");

        // Act
        var response = await client.PostAsync($"/api/models/{model.Id}/versions", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CreateModelVersionResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.ShouldNotBeNull();
        result.Id.ShouldBeGreaterThan(0);
        result.VersionNumber.ShouldBe("2.0");
        result.Notes.ShouldBe("Updated version");
        result.ModelId.ShouldBe(model.Id);
    }

    [Fact]
    public async Task CreateModelVersion_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var content = new MultipartFormDataContent();
        content.Add(new StringContent("2.0"), "VersionNumber");

        // Act
        var response = await client.PostAsync("/api/models/1/versions", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateModelVersion_WithNonExistentModel_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var user = await _userFactory.CreateTestUser();
        
        var token = await GetAuthTokenAsync(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent();
        content.Add(new StringContent("2.0"), "VersionNumber");

        // Act
        var response = await client.PostAsync("/api/models/999/versions", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateModelVersion_WithUnauthorizedUser_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateClient();
        var owner = await _userFactory.CreateTestUser("owner@test.com");
        var unauthorizedUser = await _userFactory.CreateTestUser("unauthorized@test.com");
        var model = await _modelFactory.CreateTestModel(owner.Id);
        
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
        dbContext.Models.Add(model);
        await dbContext.SaveChangesAsync();

        var token = await GetAuthTokenAsync(client, unauthorizedUser.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent();
        content.Add(new StringContent("2.0"), "VersionNumber");

        // Act
        var response = await client.PostAsync($"/api/models/{model.Id}/versions", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateModelVersion_WithoutFiles_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var user = await _userFactory.CreateTestUser();
        var model = await _modelFactory.CreateTestModel(user.Id);
        
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
        dbContext.Models.Add(model);
        await dbContext.SaveChangesAsync();

        var token = await GetAuthTokenAsync(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent();
        content.Add(new StringContent("2.0"), "VersionNumber");
        content.Add(new StringContent("Updated version"), "Notes");

        // Act
        var response = await client.PostAsync($"/api/models/{model.Id}/versions", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateModelVersion_WithInvalidFileType_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var user = await _userFactory.CreateTestUser();
        var model = await _modelFactory.CreateTestModel(user.Id);
        
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
        dbContext.Models.Add(model);
        await dbContext.SaveChangesAsync();

        var token = await GetAuthTokenAsync(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent();
        content.Add(new StringContent("2.0"), "VersionNumber");
        content.Add(new StringContent("Updated version"), "Notes");

        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("Test file content"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, "Files", "test.txt");

        // Act
        var response = await client.PostAsync($"/api/models/{model.Id}/versions", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
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

    private class CreateModelVersionResponse
    {
        public int Id { get; set; }
        public string VersionNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public Guid ModelId { get; set; }
    }
} 