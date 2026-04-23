using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.Users.GetUsers.Domain;
using PolyBucket.Api.Features.Users.GetUsers.Http;
using Xunit;

namespace PolyBucket.Tests.Features.Users.GetUsers.Http;

public class GetUsersControllerTests
{
    private readonly Mock<IGetUsersService> _mockService;
    private readonly Mock<ILogger<GetUsersController>> _mockLogger;
    private readonly GetUsersController _controller;

    public GetUsersControllerTests()
    {
        _mockService = new Mock<IGetUsersService>();
        _mockLogger = new Mock<ILogger<GetUsersController>>();
        _controller = new GetUsersController(_mockService.Object, _mockLogger.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, "admin"),
            new(ClaimTypes.Role, "Admin")
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
        var expectedResponse = new GetUsersResult
        {
            Users = new List<UserListItemDto>
            {
                new()
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

        _mockService.Setup(s => s.GetUsersAsync(It.IsAny<GetUsersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.GetUsers(page: 1, pageSize: 20);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GetUsersResult>(okResult.Value);
        Assert.Equal(expectedResponse.TotalCount, response.TotalCount);
        Assert.Equal(expectedResponse.Users, response.Users);
    }

    [Fact]
    public async Task GetUsers_WithInvalidPage_ReturnsBadRequest()
    {
        var result = await _controller.GetUsers(page: 0);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Page number must be greater than 0", badRequestResult.Value);
    }

    [Fact]
    public async Task GetUsers_WithInvalidPageSize_ReturnsBadRequest()
    {
        var result = await _controller.GetUsers(pageSize: 101);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Page size must be between 1 and 100", badRequestResult.Value);
    }

    [Fact]
    public async Task GetUsers_WithInvalidSortField_ReturnsBadRequest()
    {
        var result = await _controller.GetUsers(sortBy: "invalidField");

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("Invalid sort field", badRequestResult.Value!.ToString());
    }

    [Fact]
    public async Task GetUsers_WithValidFilters_ReturnsOkResult()
    {
        var expectedResponse = new GetUsersResult
        {
            Users = new List<UserListItemDto>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 20,
            TotalPages = 0
        };

        _mockService.Setup(s => s.GetUsersAsync(It.IsAny<GetUsersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.GetUsers(
            page: 1,
            pageSize: 20,
            searchQuery: "test",
            roleFilter: "User",
            statusFilter: "Active",
            sortBy: "Username",
            sortDescending: false);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GetUsersResult>(okResult.Value);
        Assert.Equal(expectedResponse.TotalCount, response.TotalCount);
    }

    [Fact]
    public async Task GetUsers_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        _mockService.Setup(s => s.GetUsersAsync(It.IsAny<GetUsersQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.GetUsers();

        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Contains("An error occurred while retrieving users", statusCodeResult.Value!.ToString());
    }

    [Fact]
    public async Task GetUsers_WithDefaultParameters_UsesCorrectDefaults()
    {
        var expectedResponse = new GetUsersResult
        {
            Users = new List<UserListItemDto>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 20,
            TotalPages = 0
        };

        GetUsersQuery? capturedQuery = null;
        _mockService.Setup(s => s.GetUsersAsync(It.IsAny<GetUsersQuery>(), It.IsAny<CancellationToken>()))
            .Callback<GetUsersQuery, CancellationToken>((q, _) => capturedQuery = q)
            .ReturnsAsync(expectedResponse);

        await _controller.GetUsers();

        Assert.NotNull(capturedQuery);
        Assert.Equal(1, capturedQuery!.Page);
        Assert.Equal(20, capturedQuery.PageSize);
        Assert.Equal("CreatedAt", capturedQuery.SortBy);
        Assert.True(capturedQuery.SortDescending);
    }

    [Fact]
    public async Task GetUsers_WithCustomParameters_PassesCorrectValues()
    {
        var expectedResponse = new GetUsersResult
        {
            Users = new List<UserListItemDto>(),
            TotalCount = 0,
            Page = 2,
            PageSize = 50,
            TotalPages = 0
        };

        GetUsersQuery? capturedQuery = null;
        _mockService.Setup(s => s.GetUsersAsync(It.IsAny<GetUsersQuery>(), It.IsAny<CancellationToken>()))
            .Callback<GetUsersQuery, CancellationToken>((q, _) => capturedQuery = q)
            .ReturnsAsync(expectedResponse);

        await _controller.GetUsers(
            page: 2,
            pageSize: 50,
            searchQuery: "admin",
            roleFilter: "Admin",
            statusFilter: "Active",
            sortBy: "Email",
            sortDescending: false);

        Assert.NotNull(capturedQuery);
        Assert.Equal(2, capturedQuery!.Page);
        Assert.Equal(50, capturedQuery.PageSize);
        Assert.Equal("admin", capturedQuery.SearchQuery);
        Assert.Equal("Admin", capturedQuery.RoleFilter);
        Assert.Equal("Active", capturedQuery.StatusFilter);
        Assert.Equal("Email", capturedQuery.SortBy);
        Assert.False(capturedQuery.SortDescending);
    }
}
