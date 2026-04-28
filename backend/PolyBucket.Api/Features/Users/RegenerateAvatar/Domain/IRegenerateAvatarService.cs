using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.RegenerateAvatar.Domain
{
    public interface IRegenerateAvatarService
    {
        Task<RegenerateAvatarResponse> RegenerateAvatarAsync(Guid userId, string? salt, string? avatar);
    }

    public class RegenerateAvatarRequest
    {
        public string? Salt { get; set; }
        public string? Avatar { get; set; }
    }

    public class RegenerateAvatarResponse
    {
        public string Avatar { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string? Salt { get; set; }
    }
} 