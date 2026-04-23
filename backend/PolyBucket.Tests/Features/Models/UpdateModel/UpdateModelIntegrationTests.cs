using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api;
using PolyBucket.Api.Data;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Tests.Factories;
using Shouldly;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PolyBucket.Tests.Features.Models.UpdateModel;

public class UpdateModelIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly TestUserFactory _userFactory;
    private readonly TestModelFactory _modelFactory;

    public UpdateModelIntegrationTests(WebApplicationFactory<Program> factory)
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
                    options.UseInMemoryDatabase("UpdateModelTestDb");
                });
            });
        });

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
        _userFactory = new TestUserFactory(dbContext);
        _modelFactory = new TestModelFactory(dbContext);
    }

    [Fact]
    public async Task UpdateModel_WithValidRequest_ReturnsOk()
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

        var updateRequest = new
        {
            Name = "Updated Model Name",
            Description = "Updated Description",
            Privacy = PrivacySettings.Private.ToString(),
            License = LicenseTypes.MIT.ToString()
        };

        var content = new StringContent(
            JsonSerializer.Serialize(updateRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await client.PutAsync($"/api/models/{model.Id}", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<UpdateModelResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.ShouldNotBeNull();
        result.Id.ShouldBe(model.Id);
        result.Name.ShouldBe("Updated Model Name");
        result.Description.ShouldBe("Updated Description");
        result.Privacy.ShouldBe(PrivacySettings.Private);
        result.License.ShouldBe(LicenseTypes.MIT);
    }

    [Fact]
    public async Task UpdateModel_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var updateRequest = new { Name = "Updated Name" };
        var content = new StringContent(
            JsonSerializer.Serialize(updateRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await client.PutAsync("/api/models/1", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateModel_WithNonExistentModel_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var user = await _userFactory.CreateTestUser();
        
        var token = await GetAuthTokenAsync(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new { Name = "Updated Name" };
        var content = new StringContent(
            JsonSerializer.Serialize(updateRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await client.PutAsync("/api/models/999", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateModel_WithUnauthorizedUser_ReturnsForbidden()
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

        var updateRequest = new { Name = "Updated Name" };
        var content = new StringContent(
            JsonSerializer.Serialize(updateRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await client.PutAsync($"/api/models/{model.Id}", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateModel_WithEmptyName_ReturnsBadRequest()
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

        var updateRequest = new { Name = "" };
        var content = new StringContent(
            JsonSerializer.Serialize(updateRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await client.PutAsync($"/api/models/{model.Id}", content);

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

    private class UpdateModelResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PrivacySettings Privacy { get; set; }
        public LicenseTypes License { get; set; }
        public Guid AuthorId { get; set; }
    }
} 