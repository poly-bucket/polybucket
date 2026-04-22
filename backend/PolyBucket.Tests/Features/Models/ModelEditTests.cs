using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PolyBucket.Api;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.Shared.Domain;
using PolyBucket.Api.Features.Models.Shared.Domain.Enums;
using PolyBucket.Tests.Factories;
using Xunit;

namespace PolyBucket.Tests.Features.Models
{
    public class ModelEditTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly PolyBucketDbContext _context;
        private readonly TestUserFactory _userFactory;
        private readonly TestModelFactory _modelFactory;

        public ModelEditTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            var scope = factory.Services.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
            _userFactory = new TestUserFactory(_context);
            _modelFactory = new TestModelFactory(_context);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        [Fact]
        public async Task UpdateModel_WithValidData_ShouldUpdateModel()
        {
            // Arrange
            var user = await _userFactory.CreateTestUser();
            var model = await _modelFactory.CreateTestModel("Original Name", "Original Description");
            model.AuthorId = user.Id;
            await _context.SaveChangesAsync();

            var client = _factory.CreateClient();
            var token = await GetAuthToken(client, user.Email, "TestPassword123!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var updateRequest = new
            {
                name = "Updated Name",
                description = "Updated Description",
                license = (int)LicenseTypes.GPLv3,
                privacy = (int)PrivacySettings.Private,
                aiGenerated = true,
                wip = true,
                nsfw = false,
                isRemix = false
            };

            var content = new StringContent(JsonConvert.SerializeObject(updateRequest), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PutAsync($"/api/models/{model.Id}", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var updatedModel = JsonConvert.DeserializeObject<Model>(responseContent);
            
            Assert.NotNull(updatedModel);
            Assert.Equal("Updated Name", updatedModel.Name);
            Assert.Equal("Updated Description", updatedModel.Description);
            Assert.Equal(LicenseTypes.GPLv3, updatedModel.License);
            Assert.Equal(PrivacySettings.Private, updatedModel.Privacy);
            Assert.True(updatedModel.AIGenerated);
            Assert.True(updatedModel.WIP);
            Assert.False(updatedModel.NSFW);
        }

        [Fact]
        public async Task UpdateModel_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            // Arrange
            var user = await _userFactory.CreateTestUser();
            var model = await _modelFactory.CreateTestModel();
            model.AuthorId = user.Id;
            await _context.SaveChangesAsync();

            var client = _factory.CreateClient();
            var updateRequest = new { name = "Updated Name" };
            var content = new StringContent(JsonConvert.SerializeObject(updateRequest), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PutAsync($"/api/models/{model.Id}", content);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UpdateModel_ByNonOwner_ShouldReturnForbidden()
        {
            // Arrange
            var owner = await _userFactory.CreateTestUser("owner@test.com");
            var nonOwner = await _userFactory.CreateTestUser("nonowner@test.com");
            var model = await _modelFactory.CreateTestModel();
            model.AuthorId = owner.Id;
            await _context.SaveChangesAsync();

            var client = _factory.CreateClient();
            var token = await GetAuthToken(client, nonOwner.Email, "TestPassword123!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var updateRequest = new { name = "Updated Name" };
            var content = new StringContent(JsonConvert.SerializeObject(updateRequest), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PutAsync($"/api/models/{model.Id}", content);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UpdateModel_NonExistentModel_ShouldReturnNotFound()
        {
            // Arrange
            var user = await _userFactory.CreateTestUser();
            var client = _factory.CreateClient();
            var token = await GetAuthToken(client, user.Email, "TestPassword123!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var updateRequest = new { name = "Updated Name" };
            var content = new StringContent(JsonConvert.SerializeObject(updateRequest), Encoding.UTF8, "application/json");
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await client.PutAsync($"/api/models/{nonExistentId}", content);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateModel_DeletedModel_ShouldReturnBadRequest()
        {
            // Arrange
            var user = await _userFactory.CreateTestUser();
            var model = await _modelFactory.CreateTestModel();
            model.AuthorId = user.Id;
            model.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var client = _factory.CreateClient();
            var token = await GetAuthToken(client, user.Email, "TestPassword123!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var updateRequest = new { name = "Updated Name" };
            var content = new StringContent(JsonConvert.SerializeObject(updateRequest), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PutAsync($"/api/models/{model.Id}", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateModel_PartialUpdate_ShouldUpdateOnlySpecifiedFields()
        {
            // Arrange
            var user = await _userFactory.CreateTestUser();
            var model = await _modelFactory.CreateTestModel("Original Name", "Original Description");
            model.AuthorId = user.Id;
            model.License = LicenseTypes.MIT;
            model.Privacy = PrivacySettings.Public;
            await _context.SaveChangesAsync();

            var client = _factory.CreateClient();
            var token = await GetAuthToken(client, user.Email, "TestPassword123!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var updateRequest = new { name = "Updated Name" };
            var content = new StringContent(JsonConvert.SerializeObject(updateRequest), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PutAsync($"/api/models/{model.Id}", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var updatedModel = JsonConvert.DeserializeObject<Model>(responseContent);
            
            Assert.NotNull(updatedModel);
            Assert.Equal("Updated Name", updatedModel.Name);
            Assert.Equal("Original Description", updatedModel.Description); // Should remain unchanged
            Assert.Equal(LicenseTypes.MIT, updatedModel.License); // Should remain unchanged
            Assert.Equal(PrivacySettings.Public, updatedModel.Privacy); // Should remain unchanged
        }

        [Fact]
        public async Task UpdateModel_WithRemixData_ShouldUpdateRemixFields()
        {
            // Arrange
            var user = await _userFactory.CreateTestUser();
            var model = await _modelFactory.CreateTestModel();
            model.AuthorId = user.Id;
            await _context.SaveChangesAsync();

            var client = _factory.CreateClient();
            var token = await GetAuthToken(client, user.Email, "TestPassword123!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var updateRequest = new
            {
                isRemix = true,
                remixUrl = "https://example.com/original-model"
            };
            var content = new StringContent(JsonConvert.SerializeObject(updateRequest), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PutAsync($"/api/models/{model.Id}", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var updatedModel = JsonConvert.DeserializeObject<Model>(responseContent);
            
            Assert.NotNull(updatedModel);
            Assert.True(updatedModel.IsRemix);
            Assert.Equal("https://example.com/original-model", updatedModel.RemixUrl);
        }

        private async Task<string> GetAuthToken(HttpClient client, string email, string password)
        {
            var loginData = new
            {
                email = email,
                password = password
            };

            var loginContent = new StringContent(JsonConvert.SerializeObject(loginData), Encoding.UTF8, "application/json");
            var loginResponse = await client.PostAsync("/api/authentication/login", loginContent);
            
            if (loginResponse.IsSuccessStatusCode)
            {
                var loginResult = await loginResponse.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<dynamic>(loginResult);
                return tokenResponse?.accessToken?.ToString();
            }

            throw new InvalidOperationException("Failed to get auth token");
        }
    }
} 