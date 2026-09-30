using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Moq;
using PolyBucket.Marketplace.Api.Services;
using Shouldly;
using Xunit;
using System.IO;
using System.Text;

namespace PolyBucket.Marketplace.Tests.Services
{
    public class FileServiceTests : IDisposable
    {
        private readonly Mock<ILogger<FileService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly FileService _fileService;
        private readonly string _testUploadPath;

        public FileServiceTests()
        {
            _mockLogger = new Mock<ILogger<FileService>>();
            _testUploadPath = Path.Combine(Path.GetTempPath(), "test-uploads", Guid.NewGuid().ToString());
            
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(c => c["FileStorage:UploadPath"])
                .Returns(_testUploadPath);

            _fileService = new FileService(_mockLogger.Object, _mockConfiguration.Object);
        }

        private static Mock<IFormFile> CreateFormFile(string fileName, byte[] fileBytes)
        {
            var formFile = new Mock<IFormFile>();
            formFile.Setup(f => f.FileName).Returns(fileName);
            formFile.Setup(f => f.Length).Returns(fileBytes.Length);
            formFile.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(fileBytes));
            formFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns<Stream, CancellationToken>((target, _) =>
                {
                    target.Write(fileBytes, 0, fileBytes.Length);
                    return Task.CompletedTask;
                });
            return formFile;
        }

        [Fact]
        public async Task SavePluginFileAsync_WithValidFile_ReturnsFilePath()
        {
            var pluginId = "test-plugin-1";
            var fileName = "test-file.txt";
            var fileContent = "Test file content";
            var fileBytes = Encoding.UTF8.GetBytes(fileContent);
            var formFile = CreateFormFile(fileName, fileBytes);

            var result = await _fileService.SavePluginFileAsync(formFile.Object, pluginId);

            result.ShouldNotBeNullOrEmpty();
            result.ShouldContain(pluginId);
            result.ShouldContain(fileName);
            
            File.Exists(result).ShouldBeTrue();
            var savedContent = await File.ReadAllTextAsync(result);
            savedContent.ShouldBe(fileContent);
        }

        [Fact]
        public async Task SavePluginFileAsync_WithNullFile_ThrowsArgumentException()
        {
            var pluginId = "test-plugin-1";

            await Should.ThrowAsync<ArgumentException>(async () =>
                await _fileService.SavePluginFileAsync(null!, pluginId));
        }

        [Fact]
        public async Task SavePluginFileAsync_WithEmptyFile_ThrowsArgumentException()
        {
            var pluginId = "test-plugin-1";
            var formFile = new Mock<IFormFile>();
            formFile.Setup(f => f.Length).Returns(0);

            await Should.ThrowAsync<ArgumentException>(async () =>
                await _fileService.SavePluginFileAsync(formFile.Object, pluginId));
        }

        [Fact]
        public async Task SavePluginFileAsync_WithEmptyPluginId_CreatesFile()
        {
            var pluginId = "";
            var fileName = "test-file.txt";
            var fileContent = "Test file content";
            var fileBytes = Encoding.UTF8.GetBytes(fileContent);
            var formFile = CreateFormFile(fileName, fileBytes);

            var result = await _fileService.SavePluginFileAsync(formFile.Object, pluginId);

            result.ShouldNotBeNullOrEmpty();
            File.Exists(result).ShouldBeTrue();
        }

        [Fact]
        public async Task SavePluginFileAsync_WithSpecialCharactersInFileName_HandlesCorrectly()
        {
            var pluginId = "test-plugin-1";
            var fileName = "test-file with spaces & special chars!.txt";
            var fileContent = "Test file content";
            var fileBytes = Encoding.UTF8.GetBytes(fileContent);
            var formFile = CreateFormFile(fileName, fileBytes);

            var result = await _fileService.SavePluginFileAsync(formFile.Object, pluginId);

            result.ShouldNotBeNullOrEmpty();
            File.Exists(result).ShouldBeTrue();
        }

        [Fact]
        public async Task SavePluginFileAsync_WithLongFileName_HandlesCorrectly()
        {
            var pluginId = "test-plugin-1";
            var fileName = new string('A', 300) + ".txt";
            var fileContent = "Test file content";
            var fileBytes = Encoding.UTF8.GetBytes(fileContent);
            var formFile = CreateFormFile(fileName, fileBytes);

            var result = await _fileService.SavePluginFileAsync(formFile.Object, pluginId);

            result.ShouldNotBeNullOrEmpty();
            File.Exists(result).ShouldBeTrue();
        }

        [Fact]
        public async Task SavePluginFileAsync_CreatesPluginSpecificDirectory()
        {
            var pluginId = "test-plugin-1";
            var fileName = "test-file.txt";
            var fileContent = "Test file content";
            var fileBytes = Encoding.UTF8.GetBytes(fileContent);
            var formFile = CreateFormFile(fileName, fileBytes);

            var result = await _fileService.SavePluginFileAsync(formFile.Object, pluginId);

            var pluginDir = Path.Combine(_testUploadPath, pluginId);
            Directory.Exists(pluginDir).ShouldBeTrue();
            result.ShouldStartWith(pluginDir);
        }

        [Fact]
        public async Task DeletePluginFileAsync_WithExistingFile_ReturnsTrue()
        {
            var filePath = Path.Combine(_testUploadPath, "test-file.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            await File.WriteAllTextAsync(filePath, "Test content");

            var result = await _fileService.DeletePluginFileAsync(filePath);

            result.ShouldBeTrue();
            File.Exists(filePath).ShouldBeFalse();
        }

        [Fact]
        public async Task DeletePluginFileAsync_WithNonExistentFile_ReturnsFalse()
        {
            var filePath = Path.Combine(_testUploadPath, "non-existent-file.txt");

            var result = await _fileService.DeletePluginFileAsync(filePath);

            result.ShouldBeFalse();
        }

        [Fact]
        public async Task DeletePluginFileAsync_WithNullFilePath_ReturnsFalse()
        {
            var result = await _fileService.DeletePluginFileAsync(null!);

            result.ShouldBeFalse();
        }

        [Fact]
        public async Task DeletePluginFileAsync_WithEmptyFilePath_ReturnsFalse()
        {
            var result = await _fileService.DeletePluginFileAsync("");

            result.ShouldBeFalse();
        }

        [Fact]
        public async Task GetPluginFileAsync_WithExistingFile_ReturnsFileBytes()
        {
            var filePath = Path.Combine(_testUploadPath, "test-file.txt");
            var fileContent = "Test file content";
            var expectedBytes = Encoding.UTF8.GetBytes(fileContent);
            
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            await File.WriteAllBytesAsync(filePath, expectedBytes);

            var result = await _fileService.GetPluginFileAsync(filePath);

            result.ShouldNotBeNull();
            result.ShouldBe(expectedBytes);
        }

        [Fact]
        public async Task GetPluginFileAsync_WithNonExistentFile_ReturnsNull()
        {
            var filePath = Path.Combine(_testUploadPath, "non-existent-file.txt");

            var result = await _fileService.GetPluginFileAsync(filePath);

            result.ShouldBeNull();
        }

        [Fact]
        public async Task GetPluginFileAsync_WithNullFilePath_ReturnsNull()
        {
            var result = await _fileService.GetPluginFileAsync(null!);

            result.ShouldBeNull();
        }

        [Fact]
        public async Task GetPluginFileAsync_WithEmptyFilePath_ReturnsNull()
        {
            var result = await _fileService.GetPluginFileAsync("");

            result.ShouldBeNull();
        }

        [Fact]
        public async Task GetPluginFileUrlAsync_WithValidFilePath_ReturnsUrl()
        {
            var filePath = Path.Combine(_testUploadPath, "plugin-1", "test-file.txt");

            var result = await _fileService.GetPluginFileUrlAsync(filePath);

            result.ShouldNotBeNullOrEmpty();
            result.ShouldStartWith("/api/files/");
            result.ShouldContain("plugin-1");
            result.ShouldContain("test-file.txt");
        }

        [Fact]
        public async Task GetPluginFileUrlAsync_WithNestedFilePath_ReturnsCorrectUrl()
        {
            var filePath = Path.Combine(_testUploadPath, "plugin-1", "subfolder", "test-file.txt");

            var result = await _fileService.GetPluginFileUrlAsync(filePath);

            result.ShouldNotBeNullOrEmpty();
            result.ShouldStartWith("/api/files/");
            result.ShouldContain("plugin-1");
            result.ShouldContain("subfolder");
            result.ShouldContain("test-file.txt");
        }

        [Fact]
        public async Task GetPluginFileUrlAsync_WithNullFilePath_ReturnsEmptyString()
        {
            var result = await _fileService.GetPluginFileUrlAsync(null!);

            result.ShouldBeEmpty();
        }

        [Fact]
        public async Task GetPluginFileUrlAsync_WithEmptyFilePath_ReturnsEmptyString()
        {
            var result = await _fileService.GetPluginFileUrlAsync("");

            result.ShouldBeEmpty();
        }

        [Fact]
        public void Constructor_WithDefaultConfiguration_CreatesUploadDirectory()
        {
            var uploadPath = Path.Combine(Path.GetTempPath(), "test-uploads", Guid.NewGuid().ToString());
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["FileStorage:UploadPath"]).Returns(uploadPath);

            var fileService = new FileService(_mockLogger.Object, mockConfig.Object);

            Directory.Exists(uploadPath).ShouldBeTrue();
        }

        [Fact]
        public void Constructor_WithNullConfiguration_UsesDefaultPath()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["FileStorage:UploadPath"]).Returns((string?)null);

            var fileService = new FileService(_mockLogger.Object, mockConfig.Object);

            fileService.ShouldNotBeNull();
        }

        public void Dispose()
        {
            if (Directory.Exists(_testUploadPath))
            {
                Directory.Delete(_testUploadPath, true);
            }
        }
    }
}
