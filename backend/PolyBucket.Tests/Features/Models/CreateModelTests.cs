using System;
using System.IO;
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
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.Models.Domain.Enums;
using PolyBucket.Tests.Factories;
using Xunit;

namespace PolyBucket.Tests.Features.Models
{
    public class CreateModelTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly PolyBucketDbContext _context;
        private readonly TestUserFactory _userFactory;
        private readonly TestModelFactory _modelFactory;

        public CreateModelTests(WebApplicationFactory<Program> factory)
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
        public async Task CreateModel_WithValidData_ShouldCreateModel()
        {
            // Arrange
            var user = await _userFactory.CreateTestUser();
            var client = _factory.CreateClient();
            
            // Get auth token
            var token = await GetAuthToken(client, user.Email, "TestPassword123!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Create test file
            var testFileContent = "Test STL file content";
            var testFileBytes = Encoding.UTF8.GetBytes(testFileContent);

            using var formData = new MultipartFormDataContent();
            formData.Add(new StringContent("Test Model"), "name");
            formData.Add(new StringContent("Test Description"), "description");
            formData.Add(new StringContent("public"), "privacy");
            formData.Add(new StringContent("MIT"), "license");
            formData.Add(new StringContent("false"), "aiGenerated");
            formData.Add(new StringContent("false"), "workInProgress");
            formData.Add(new StringContent("false"), "nsfw");
            formData.Add(new StringContent("false"), "remix");
            
            var fileContent = new ByteArrayContent(testFileBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            formData.Add(fileContent, "files", "test.stl");

            // Act
            var response = await client.PostAsync("/api/models", formData);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<Model>(responseContent);
            
            Assert.NotNull(model);
            Assert.Equal("Test Model", model.Name);
            Assert.Equal("Test Description", model.Description);
            Assert.Equal(PrivacySettings.Public, model.Privacy);
            Assert.Equal(LicenseTypes.MIT, model.License);
            Assert.Equal(user.Id, model.AuthorId);
            Assert.False(model.IsDeleted);
        }

        [Fact]
        public async Task CreateModel_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            
            using var formData = new MultipartFormDataContent();
            formData.Add(new StringContent("Test Model"), "name");
            
            var testFileBytes = Encoding.UTF8.GetBytes("Test content");
            var fileContent = new ByteArrayContent(testFileBytes);
            formData.Add(fileContent, "files", "test.stl");

            // Act
            var response = await client.PostAsync("/api/models", formData);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateModel_WithoutName_ShouldReturnBadRequest()
        {
            // Arrange
            var user = await _userFactory.CreateTestUser();
            var client = _factory.CreateClient();
            
            var token = await GetAuthToken(client, user.Email, "TestPassword123!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(""), "name");
            
            var testFileBytes = Encoding.UTF8.GetBytes("Test content");
            var fileContent = new ByteArrayContent(testFileBytes);
            formData.Add(fileContent, "files", "test.stl");

            // Act
            var response = await client.PostAsync("/api/models", formData);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateModel_WithoutFiles_ShouldReturnBadRequest()
        {
            // Arrange
            var user = await _userFactory.CreateTestUser();
            var client = _factory.CreateClient();
            
            var token = await GetAuthToken(client, user.Email, "TestPassword123!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var formData = new MultipartFormDataContent();
            formData.Add(new StringContent("Test Model"), "name");

            // Act
            var response = await client.PostAsync("/api/models", formData);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateModel_WithUnsupportedFileType_ShouldReturnBadRequest()
        {
            // Arrange
            var user = await _userFactory.CreateTestUser();
            var client = _factory.CreateClient();
            
            var token = await GetAuthToken(client, user.Email, "TestPassword123!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var formData = new MultipartFormDataContent();
            formData.Add(new StringContent("Test Model"), "name");
            
            var testFileBytes = Encoding.UTF8.GetBytes("Test content");
            var fileContent = new ByteArrayContent(testFileBytes);
            formData.Add(fileContent, "files", "test.txt");

            // Act
            var response = await client.PostAsync("/api/models", formData);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateModel_WithMultipleFiles_ShouldCreateModelWithAllFiles()
        {
            // Arrange
            var user = await _userFactory.CreateTestUser();
            var client = _factory.CreateClient();
            
            var token = await GetAuthToken(client, user.Email, "TestPassword123!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var formData = new MultipartFormDataContent();
            formData.Add(new StringContent("Test Model"), "name");
            formData.Add(new StringContent("Test Description"), "description");
            
            var stlFileBytes = Encoding.UTF8.GetBytes("STL content");
            var stlFileContent = new ByteArrayContent(stlFileBytes);
            formData.Add(stlFileContent, "files", "model.stl");
            
            var imageFileBytes = Encoding.UTF8.GetBytes("Image content");
            var imageFileContent = new ByteArrayContent(imageFileBytes);
            formData.Add(imageFileContent, "files", "preview.jpg");

            // Act
            var response = await client.PostAsync("/api/models", formData);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<Model>(responseContent);
            
            Assert.NotNull(model);
            Assert.Equal("Test Model", model.Name);
            Assert.Equal(2, model.Files.Count);
        }

        private async Task<string> GetAuthToken(HttpClient client, string email, string password)
        {
            var loginData = new
            {
                Email = email,
                Password = password
            };

            var json = JsonConvert.SerializeObject(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/authentication/login", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);

            return loginResponse?.token?.ToString() ?? string.Empty;
        }
    }
} 