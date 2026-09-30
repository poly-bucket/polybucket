using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Common.Http;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Email.Domain;
using PolyBucket.Api.Features.Users.Domain;
using PolyBucket.Api.Features.Users.GeneratePasswordResetLink.Repository;

namespace PolyBucket.Api.Features.Users.GeneratePasswordResetLink.Domain;

public class GeneratePasswordResetLinkService(
    IGeneratePasswordResetLinkRepository repository,
    ITokenService tokenService,
    IEmailSettingsResolver emailSettingsResolver,
    TimeProvider timeProvider,
    ILogger<GeneratePasswordResetLinkService> logger) : IGeneratePasswordResetLinkService
{
    public static readonly TimeSpan LinkLifetime = TimeSpan.FromHours(24);

    public async Task<PasswordResetLinkResult> GenerateAsync(Guid userId, Guid performedByUserId, ClientRequestInfo client, CancellationToken cancellationToken = default)
    {
        var user = await repository.FindByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found");

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var expiresAt = now.Add(LinkLifetime);
        var rawToken = tokenService.GeneratePasswordResetToken();

        repository.AddPasswordResetToken(new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            Token = TokenHasher.Hash(rawToken),
            Email = user.Email,
            ExpiresAt = expiresAt,
            CreatedAt = now,
            CreatedByIp = client.IpAddress
        });
        repository.AddAuditLog(new UserAuditLog
        {
            UserId = user.Id,
            PerformedByUserId = performedByUserId,
            Action = UserAuditAction.PasswordResetLinkGenerated,
            Details = $"Password reset link generated, expires {expiresAt:O}",
            IpAddress = client.IpAddress,
            CreatedAt = now
        });
        await repository.SaveChangesAsync(cancellationToken);

        var path = $"{AccountEmailService.ResetPasswordPath}?token={Uri.EscapeDataString(rawToken)}";
        var settings = await emailSettingsResolver.GetEffectiveSettingsAsync(cancellationToken);
        var url = settings.HasPublicBaseUrl ? settings.BuildUrl(path) : null;

        logger.LogInformation("Password reset link generated for user {UserId} by {AdminId}", user.Id, performedByUserId);
        return new PasswordResetLinkResult(path, url, expiresAt);
    }
}
