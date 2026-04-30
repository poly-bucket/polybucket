using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Models.CreateModel.Domain;
using PolyBucket.Api.Common.Models;
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

        [Fact(DisplayName = "When deleting a model as the owner, the delete model controller soft-deletes the model.")]
        public async Task DeleteModel_ByOwner_ShouldSoftDeleteModel()
        {
            // Arrange
            await ResetStateAsync();
            var user = await UserFactory.CreateTestUser();
            var model = await ModelFactory.CreateTestModel(user.Id);
            await DbContext.SaveChangesAsync();

            var client = Factory.CreateClient();
            var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await client.DeleteAsync($"/api/models/{model.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            DbContext.ChangeTracker.Clear();

            // Verify model is soft deleted
            var deletedModel = await DbContext.Models
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == model.Id);

            Assert.NotNull(deletedModel);
            Assert.NotNull(deletedModel.DeletedAt);
            Assert.Equal(user.Id, deletedModel.DeletedById);
        }

        [Fact(DisplayName = "When deleting a model without authentication, the delete model controller returns Unauthorized.")]
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

        [Fact(DisplayName = "When deleting a model as a non-owner, the delete model controller returns Forbidden.")]
        public async Task DeleteModel_ByNonOwner_ShouldReturnForbidden()
        {
            // Arrange
            await ResetStateAsync();
            var owner = await UserFactory.CreateTestUser("owner@test.com");
            var nonOwner = await UserFactory.CreateTestUser("nonowner@test.com");
            var model = await ModelFactory.CreateTestModel(owner.Id);
            await DbContext.SaveChangesAsync();

            var client = Factory.CreateClient();
            var token = await GetAuthTokenWithClient(client, nonOwner.Email, "TestPassword123!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await client.DeleteAsync($"/api/models/{model.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact(DisplayName = "When deleting a model that does not exist, the delete model controller returns NotFound.")]
        public async Task DeleteModel_NonExistentModel_ShouldReturnNotFound()
        {
            // Arrange
            await ResetStateAsync();
            var user = await UserFactory.CreateTestUser();
            var nonExistentId = Guid.NewGuid();

            var client = Factory.CreateClient();
            var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await client.DeleteAsync($"/api/models/{nonExistentId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "When deleting a model that has already been deleted, the delete model controller returns BadRequest.")]
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
            var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await client.DeleteAsync($"/api/models/{model.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "When deleting a model that has files, the delete model controller soft-deletes the model and its files.")]
        public async Task DeleteModel_WithFiles_ShouldSoftDeleteModelAndFiles()
        {
            // Arrange
            await ResetStateAsync();
            var user = await UserFactory.CreateTestUser();
            var model = await ModelFactory.CreateTestModel(user.Id);
            
            var modelFile = new ModelFile
            {
                Id = Guid.NewGuid(),
                ModelId = model.Id,
                Name = "test.stl",
                Path = "/uploads/test.stl",
                Size = 1024,
                MimeType = "application/octet-stream",
                CreatedAt = DateTime.UtcNow,
                CreatedById = user.Id,
                UpdatedAt = DateTime.UtcNow,
                UpdatedById = user.Id
            };
            await DbContext.ModelFiles.AddAsync(modelFile);
            await DbContext.SaveChangesAsync();

            var client = Factory.CreateClient();
            var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await client.DeleteAsync($"/api/models/{model.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            DbContext.ChangeTracker.Clear();

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

        [Fact(DisplayName = "When deleting a model as a user with admin permission, the delete model controller allows the deletion.")]
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
            var token = await GetAuthTokenWithClient(client, admin.Email, "TestPassword123!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await client.DeleteAsync($"/api/models/{model.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            DbContext.ChangeTracker.Clear();

            // Verify model is soft deleted
            var deletedModel = await DbContext.Models
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == model.Id);

            Assert.NotNull(deletedModel);
            Assert.NotNull(deletedModel.DeletedAt);
            Assert.Equal(admin.Id, deletedModel.DeletedById);
        }

    }
} 