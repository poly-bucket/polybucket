using MediatR;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Users.UpdateUserProfile.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.UpdateUserProfile.Handlers
{
    public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand>
    {
        private readonly PolyBucketDbContext _dbContext;
        private readonly ILogger<UpdateUserProfileCommandHandler> _logger;

        public UpdateUserProfileCommandHandler(PolyBucketDbContext dbContext, ILogger<UpdateUserProfileCommandHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {request.UserId} not found");
            }

            // Update profile information
            user.Bio = request.Bio;
            user.Country = request.Country;
            user.WebsiteUrl = request.WebsiteUrl;
            user.TwitterUrl = request.TwitterUrl;
            user.InstagramUrl = request.InstagramUrl;
            user.YouTubeUrl = request.YouTubeUrl;
            user.IsProfilePublic = request.IsProfilePublic;
            user.ShowEmail = request.ShowEmail;
            user.ShowLastLogin = request.ShowLastLogin;
            user.ShowStatistics = request.ShowStatistics;
            user.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("User profile updated for user {UserId}", request.UserId);
        }
    }
}
