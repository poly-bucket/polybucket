using PolyBucket.Api.Features.Federation.CreateFederatedInstance.Http;

namespace PolyBucket.Api.Features.Federation.CreateFederatedInstance.Domain;

public enum CreateFederatedInstanceOutcome
{
    Created,
    ValidationError,
    Unauthorized
}

public sealed class CreateFederatedInstanceCommand
{
    public required CreateFederatedInstanceRequest Request { get; init; }
    public Guid? UserId { get; init; }
}

public sealed class CreateFederatedInstanceResult
{
    public CreateFederatedInstanceOutcome Outcome { get; init; }
    public string? ErrorMessage { get; init; }
    public CreateFederatedInstanceResponse? Response { get; init; }
}

public interface ICreateFederatedInstanceService
{
    Task<CreateFederatedInstanceResult> CreateAsync(CreateFederatedInstanceCommand command, CancellationToken cancellationToken = default);
}
