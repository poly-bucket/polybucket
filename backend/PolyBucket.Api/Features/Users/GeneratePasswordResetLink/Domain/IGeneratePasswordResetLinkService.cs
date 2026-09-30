using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Http;

namespace PolyBucket.Api.Features.Users.GeneratePasswordResetLink.Domain;

public interface IGeneratePasswordResetLinkService
{
    Task<PasswordResetLinkResult> GenerateAsync(Guid userId, Guid performedByUserId, ClientRequestInfo client, CancellationToken cancellationToken = default);
}

public sealed record PasswordResetLinkResult(string Path, string? Url, DateTime ExpiresAt);
