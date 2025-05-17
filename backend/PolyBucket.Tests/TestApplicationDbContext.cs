using Core.Models.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Core.Models;
using Core.Models.Auth;
using Infrastructure.Data;
using System;

namespace PolyBucket.Tests
{
    /// <summary>
    /// Custom test version of ApplicationDbContext with simplified configuration for in-memory testing
    /// </summary>
    public class TestApplicationDbContext : Infrastructure.Data.DatabaseContext
    {
        public TestApplicationDbContext(DbContextOptions<Infrastructure.Data.DatabaseContext> options) 
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            
            // Disable nullability validation for tests
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.ConfigureWarnings(w => 
                w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.NavigationBaseIncludeIgnored));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // For test context, just set up minimal validation requirements
            
            // Configure Role entity
            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Description).IsRequired(false);
                
                // Configure soft delete query filter
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });
            
            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Username).IsRequired();
                entity.Property(e => e.Email).IsRequired();
                
                // Set up relationships
                entity.HasMany(u => u.RefreshTokens)
                    .WithOne(rt => rt.User)
                    .HasForeignKey(rt => rt.UserId);
                
                entity.HasMany(u => u.Collections)
                    .WithOne(c => c.User)
                    .HasForeignKey(c => c.UserId);
            });
            
            // Configure RefreshToken entity
            modelBuilder.Entity<Core.Models.RefreshToken.RefreshToken>(entity =>
            {
                entity.Property(e => e.Token).IsRequired();
            });
        }
    }
} 