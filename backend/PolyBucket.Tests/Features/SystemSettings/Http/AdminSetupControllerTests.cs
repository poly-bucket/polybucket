using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.Http;
using System.Threading.Tasks;
using Xunit;
using PolyBucket.Api.Common.Models;
using Microsoft.AspNetCore.Http;

namespace PolyBucket.Tests.Features.SystemSettings.Http
{
    public class AdminSetupControllerTests
    {
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly PolyBucketDbContext _context;
        private readonly AdminSetupController _controller;

        public AdminSetupControllerTests()
        {
            _passwordHasherMock = new Mock<IPasswordHasher>();
            var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString()) // Unique name for each test run
                .Options;
            _context = new PolyBucketDbContext(options);
            _controller = new AdminSetupController(_context, _passwordHasherMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Fact]
        public async Task AdminSetup_WhenNotCompleted_ShouldCreateAdminAndReturnOk()
        {
            // Arrange
            _passwordHasherMock.Setup(p => p.GenerateSalt()).Returns("salt");
            _passwordHasherMock.Setup(p => p.HashPassword(It.IsAny<string>(), It.IsAny<string>())).Returns("hashed_password");
            
            var request = new AdminSetupRequest { Username = "admin", Email = "admin@test.com", Password = "password" };

            // Act
            var result = await _controller.AdminSetup(request);

            // Assert
            Assert.IsType<OkResult>(result);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
            Assert.NotNull(user);
            Assert.Equal("admin@test.com", user.Email);
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == SystemSettingKeys.AdminSetupCompleted);
            Assert.NotNull(setting);
            Assert.Equal("true", setting.Value);

            _context.Database.EnsureDeleted();
        }

        [Fact]
        public async Task AdminSetup_WhenAlreadyCompleted_ShouldReturnBadRequest()
        {
            // Arrange
            _context.SystemSettings.Add(new SystemSetting { Key = SystemSettingKeys.AdminSetupCompleted, Value = "true" });
            await _context.SaveChangesAsync();
            var request = new AdminSetupRequest { Username = "admin", Email = "admin@test.com", Password = "password" };

            // Act
            var result = await _controller.AdminSetup(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            _context.Database.EnsureDeleted();
        }
    }
} 