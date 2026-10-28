using PolyBucket.Api.Common.Services;
using PolyBucket.Api.Features.Users.RegenerateAvatar.Repository;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.RegenerateAvatar.Domain
{
    public class RegenerateAvatarService(
        IRegenerateAvatarRepository repository,
        IAvatarService avatarService,
        IAvatarSvgValidator avatarSvgValidator) : IRegenerateAvatarService
    {
        private readonly IRegenerateAvatarRepository _repository = repository;
        private readonly IAvatarService _avatarService = avatarService;
        private readonly IAvatarSvgValidator _avatarSvgValidator = avatarSvgValidator;

        public async Task<RegenerateAvatarResponse> RegenerateAvatarAsync(Guid userId, string? salt, string? avatar)
        {
            var user = await _repository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            string nextAvatar;
            if (string.IsNullOrWhiteSpace(avatar))
            {
                nextAvatar = _avatarService.GenerateUserAvatar(userId, salt);
            }
            else if (!_avatarSvgValidator.TryValidate(avatar, out var sanitized, out var error))
            {
                throw new ArgumentException(error ?? "Invalid avatar.");
            }
            else
            {
                nextAvatar = sanitized ?? avatar;
            }

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