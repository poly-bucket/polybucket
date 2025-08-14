using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.Users.Queries.GetUsers;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PolyBucket.Tests.Features.Users.Queries.GetUsers
{
    public class GetUsersControllerTests
    {
        private readonly Mock<GetUsersQueryHandler> _mockHandler;
        private readonly Mock<ILogger<GetUsersController>> _mockLogger;
        private readonly GetUsersController _controller;

        public GetUsersControllerTests()
        {
            _mockHandler = new Mock<GetUsersQueryHandler>();
            _mockLogger = new Mock<ILogger<GetUsersController>>();
            _controller = new GetUsersController(_mockHandler.Object, _mockLogger.Object);
            
            // Setup default user context
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task GetUsers_WithValidParameters_ReturnsOkResult()
        {
            // Arrange
            var expectedResponse = new GetUsersResponse
            {
                Users = new List<UserDto>
                {
                    new UserDto
                    {
                        Id = Guid.NewGuid(),
                        Username = "testuser",
                        Email = "test@example.com",
                        RoleName = "User",
                        CreatedAt = DateTime.UtcNow
                    }
                },
                TotalCount = 1,
                Page = 1,
                PageSize = 20,
                TotalPages = 1
            };

            _mockHandler.Setup(h => h.Handle(It.IsAny<GetUsersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetUsers(page: 1, pageSize: 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<GetUsersResponse>(okResult.Value);
            Assert.Equal(expectedResponse.TotalCount, response.TotalCount);
            Assert.Equal(expectedResponse.Users, response.Users);
        }

        [Fact]
        public async Task GetUsers_WithInvalidPage_ReturnsBadRequest()
        {
            // Arrange
            var invalidPage = 0;

            // Act
            var result = await _controller.GetUsers(page: invalidPage);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Page number must be greater than 0", badRequestResult.Value);
        }

        [Fact]
        public async Task GetUsers_WithInvalidPageSize_ReturnsBadRequest()
        {
            // Arrange
            var invalidPageSize = 101;

            // Act
            var result = await _controller.GetUsers(pageSize: invalidPageSize);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Page size must be between 1 and 100", badRequestResult.Value);
        }

        [Fact]
        public async Task GetUsers_WithInvalidSortField_ReturnsBadRequest()
        {
            // Arrange
            var invalidSortField = "invalidField";

            // Act
            var result = await _controller.GetUsers(sortBy: invalidSortField);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("Invalid sort field", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task GetUsers_WithValidFilters_ReturnsOkResult()
        {
            // Arrange
            var expectedResponse = new GetUsersResponse
            {
                Users = new List<UserDto>(),
                TotalCount = 0,
                Page = 1,
                PageSize = 20,
                TotalPages = 0
            };

            _mockHandler.Setup(h => h.Handle(It.IsAny<GetUsersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetUsers(
                page: 1,
                pageSize: 20,
                searchQuery: "test",
                roleFilter: "User",
                statusFilter: "Active",
                sortBy: "Username",
                sortDescending: false);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<GetUsersResponse>(okResult.Value);
            Assert.Equal(expectedResponse.TotalCount, response.TotalCount);
        }

        [Fact]
        public async Task GetUsers_WhenHandlerThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            _mockHandler.Setup(h => h.Handle(It.IsAny<GetUsersQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetUsers();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Contains("An error occurred while retrieving users", statusCodeResult.Value.ToString());
        }

        [Fact]
        public async Task GetUsers_WithDefaultParameters_UsesCorrectDefaults()
        {
            // Arrange
            var expectedResponse = new GetUsersResponse
            {
                Users = new List<UserDto>(),
                TotalCount = 0,
                Page = 1,
                PageSize = 20,
                TotalPages = 0
            };

            GetUsersQuery capturedQuery = null;
            _mockHandler.Setup(h => h.Handle(It.IsAny<GetUsersQuery>(), It.IsAny<CancellationToken>()))
                .Callback<GetUsersQuery, CancellationToken>((query, token) => capturedQuery = query)
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetUsers();

            // Assert
            Assert.NotNull(capturedQuery);
            Assert.Equal(1, capturedQuery.Page);
            Assert.Equal(20, capturedQuery.PageSize);
            Assert.Equal("CreatedAt", capturedQuery.SortBy);
            Assert.True(capturedQuery.SortDescending);
        }

        [Fact]
        public async Task GetUsers_WithCustomParameters_PassesCorrectValues()
        {
            // Arrange
            var expectedResponse = new GetUsersResponse
            {
                Users = new List<UserDto>(),
                TotalCount = 0,
                Page = 2,
                PageSize = 50,
                TotalPages = 0
            };

            GetUsersQuery capturedQuery = null;
            _mockHandler.Setup(h => h.Handle(It.IsAny<GetUsersQuery>(), It.IsAny<CancellationToken>()))
                .Callback<GetUsersQuery, CancellationToken>((query, token) => capturedQuery = query)
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetUsers(
                page: 2,
                pageSize: 50,
                searchQuery: "admin",
                roleFilter: "Admin",
                statusFilter: "Active",
                sortBy: "Email",
                sortDescending: false);

            // Assert
            Assert.NotNull(capturedQuery);
            Assert.Equal(2, capturedQuery.Page);
            Assert.Equal(50, capturedQuery.PageSize);
            Assert.Equal("admin", capturedQuery.SearchQuery);
            Assert.Equal("Admin", capturedQuery.RoleFilter);
            Assert.Equal("Active", capturedQuery.StatusFilter);
            Assert.Equal("Email", capturedQuery.SortBy);
            Assert.False(capturedQuery.SortDescending);
        }
    }
}
