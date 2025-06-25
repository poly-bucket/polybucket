using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Users.Domain;
using System;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Enums;

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
        Task<UserSettings?> GetSettingsByUserIdAsync(Guid userId);
        Task UpdateSettingsAsync(UserSettings settings);
    }

    public class UserProfile
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 