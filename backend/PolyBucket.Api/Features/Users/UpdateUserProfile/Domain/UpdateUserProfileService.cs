using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Users.UpdateUserProfile.Repository;

namespace PolyBucket.Api.Features.Users.UpdateUserProfile.Domain;

public class UpdateUserProfileService(
    IUpdateUserProfileRepository repository,
    ILogger<UpdateUserProfileService> logger) : IUpdateUserProfileService
{
    public async Task UpdateAsync(UpdateUserProfileCommand command, CancellationToken cancellationToken = default)
    {
        var user = await repository.GetUserByIdForUpdateAsync(command.UserId, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {command.UserId} not found");
        }

        user.Bio = command.Bio;
        user.Country = command.Country;
        user.WebsiteUrl = command.WebsiteUrl;
        user.TwitterUrl = command.TwitterUrl;
        user.InstagramUrl = command.InstagramUrl;
        user.YouTubeUrl = command.YouTubeUrl;
        user.IsProfilePublic = command.IsProfilePublic;
        user.ShowEmail = command.ShowEmail;
        user.ShowLastLogin = command.ShowLastLogin;
        user.ShowStatistics = command.ShowStatistics;
        user.UpdatedAt = DateTime.UtcNow;

        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User profile updated for user {UserId}", command.UserId);
    }
}
