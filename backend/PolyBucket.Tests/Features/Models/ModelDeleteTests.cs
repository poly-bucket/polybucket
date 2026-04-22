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
using PolyBucket.Api.Features.Models.CreateModel.Domain;
using PolyBucket.Api.Features.Models.Shared.Domain;
using PolyBucket.Tests.Factories;
using Xunit;

namespace PolyBucket.Tests.Features.Models
{
    [Collection("TestCollection")]
    public class ModelDeleteTests : BaseIntegrationTest
    {
        public ModelDeleteTests(TestCollectionFixture testFixture) : base(testFixture)
        {
        }

        [Fact]
        public async Task DeleteModel_ByOwner_ShouldSoftDeleteModel()
        {
            // Arrange
            await ResetStateAsync();
            var user = await UserFactory.CreateTestUser();
            var model = await ModelFactory.CreateTestModel(user.Id);
            await DbContext.SaveChangesAsync();

            var client = Factory.CreateClient();
            var token = await GetAuthToken(client, user.Email, "TestPassword123!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await client.DeleteAsync($"/api/models/{model.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify model is soft deleted
            var deletedModel = await DbContext.Models
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == model.Id);

            Assert.NotNull(deletedModel);
            Assert.NotNull(deletedModel.DeletedAt);
            Assert.Equal(user.Id, deletedModel.DeletedById);
        }

        [Fact]
        public async Task DeleteModel_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            // Arrange
            await ResetStateAsync();
            var user = await UserFactory.CreateTestUser();
            var model = await ModelFactory.CreateTestModel(user.Id);
            await DbContext.SaveChangesAsync();

            var client = Factory.CreateClient();

            // Act
            var response = await client.DeleteAsync($"/api/models/{model.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task DeleteModel_ByNonOwner_ShouldReturnForbidden()
        {
            // Arrange
            await ResetStateAsync();
            var owner = await UserFactory.CreateTestUser("owner@test.com");
            var nonOwner = await UserFactory.CreateTestUser("nonowner@test.com");
            var model = await ModelFactory.CreateTestModel(owner.Id);
            await DbContext.SaveChangesAsync();

            var client = Factory.CreateClient();
            var token = await GetAuthToken(client, nonOwner.Email, "TestPassword123!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await client.DeleteAsync($"/api/models/{model.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task DeleteModel_NonExistentModel_ShouldReturnNotFound()
        {
            // Arrange
            await ResetStateAsync();
            var user = await UserFactory.CreateTestUser();
            var nonExistentId = Guid.NewGuid();

            var client = Factory.CreateClient();
            var token = await GetAuthToken(client, user.Email, "TestPassword123!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await client.DeleteAsync($"/api/models/{nonExistentId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteModel_AlreadyDeletedModel_ShouldReturnBadRequest()
        {
            // Arrange
            await ResetStateAsync();
            var user = await UserFactory.CreateTestUser();
            var model = await ModelFactory.CreateTestModel(user.Id);
            model.DeletedAt = DateTime.UtcNow;
            model.DeletedById = user.Id;
            await DbContext.SaveChangesAsync();

            var client = Factory.CreateClient();
            var token = await GetAuthToken(client, user.Email, "TestPassword123!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await client.DeleteAsync($"/api/models/{model.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteModel_WithFiles_ShouldSoftDeleteModelAndFiles()
        {
            // Arrange
            await ResetStateAsync();
            var user = await UserFactory.CreateTestUser();
            var model = await ModelFactory.CreateTestModel(user.Id);
            
            // Add some files to the model
            var modelFile = new ModelFile
            {
                Id = Guid.NewGuid(),
                Name = "test.stl",
                Path = "/uploads/test.stl",
                Size = 1024,
                MimeType = "application/octet-stream"
            };
            model.Files.Add(modelFile);
            await DbContext.SaveChangesAsync();

            var client = Factory.CreateClient();
            var token = await GetAuthToken(client, user.Email, "TestPassword123!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await client.DeleteAsync($"/api/models/{model.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify model is soft deleted
            var deletedModel = await DbContext.Models
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == model.Id);

            Assert.NotNull(deletedModel);
            Assert.NotNull(deletedModel.DeletedAt);
            Assert.Equal(user.Id, deletedModel.DeletedById);

            // Verify files are also soft deleted
            var deletedFile = await DbContext.ModelFiles
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(f => f.Model.Id == model.Id);

            Assert.NotNull(deletedFile);
            Assert.NotNull(deletedFile.DeletedAt);
        }

        [Fact]
        public async Task DeleteModel_WithAdminPermission_ShouldAllowDeletion()
        {
            // Arrange
            await ResetStateAsync();
            var admin = await UserFactory.CreateTestUser("admin@test.com");
            var regularUser = await UserFactory.CreateTestUser("user@test.com");
            var model = await ModelFactory.CreateTestModel(regularUser.Id);
            await DbContext.SaveChangesAsync();

            // Make admin user an admin
            var adminRole = await DbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole != null)
            {
                admin.RoleId = adminRole.Id;
                await DbContext.SaveChangesAsync();
            }

            var client = Factory.CreateClient();
            var token = await GetAuthToken(client, admin.Email, "TestPassword123!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await client.DeleteAsync($"/api/models/{model.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify model is soft deleted
            var deletedModel = await DbContext.Models
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == model.Id);

            Assert.NotNull(deletedModel);
            Assert.NotNull(deletedModel.DeletedAt);
            Assert.Equal(admin.Id, deletedModel.DeletedById);
        }

        private async Task<string> GetAuthToken(HttpClient client, string email, string password)
        {
            var loginRequest = new
            {
                Email = email,
                Password = password
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(loginRequest),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("/api/authentication/login", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent);
                return loginResponse?.AccessToken ?? string.Empty;
            }

            throw new InvalidOperationException("Failed to get auth token");
        }

        private class LoginResponse
        {
            public string AccessToken { get; set; } = string.Empty;
        }
    }
} 