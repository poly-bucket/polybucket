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

        [Fact]
        public async Task SavePluginFileAsync_WithValidFile_ReturnsFilePath()
        {
            // Arrange
            var pluginId = "test-plugin-1";
            var fileName = "test-file.txt";
            var fileContent = "Test file content";
            var fileBytes = Encoding.UTF8.GetBytes(fileContent);
            
            using var stream = new MemoryStream(fileBytes);
            var formFile = new Mock<IFormFile>();
            formFile.Setup(f => f.FileName).Returns(fileName);
            formFile.Setup(f => f.Length).Returns(fileBytes.Length);
            formFile.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(fileBytes));

            // Act
            var result = await _fileService.SavePluginFileAsync(formFile.Object, pluginId);

            // Assert
            result.ShouldNotBeNullOrEmpty();
            result.ShouldContain(pluginId);
            result.ShouldContain(fileName);
            
            // Verify file was actually created
            File.Exists(result).ShouldBeTrue();
            var savedContent = await File.ReadAllTextAsync(result);
            savedContent.ShouldBe(fileContent);
        }

        [Fact]
        public async Task SavePluginFileAsync_WithNullFile_ThrowsArgumentException()
        {
            // Arrange
            var pluginId = "test-plugin-1";

            // Act & Assert
            await Should.ThrowAsync<ArgumentException>(async () =>
                await _fileService.SavePluginFileAsync(null!, pluginId));
        }

        [Fact]
        public async Task SavePluginFileAsync_WithEmptyFile_ThrowsArgumentException()
        {
            // Arrange
            var pluginId = "test-plugin-1";
            var formFile = new Mock<IFormFile>();
            formFile.Setup(f => f.Length).Returns(0);

            // Act & Assert
            await Should.ThrowAsync<ArgumentException>(async () =>
                await _fileService.SavePluginFileAsync(formFile.Object, pluginId));
        }

        [Fact]
        public async Task SavePluginFileAsync_WithEmptyPluginId_CreatesFile()
        {
            // Arrange
            var pluginId = "";
            var fileName = "test-file.txt";
            var fileContent = "Test file content";
            var fileBytes = Encoding.UTF8.GetBytes(fileContent);
            
            using var stream = new MemoryStream(fileBytes);
            var formFile = new Mock<IFormFile>();
            formFile.Setup(f => f.FileName).Returns(fileName);
            formFile.Setup(f => f.Length).Returns(fileBytes.Length);
            formFile.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(fileBytes));

            // Act
            var result = await _fileService.SavePluginFileAsync(formFile.Object, pluginId);

            // Assert
            result.ShouldNotBeNullOrEmpty();
            File.Exists(result).ShouldBeTrue();
        }

        [Fact]
        public async Task SavePluginFileAsync_WithSpecialCharactersInFileName_HandlesCorrectly()
        {
            // Arrange
            var pluginId = "test-plugin-1";
            var fileName = "test-file with spaces & special chars!.txt";
            var fileContent = "Test file content";
            var fileBytes = Encoding.UTF8.GetBytes(fileContent);
            
            using var stream = new MemoryStream(fileBytes);
            var formFile = new Mock<IFormFile>();
            formFile.Setup(f => f.FileName).Returns(fileName);
            formFile.Setup(f => f.Length).Returns(fileBytes.Length);
            formFile.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(fileBytes));

            // Act
            var result = await _fileService.SavePluginFileAsync(formFile.Object, pluginId);

            // Assert
            result.ShouldNotBeNullOrEmpty();
            File.Exists(result).ShouldBeTrue();
        }

        [Fact]
        public async Task SavePluginFileAsync_WithLongFileName_HandlesCorrectly()
        {
            // Arrange
            var pluginId = "test-plugin-1";
            var fileName = new string('A', 300) + ".txt"; // Very long filename
            var fileContent = "Test file content";
            var fileBytes = Encoding.UTF8.GetBytes(fileContent);
            
            using var stream = new MemoryStream(fileBytes);
            var formFile = new Mock<IFormFile>();
            formFile.Setup(f => f.FileName).Returns(fileName);
            formFile.Setup(f => f.Length).Returns(fileBytes.Length);
            formFile.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(fileBytes));

            // Act
            var result = await _fileService.SavePluginFileAsync(formFile.Object, pluginId);

            // Assert
            result.ShouldNotBeNullOrEmpty();
            File.Exists(result).ShouldBeTrue();
        }

        [Fact]
        public async Task SavePluginFileAsync_CreatesPluginSpecificDirectory()
        {
            // Arrange
            var pluginId = "test-plugin-1";
            var fileName = "test-file.txt";
            var fileContent = "Test file content";
            var fileBytes = Encoding.UTF8.GetBytes(fileContent);
            
            using var stream = new MemoryStream(fileBytes);
            var formFile = new Mock<IFormFile>();
            formFile.Setup(f => f.FileName).Returns(fileName);
            formFile.Setup(f => f.Length).Returns(fileBytes.Length);
            formFile.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(fileBytes));

            // Act
            var result = await _fileService.SavePluginFileAsync(formFile.Object, pluginId);

            // Assert
            var pluginDir = Path.Combine(_testUploadPath, pluginId);
            Directory.Exists(pluginDir).ShouldBeTrue();
            result.ShouldStartWith(pluginDir);
        }

        [Fact]
        public async Task DeletePluginFileAsync_WithExistingFile_ReturnsTrue()
        {
            // Arrange
            var filePath = Path.Combine(_testUploadPath, "test-file.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            await File.WriteAllTextAsync(filePath, "Test content");

            // Act
            var result = await _fileService.DeletePluginFileAsync(filePath);

            // Assert
            result.ShouldBeTrue();
            File.Exists(filePath).ShouldBeFalse();
        }

        [Fact]
        public async Task DeletePluginFileAsync_WithNonExistentFile_ReturnsFalse()
        {
            // Arrange
            var filePath = Path.Combine(_testUploadPath, "non-existent-file.txt");

            // Act
            var result = await _fileService.DeletePluginFileAsync(filePath);

            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task DeletePluginFileAsync_WithNullFilePath_ReturnsFalse()
        {
            // Act
            var result = await _fileService.DeletePluginFileAsync(null!);

            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task DeletePluginFileAsync_WithEmptyFilePath_ReturnsFalse()
        {
            // Act
            var result = await _fileService.DeletePluginFileAsync("");

            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task GetPluginFileAsync_WithExistingFile_ReturnsFileBytes()
        {
            // Arrange
            var filePath = Path.Combine(_testUploadPath, "test-file.txt");
            var fileContent = "Test file content";
            var expectedBytes = Encoding.UTF8.GetBytes(fileContent);
            
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            await File.WriteAllBytesAsync(filePath, expectedBytes);

            // Act
            var result = await _fileService.GetPluginFileAsync(filePath);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe(expectedBytes);
        }

        [Fact]
        public async Task GetPluginFileAsync_WithNonExistentFile_ReturnsNull()
        {
            // Arrange
            var filePath = Path.Combine(_testUploadPath, "non-existent-file.txt");

            // Act
            var result = await _fileService.GetPluginFileAsync(filePath);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task GetPluginFileAsync_WithNullFilePath_ReturnsNull()
        {
            // Act
            var result = await _fileService.GetPluginFileAsync(null!);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task GetPluginFileAsync_WithEmptyFilePath_ReturnsNull()
        {
            // Act
            var result = await _fileService.GetPluginFileAsync("");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task GetPluginFileUrlAsync_WithValidFilePath_ReturnsUrl()
        {
            // Arrange
            var filePath = Path.Combine(_testUploadPath, "plugin-1", "test-file.txt");

            // Act
            var result = await _fileService.GetPluginFileUrlAsync(filePath);

            // Assert
            result.ShouldNotBeNullOrEmpty();
            result.ShouldStartWith("/api/files/");
            result.ShouldContain("plugin-1");
            result.ShouldContain("test-file.txt");
        }

        [Fact]
        public async Task GetPluginFileUrlAsync_WithNestedFilePath_ReturnsCorrectUrl()
        {
            // Arrange
            var filePath = Path.Combine(_testUploadPath, "plugin-1", "subfolder", "test-file.txt");

            // Act
            var result = await _fileService.GetPluginFileUrlAsync(filePath);

            // Assert
            result.ShouldNotBeNullOrEmpty();
            result.ShouldStartWith("/api/files/");
            result.ShouldContain("plugin-1");
            result.ShouldContain("subfolder");
            result.ShouldContain("test-file.txt");
        }

        [Fact]
        public async Task GetPluginFileUrlAsync_WithNullFilePath_ReturnsEmptyString()
        {
            // Act
            var result = await _fileService.GetPluginFileUrlAsync(null!);

            // Assert
            result.ShouldBeEmpty();
        }

        [Fact]
        public async Task GetPluginFileUrlAsync_WithEmptyFilePath_ReturnsEmptyString()
        {
            // Act
            var result = await _fileService.GetPluginFileUrlAsync("");

            // Assert
            result.ShouldBeEmpty();
        }

        [Fact]
        public void Constructor_WithDefaultConfiguration_CreatesUploadDirectory()
        {
            // Arrange
            var uploadPath = Path.Combine(Path.GetTempPath(), "test-uploads", Guid.NewGuid().ToString());
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["FileStorage:UploadPath"]).Returns(uploadPath);

            // Act
            var fileService = new FileService(_mockLogger.Object, mockConfig.Object);

            // Assert
            Directory.Exists(uploadPath).ShouldBeTrue();
        }

        [Fact]
        public void Constructor_WithNullConfiguration_UsesDefaultPath()
        {
            // Arrange
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["FileStorage:UploadPath"]).Returns((string?)null);

            // Act
            var fileService = new FileService(_mockLogger.Object, mockConfig.Object);

            // Assert
            // Should not throw exception and should create default directory
            fileService.ShouldNotBeNull();
        }

        public void Dispose()
        {
            // Clean up test files and directories
            if (Directory.Exists(_testUploadPath))
            {
                Directory.Delete(_testUploadPath, true);
            }
        }
    }
}
