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

namespace PolyBucket.Tests.Features.Models.GetModelByUserId;

public class GetModelByUserIdIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly TestUserFactory _userFactory;
    private readonly TestModelFactory _modelFactory;

    public GetModelByUserIdIntegrationTests(WebApplicationFactory<Program> factory)
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
                    options.UseInMemoryDatabase("GetModelByUserIdTestDb");
                });
            });
        });

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
        _userFactory = new TestUserFactory(dbContext);
        _modelFactory = new TestModelFactory(dbContext);
    }

    [Fact]
    public async Task GetModelByUserId_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        var user = await _userFactory.CreateTestUser();
        var model1 = await _modelFactory.CreateTestModel(user.Id);
        var model2 = await _modelFactory.CreateTestModel(user.Id);
        
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
        dbContext.Models.AddRange(model1, model2);
        await dbContext.SaveChangesAsync();

        var token = await GetAuthTokenAsync(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync($"/api/models/user/{user.Id.ToString()}?page=1&pageSize=10");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<GetModelByUserIdResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.ShouldNotBeNull();
        result.Models.ShouldNotBeNull();
        result.Models.Count().ShouldBe(2);
        result.TotalCount.ShouldBe(2);
    }

    [Fact]
    public async Task GetModelByUserId_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/models/user/1");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetModelByUserId_WithUnauthorizedUser_ReturnsForbidden()
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

        // Act
        var response = await client.GetAsync($"/api/models/user/{owner.Id.ToString()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetModelByUserId_WithPagination_ReturnsCorrectResults()
    {
        // Arrange
        var client = _factory.CreateClient();
        var user = await _userFactory.CreateTestUser();
        var model1 = await _modelFactory.CreateTestModel(user.Id);
        var model2 = await _modelFactory.CreateTestModel(user.Id);
        var model3 = await _modelFactory.CreateTestModel(user.Id);
        
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
        dbContext.Models.AddRange(model1, model2, model3);
        await dbContext.SaveChangesAsync();

        var token = await GetAuthTokenAsync(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync($"/api/models/user/{user.Id.ToString()}?page=1&pageSize=2");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<GetModelByUserIdResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.ShouldNotBeNull();
        result.Models.ShouldNotBeNull();
        result.Models.Count().ShouldBe(2);
        result.TotalCount.ShouldBe(3);
    }

    [Fact]
    public async Task GetModelByUserId_WithNonExistentUser_ReturnsEmptyList()
    {
        // Arrange
        var client = _factory.CreateClient();
        var user = await _userFactory.CreateTestUser();
        
        var token = await GetAuthTokenAsync(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync($"/api/models/user/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<GetModelByUserIdResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.ShouldNotBeNull();
        result.Models.ShouldNotBeNull();
        result.Models.Count().ShouldBe(0);
        result.TotalCount.ShouldBe(0);
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

    private class GetModelByUserIdResponse
    {
        public IEnumerable<ModelSummary> Models { get; set; } = new List<ModelSummary>();
        public int TotalCount { get; set; }
    }

    private class ModelSummary
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid AuthorId { get; set; }
    }
} 