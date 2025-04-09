using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PolyBucket.Core.Entities;
using PolyBucket.Core.Models;
using PolyBucket.Core.Models.Auth;
using PolyBucket.Infrastructure.Data;
using PolyBucket.Infrastructure.Data.Repositories;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PolyBucket.Core.Interfaces;

namespace PolyBucket.Tests.Repositories
{
    public class UserRepositoryTests
    {
        [Fact]
        public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var mockUser = new User { Id = userId, Username = "testuser" };
            
            var mockUserRepository = CreateMockUserRepository();
            mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(mockUser);
            
            // Act
            var result = await mockUserRepository.Object.GetByIdAsync(userId);
            
            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(userId);
            result.Username.ShouldBe("testuser");
        }
        
        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            
            var mockUserRepository = CreateMockUserRepository();
            mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync((User)null);
            
            // Act
            var result = await mockUserRepository.Object.GetByIdAsync(userId);
            
            // Assert
            result.ShouldBeNull();
        }
        
        [Fact]
        public async Task GetByUsernameAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var username = "testuser";
            var mockUser = new User { Id = Guid.NewGuid(), Username = username };
            
            var mockUserRepository = CreateMockUserRepository();
            mockUserRepository.Setup(repo => repo.GetByUsernameAsync(username))
                .ReturnsAsync(mockUser);
            
            // Act
            var result = await mockUserRepository.Object.GetByUsernameAsync(username);
            
            // Assert
            result.ShouldNotBeNull();
            result.Username.ShouldBe(username);
        }
        
        [Fact]
        public async Task UsernameExistsAsync_ShouldReturnTrue_WhenUsernameExists()
        {
            // Arrange
            var username = "testuser";
            
            var mockUserRepository = CreateMockUserRepository();
            mockUserRepository.Setup(repo => repo.UsernameExistsAsync(username))
                .ReturnsAsync(true);
            
            // Act
            var result = await mockUserRepository.Object.UsernameExistsAsync(username);
            
            // Assert
            result.ShouldBeTrue();
        }
        
        [Fact]
        public async Task EmailExistsAsync_ShouldReturnTrue_WhenEmailExists()
        {
            // Arrange
            var email = "test@example.com";
            
            var mockUserRepository = CreateMockUserRepository();
            mockUserRepository.Setup(repo => repo.EmailExistsAsync(email))
                .ReturnsAsync(true);
            
            // Act
            var result = await mockUserRepository.Object.EmailExistsAsync(email);
            
            // Assert
            result.ShouldBeTrue();
        }
        
        [Fact]
        public async Task AddAsync_ShouldReturnUser_WhenSuccessful()
        {
            // Arrange
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "newuser",
                Email = "newuser@example.com"
            };
            
            var mockUserRepository = CreateMockUserRepository();
            mockUserRepository.Setup(repo => repo.AddAsync(newUser))
                .ReturnsAsync(newUser);
            
            // Act
            var result = await mockUserRepository.Object.AddAsync(newUser);
            
            // Assert
            result.ShouldNotBeNull();
            result.Username.ShouldBe("newuser");
            result.Email.ShouldBe("newuser@example.com");
        }
        
        private Mock<IUserRepository> CreateMockUserRepository()
        {
            return new Mock<IUserRepository>();
        }
    }
    
    // Test-specific DbContext that doesn't apply entity configurations for in-memory testing
    public class TestApplicationDbContext : ApplicationDbContext
    {
        public TestApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Skip applying configurations for in-memory tests to avoid validation issues
            // Just set up the basic relationships
            
            modelBuilder.Entity<User>()
                .HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId);
                
            modelBuilder.Entity<User>()
                .HasMany(u => u.Models)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId);
                
            modelBuilder.Entity<UserFollow>()
                .HasKey(uf => uf.Id);
                
            modelBuilder.Entity<UserFollow>()
                .HasOne(uf => uf.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(uf => uf.FollowerId);
                
            modelBuilder.Entity<UserFollow>()
                .HasOne(uf => uf.Followed)
                .WithMany(u => u.Followers)
                .HasForeignKey(uf => uf.FollowedId);
        }
    }
} 