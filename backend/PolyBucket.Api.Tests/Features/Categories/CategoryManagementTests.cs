using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Categories.CreateCategory.Domain;
using PolyBucket.Api.Features.Categories.DeleteCategory.Domain;
using PolyBucket.Api.Features.Categories.DeleteCategories.Domain;
using PolyBucket.Api.Features.Categories.UpdateCategory.Domain;
using PolyBucket.Api.Features.Categories.GetCategories.Domain;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Tests;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace PolyBucket.Api.Tests.Features.Categories
{
    [Collection("TestCollection")]
    public class CategoryManagementTests : BaseIntegrationTest
    {
        public CategoryManagementTests(TestCollectionFixture testFixture) : base(testFixture)
        {
        }

        [Fact]
        public async Task CreateCategory_WithValidName_ShouldCreateCategory()
        {
            // Arrange
            await ResetStateAsync();
            var adminUser = await CreateTestUser("admin@test.com", "AdminPassword123!");
            await AssignAdminRole(adminUser);
            var token = await GetAuthToken("admin@test.com", "AdminPassword123!");
            SetAuthHeaders(token);

            var command = new CreateCategoryCommand
            {
                Name = "Test Category"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/admin/categories", command);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<CreateCategoryResponse>();
            Assert.NotNull(result);
            Assert.Equal("Test Category", result.Name);
            Assert.NotEqual(Guid.Empty, result.Id);
        }

        [Fact]
        public async Task GetCategories_AsAdmin_ShouldReturnCategories()
        {
            // Arrange
            await ResetStateAsync();
            var adminUser = await CreateTestUser("admin@test.com", "AdminPassword123!");
            await AssignAdminRole(adminUser);
            var token = await GetAuthToken("admin@test.com", "AdminPassword123!");
            SetAuthHeaders(token);

            // Act
            var response = await Client.GetAsync("/api/admin/categories");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<GetCategoriesResponse>();
            Assert.NotNull(result);
            Assert.NotNull(result.Categories);
        }

        [Fact]
        public async Task UpdateCategory_WithValidData_ShouldUpdateCategory()
        {
            // Arrange
            await ResetStateAsync();
            var adminUser = await CreateTestUser("admin@test.com", "AdminPassword123!");
            await AssignAdminRole(adminUser);
            var token = await GetAuthToken("admin@test.com", "AdminPassword123!");
            SetAuthHeaders(token);

            // First create a category
            var createCommand = new CreateCategoryCommand { Name = "Original Name" };
            var createResponse = await Client.PostAsJsonAsync("/api/admin/categories", createCommand);
            createResponse.EnsureSuccessStatusCode();
            var createdCategory = await createResponse.Content.ReadFromJsonAsync<CreateCategoryResponse>();

            var updateCommand = new UpdateCategoryCommand
            {
                Id = createdCategory.Id,
                Name = "Updated Name",
                Description = "A detailed category description",
                Color = "#3B82F6"
            };

            // Act
            var response = await Client.PutAsJsonAsync($"/api/admin/categories/{createdCategory.Id}", updateCommand);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<UpdateCategoryResponse>();
            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.Name);
            Assert.Equal("A detailed category description", result.Description);
            Assert.Equal("#3B82F6", result.Color);

            var listResponse = await Client.GetAsync("/api/admin/categories?page=1&pageSize=100");
            listResponse.EnsureSuccessStatusCode();
            var list = await listResponse.Content.ReadFromJsonAsync<GetCategoriesResponse>();
            var listed = list!.Categories!.Find(c => c.Id == createdCategory.Id.ToString());
            Assert.NotNull(listed);
            Assert.Equal("A detailed category description", listed.Description);
            Assert.Equal("#3B82F6", listed.Color);
            Assert.Equal(adminUser.Username, listed.CreatedByUsername);
            Assert.Equal(adminUser.Email, listed.CreatedByEmail);
        }

        [Fact]
        public async Task DeleteCategory_WithValidId_ShouldDeleteCategory()
        {
            // Arrange
            await ResetStateAsync();
            var adminUser = await CreateTestUser("admin@test.com", "AdminPassword123!");
            await AssignAdminRole(adminUser);
            var token = await GetAuthToken("admin@test.com", "AdminPassword123!");
            SetAuthHeaders(token);

            // First create a category
            var createCommand = new CreateCategoryCommand { Name = "Category to Delete" };
            var createResponse = await Client.PostAsJsonAsync("/api/admin/categories", createCommand);
            createResponse.EnsureSuccessStatusCode();
            var createdCategory = await createResponse.Content.ReadFromJsonAsync<CreateCategoryResponse>();

            // Act
            var response = await Client.DeleteAsync($"/api/admin/categories/{createdCategory.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<DeleteCategoryResponse>();
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Category to Delete", result.Name);
        }

        [Fact]
        public async Task DeleteCategories_WithValidIds_ShouldDeleteCategories()
        {
            // Arrange
            await ResetStateAsync();
            var adminUser = await CreateTestUser("admin@test.com", "AdminPassword123!");
            await AssignAdminRole(adminUser);
            var token = await GetAuthToken("admin@test.com", "AdminPassword123!");
            SetAuthHeaders(token);

            var first = await Client.PostAsJsonAsync("/api/admin/categories", new CreateCategoryCommand { Name = "Bulk One" });
            first.EnsureSuccessStatusCode();
            var firstCategory = await first.Content.ReadFromJsonAsync<CreateCategoryResponse>();

            var second = await Client.PostAsJsonAsync("/api/admin/categories", new CreateCategoryCommand { Name = "Bulk Two" });
            second.EnsureSuccessStatusCode();
            var secondCategory = await second.Content.ReadFromJsonAsync<CreateCategoryResponse>();

            var bulkCommand = new DeleteCategoriesCommand
            {
                Ids = new List<Guid> { firstCategory!.Id, secondCategory!.Id }
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/admin/categories/bulk-delete", bulkCommand);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<DeleteCategoriesResponse>();
            Assert.NotNull(result);
            Assert.Equal(2, result.DeletedCount);
            Assert.Equal(0, result.FailedCount);
        }

        [Fact]
        public async Task GetCategories_WithPagination_ShouldReturnPagedResults()
        {
            // Arrange
            await ResetStateAsync();
            var adminUser = await CreateTestUser("admin@test.com", "AdminPassword123!");
            await AssignAdminRole(adminUser);
            var token = await GetAuthToken("admin@test.com", "AdminPassword123!");
            SetAuthHeaders(token);

            for (var i = 0; i < 3; i++)
            {
                var createResponse = await Client.PostAsJsonAsync(
                    "/api/admin/categories",
                    new CreateCategoryCommand { Name = $"Paged Category {i}" });
                createResponse.EnsureSuccessStatusCode();
            }

            // Act
            var response = await Client.GetAsync("/api/admin/categories?page=1&pageSize=2");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<GetCategoriesResponse>();
            Assert.NotNull(result);
            Assert.Equal(2, result.Categories.Count);
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(1, result.Page);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(2, result.TotalPages);
        }

        private async Task AssignAdminRole(User user)
        {
            var adminRole = await DbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null)
            {
                adminRole = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin",
                    Description = "Administrator role",
                    Priority = 1000,
                    IsSystemRole = true,
                    IsActive = true
                };
                DbContext.Roles.Add(adminRole);
                await DbContext.SaveChangesAsync();
            }

            user.RoleId = adminRole.Id;
            await DbContext.SaveChangesAsync();
        }
    }
}
