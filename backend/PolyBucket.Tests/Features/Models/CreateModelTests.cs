using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Features.Models.CreateModel.Domain;
using Xunit;

namespace PolyBucket.Tests.Features.Models;

[Collection("TestCollection")]
public class CreateModelTests : BaseIntegrationTest
{
    private static readonly JsonSerializerOptions CreateModelJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public CreateModelTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When creating a model with valid data, the create model controller creates the model.")]
    public async Task CreateModel_WithValidData_ShouldCreateModel()
    {
        await ResetStateAsync();
        var user = await UserFactory.CreateTestUser();
        var client = Factory.CreateClient();

        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var testFileBytes = Encoding.UTF8.GetBytes("solid polybucket-test\nendsolid polybucket-test\n");

        using var formData = new MultipartFormDataContent();
        formData.Add(new StringContent("Test Model"), "name");
        formData.Add(new StringContent("Test Description"), "description");
        formData.Add(new StringContent("public"), "privacy");
        formData.Add(new StringContent("MIT"), "license");
        formData.Add(new StringContent("false"), "aiGenerated");
        formData.Add(new StringContent("false"), "workInProgress");
        formData.Add(new StringContent("false"), "nSFW");
        formData.Add(new StringContent("false"), "remix");

        var fileContent = new ByteArrayContent(testFileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        formData.Add(fileContent, "files", "test.stl");

        var response = await client.PostAsync("/api/models", formData);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<CreateModelResponse>(responseContent, CreateModelJsonOptions);
        var model = envelope?.Model;

        Assert.NotNull(model);
        Assert.Equal("Test Model", model!.Name);
        Assert.Equal("Test Description", model.Description);
        Assert.Equal(PrivacySettings.Public, model.Privacy);
        Assert.Equal(LicenseTypes.MIT, model.License);
        Assert.Equal(user.Id, model.AuthorId);
        Assert.False(model.IsDeleted);
    }

    [Fact(DisplayName = "When creating a model without authentication, the create model controller returns Unauthorized.")]
    public async Task CreateModel_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        await ResetStateAsync();
        var client = Factory.CreateClient();

        using var formData = new MultipartFormDataContent();
        formData.Add(new StringContent("Test Model"), "name");

        var testFileBytes = Encoding.UTF8.GetBytes("Test content");
        var fileContent = new ByteArrayContent(testFileBytes);
        formData.Add(fileContent, "files", "test.stl");

        var response = await client.PostAsync("/api/models", formData);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "When creating a model without a name, the create model controller returns BadRequest.")]
    public async Task CreateModel_WithoutName_ShouldReturnBadRequest()
    {
        await ResetStateAsync();
        var user = await UserFactory.CreateTestUser();
        var client = Factory.CreateClient();

        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var formData = new MultipartFormDataContent();
        formData.Add(new StringContent(""), "name");

        var testFileBytes = Encoding.UTF8.GetBytes("solid polybucket-test\nendsolid polybucket-test\n");
        var fileContent = new ByteArrayContent(testFileBytes);
        formData.Add(fileContent, "files", "test.stl");

        var response = await client.PostAsync("/api/models", formData);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "When creating a model without any files, the create model controller returns BadRequest.")]
    public async Task CreateModel_WithoutFiles_ShouldReturnBadRequest()
    {
        await ResetStateAsync();
        var user = await UserFactory.CreateTestUser();
        var client = Factory.CreateClient();

        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var formData = new MultipartFormDataContent();
        formData.Add(new StringContent("Test Model"), "name");

        var response = await client.PostAsync("/api/models", formData);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "When creating a model with an unsupported file type, the create model controller returns BadRequest.")]
    public async Task CreateModel_WithUnsupportedFileType_ShouldReturnBadRequest()
    {
        await ResetStateAsync();
        var user = await UserFactory.CreateTestUser();
        var client = Factory.CreateClient();

        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var formData = new MultipartFormDataContent();
        formData.Add(new StringContent("Test Model"), "name");

        var testFileBytes = Encoding.UTF8.GetBytes("plain text");
        var fileContent = new ByteArrayContent(testFileBytes);
        formData.Add(fileContent, "files", "test.txt");

        var response = await client.PostAsync("/api/models", formData);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "When creating a model with multiple files, the create model controller creates a model that includes all uploaded files.")]
    public async Task CreateModel_WithMultipleFiles_ShouldCreateModelWithAllFiles()
    {
        await ResetStateAsync();
        var user = await UserFactory.CreateTestUser();
        var client = Factory.CreateClient();

        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var formData = new MultipartFormDataContent();
        formData.Add(new StringContent("Test Model"), "name");
        formData.Add(new StringContent("Test Description"), "description");
        formData.Add(new StringContent("public"), "privacy");
        formData.Add(new StringContent("MIT"), "license");
        formData.Add(new StringContent("false"), "aiGenerated");
        formData.Add(new StringContent("false"), "workInProgress");
        formData.Add(new StringContent("false"), "nSFW");
        formData.Add(new StringContent("false"), "remix");

        var stlFileBytes = Encoding.UTF8.GetBytes("solid polybucket-test\nendsolid polybucket-test\n");
        var stlFileContent = new ByteArrayContent(stlFileBytes);
        formData.Add(stlFileContent, "files", "model.stl");

        var imageFileBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00 };
        var imageFileContent = new ByteArrayContent(imageFileBytes);
        formData.Add(imageFileContent, "files", "preview.jpg");

        var response = await client.PostAsync("/api/models", formData);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<CreateModelResponse>(responseContent, CreateModelJsonOptions);
        var model = envelope?.Model;

        Assert.NotNull(model);
        Assert.Equal("Test Model", model!.Name);
        Assert.Equal(2, model.Files.Count);
    }
}
