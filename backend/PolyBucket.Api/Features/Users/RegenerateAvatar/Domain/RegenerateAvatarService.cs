using PolyBucket.Api.Common.Services;
using PolyBucket.Api.Features.Users.RegenerateAvatar.Repository;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.RegenerateAvatar.Domain
{
    public class RegenerateAvatarService(IRegenerateAvatarRepository repository, IAvatarService avatarService) : IRegenerateAvatarService
    {
        private readonly IRegenerateAvatarRepository _repository = repository;
        private readonly IAvatarService _avatarService = avatarService;

        public async Task<RegenerateAvatarResponse> RegenerateAvatarAsync(Guid userId, string? salt, string? avatar)
        {
            var user = await _repository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            var nextAvatar = string.IsNullOrWhiteSpace(avatar)
                ? _avatarService.GenerateUserAvatar(userId, salt)
                : avatar;

            // Update user's avatar in database
            user.Avatar = nextAvatar;
            await _repository.UpdateUserAsync(user);

            return new RegenerateAvatarResponse
            {
                Avatar = nextAvatar,
                UserId = userId,
                Salt = salt
            };
        }
    }
} 