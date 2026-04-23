using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.GetUsers.Repository;
using Microsoft.Extensions.Logging;

namespace PolyBucket.Api.Features.Users.GetUsers.Domain;

public class GetUsersService(IGetUsersRepository repository, ILogger<GetUsersService> logger) : IGetUsersService
{
    private readonly IGetUsersRepository _repository = repository;
    private readonly ILogger<GetUsersService> _logger = logger;

    public async Task<GetUsersResult> GetUsersAsync(GetUsersQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _repository.GetPagedAsync(query, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching users");
            throw;
        }
    }
}
