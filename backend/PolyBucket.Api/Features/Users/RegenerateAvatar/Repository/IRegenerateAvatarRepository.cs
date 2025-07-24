using PolyBucket.Api.Common.Models;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.RegenerateAvatar.Repository
{
    public interface IRegenerateAvatarRepository
    {
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<User> UpdateUserAsync(User user);
    }
} 