using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Http;

namespace PolyBucket.Api.Features.Users.MarkEmailVerified.Domain;

public interface IMarkEmailVerifiedService
{
    Task<DateTime> MarkEmailVerifiedAsync(Guid userId, Guid performedByUserId, ClientRequestInfo client, CancellationToken cancellationToken = default);
}
