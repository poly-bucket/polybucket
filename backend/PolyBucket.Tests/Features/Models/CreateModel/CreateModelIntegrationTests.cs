using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.Models.Domain.Enums;
using PolyBucket.Tests.Factories;
using Shouldly;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PolyBucket.Tests.Features.Models.CreateModel;

[Collection("TestCollection")]
public class CreateModelIntegrationTests : BaseIntegrationTest
{
    public CreateModelIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact]
    public async Task CreateModel_WithValidRequest_ReturnsCreated()
    {
        // Arrange
        await InitializeAsync();
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser();
        
        // Ensure the user is saved to the database
        await DbContext.SaveChangesAsync();
        
        var token = await GetAuthTokenAsync(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent();
        content.Add(new StringContent("Test Model"), "Name");
        content.Add(new StringContent("Test Description"), "Description");
        content.Add(new StringContent("Public"), "Privacy");
        content.Add(new StringContent("CCBy4"), "License");

        // Act
        var response = await client.PostAsync("/api/models", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CreateModelResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.ShouldNotBeNull();
        result.Model.ShouldNotBeNull();
        result.Model.Id.ShouldNotBe(Guid.Empty);
        result.Model.Name.ShouldBe("Test Model");
        result.Model.Description.ShouldBe("Test Description");
        result.Model.Privacy.ShouldBe(PrivacySettings.Public);
        result.Model.License.ShouldBe(LicenseTypes.CCBy4);
        result.Model.AuthorId.ShouldBe(user.Id);
    }

    [Fact]
    public async Task CreateModel_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await InitializeAsync();
        var client = Factory.CreateClient();
        var content = new MultipartFormDataContent();
        content.Add(new StringContent("Test Model"), "Name");

        // Act
        var response = await client.PostAsync("/api/models", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateModel_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        await InitializeAsync();
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser();
        
        var token = await GetAuthTokenAsync(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent();
        content.Add(new StringContent(""), "Name");
        content.Add(new StringContent("Test Description"), "Description");

        // Act
        var response = await client.PostAsync("/api/models", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateModel_WithoutFiles_ReturnsBadRequest()
    {
        // Arrange
        await InitializeAsync();
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser();
        
        var token = await GetAuthTokenAsync(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent();
        content.Add(new StringContent("Test Model"), "Name");
        content.Add(new StringContent("Test Description"), "Description");

        // Act
        var response = await client.PostAsync("/api/models", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateModel_WithInvalidFileType_ReturnsBadRequest()
    {
        // Arrange
        await InitializeAsync();
        var client = Factory.CreateClient();
        var user = await UserFactory.CreateTestUser();
        
        var token = await GetAuthTokenAsync(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent();
        content.Add(new StringContent("Test Model"), "Name");
        content.Add(new StringContent("Test Description"), "Description");

        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("Test file content"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, "Files", "test.txt");

        // Act
        var response = await client.PostAsync("/api/models", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private async Task<string> GetAuthTokenAsync(HttpClient client, string email, string password)
    {
        var loginContent = new StringContent(
            JsonSerializer.Serialize(new { Email = email, Password = password }),
            Encoding.UTF8,
            "application/json");

        var loginResponse = await client.PostAsync("/api/auth/login", loginContent);
        
        if (!loginResponse.IsSuccessStatusCode)
        {
            var errorContent = await loginResponse.Content.ReadAsStringAsync();
            throw new Exception($"Login failed with status {loginResponse.StatusCode}: {errorContent}");
        }
        
        var loginResult = await loginResponse.Content.ReadAsStringAsync();
        
        if (string.IsNullOrWhiteSpace(loginResult))
        {
            throw new Exception("Login response was empty");
        }
        
        var loginData = JsonSerializer.Deserialize<LoginResponse>(loginResult, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (loginData?.AccessToken == null)
        {
            throw new Exception($"Failed to deserialize login response: {loginResult}");
        }

        return loginData.AccessToken;
    }

    private class CreateModelResponse
    {
        public ModelResponse Model { get; set; } = new();
    }

    private class ModelResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PrivacySettings Privacy { get; set; }
        public LicenseTypes License { get; set; }
        public Guid AuthorId { get; set; }
    }

    private class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
    }
} 