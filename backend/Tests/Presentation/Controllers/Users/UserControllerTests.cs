using Api.Controllers.Users.GetUser.Http;
using Api.Controllers.Users.GetUserById.Domain;
using Api.Controllers.Users.GetUserById.Persistance;
using AutoMapper;
using Conductors.Users;
using Core.Models.Users;
using Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace Tests.Presentation.Controllers.Users
{
    public class UserControllerTests : IDisposable
    {
        private readonly Mock<GetUserByIdService> _mockGetUserByIdService;
        private readonly Mock<GetUserByIdDataAccess> _mockGetUserByIdDataAccess;
        private readonly Mock<ILogger<UserController>> _mockLogger;
        private readonly Mock<IMapper> _mockMapper;
        private readonly UserController _controller;
        private readonly Context _context;

        public UserControllerTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "TestUserDb")
                .Options;
            _context = new Context(options);

            // Setup mocks
            _mockLogger = new Mock<ILogger<UserController>>();
            _mockMapper = new Mock<IMapper>();
            _mockGetUserByIdDataAccess = new Mock<GetUserByIdDataAccess>(_context, _mockMapper.Object, _mockLogger.Object);
            _mockGetUserByIdService = new Mock<GetUserByIdService>(_mockGetUserByIdDataAccess.Object, _mockLogger.Object);
            _controller = new UserController(_mockGetUserByIdService.Object, _mockLogger.Object);
        }

        // Clean up after each test
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}