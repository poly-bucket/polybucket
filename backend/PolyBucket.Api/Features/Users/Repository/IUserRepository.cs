using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Users.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.Repository
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<bool> IsEmailTakenAsync(string email, Guid? excludeUserId = null);
        Task<bool> IsUsernameTakenAsync(string username, Guid? excludeUserId = null);
        Task<UserProfile> GetUserProfileAsync(Guid id);
        Task<PolyBucket.Api.Features.Users.Domain.UserSettings?> GetSettingsByUserIdAsync(Guid userId);
        Task UpdateSettingsAsync(PolyBucket.Api.Features.Users.Domain.UserSettings settings);
    }

    public class UserProfile
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid? RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
} 