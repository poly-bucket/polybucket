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

        public async Task<RegenerateAvatarResponse> RegenerateAvatarAsync(Guid userId, string? salt)
        {
            var user = await _repository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            // Generate avatar using the new method
            var avatar = _avatarService.GenerateUserAvatar(userId, salt);

            // Update user's avatar in database
            user.Avatar = avatar;
            await _repository.UpdateUserAsync(user);

            return new RegenerateAvatarResponse
            {
                Avatar = avatar,
                UserId = userId,
                Salt = salt
            };
        }
    }
} 