using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Categories.CreateCategory.Domain;
using PolyBucket.Api.Features.Categories.DeleteCategory.Domain;
using PolyBucket.Api.Features.Categories.UpdateCategory.Domain;
using PolyBucket.Api.Features.Categories.GetCategories.Domain;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Tests;
using System;
using System.Threading.Tasks;
using Xunit;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace PolyBucket.Tests.Features.Categories
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
                Name = "Updated Name"
            };

            // Act
            var response = await Client.PutAsJsonAsync($"/api/admin/categories/{createdCategory.Id}", updateCommand);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<UpdateCategoryResponse>();
            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.Name);
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
