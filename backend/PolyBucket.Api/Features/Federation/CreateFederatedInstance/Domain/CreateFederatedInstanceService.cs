using PolyBucket.Api.Features.Federation.CreateFederatedInstance.Http;
using PolyBucket.Api.Features.Federation.CreateFederatedInstance.Repository;

namespace PolyBucket.Api.Features.Federation.CreateFederatedInstance.Domain;

public class CreateFederatedInstanceService(ICreateFederatedInstanceRepository repository) : ICreateFederatedInstanceService
{
    public async Task<CreateFederatedInstanceResult> CreateAsync(CreateFederatedInstanceCommand command, CancellationToken cancellationToken = default)
    {
        var request = command.Request;
        if (string.IsNullOrWhiteSpace(request.Name))
            return new CreateFederatedInstanceResult { Outcome = CreateFederatedInstanceOutcome.ValidationError, ErrorMessage = "Instance name is required" };
        if (string.IsNullOrWhiteSpace(request.BaseUrl))
            return new CreateFederatedInstanceResult { Outcome = CreateFederatedInstanceOutcome.ValidationError, ErrorMessage = "Instance base URL is required" };
        if (!Uri.TryCreate(request.BaseUrl, UriKind.Absolute, out _))
            return new CreateFederatedInstanceResult { Outcome = CreateFederatedInstanceOutcome.ValidationError, ErrorMessage = "Invalid base URL format" };
        if (command.UserId == null || command.UserId == Guid.Empty)
            return new CreateFederatedInstanceResult { Outcome = CreateFederatedInstanceOutcome.Unauthorized, ErrorMessage = "User is not authenticated" };

        var userId = command.UserId.Value;
        var now = DateTime.UtcNow;
        var instance = new FederatedInstance
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            BaseUrl = request.BaseUrl.TrimEnd('/'),
            Description = request.Description,
            PublicKey = request.PublicKey,
            SharedSecret = request.SharedSecret,
            Status = FederationStatus.Pending,
            IsEnabled = request.IsEnabled ?? true,
            CreatedById = userId,
            CreatedAt = now,
            UpdatedById = userId
        };

        var created = await repository.AddAsync(instance, cancellationToken);
        return new CreateFederatedInstanceResult
        {
            Outcome = CreateFederatedInstanceOutcome.Created,
            Response = new CreateFederatedInstanceResponse
            {
                Id = created.Id,
                Name = created.Name,
                BaseUrl = created.BaseUrl,
                Status = created.Status.ToString(),
                Description = created.Description,
                IsEnabled = created.IsEnabled,
                CreatedAt = created.CreatedAt
            }
        };
    }
}
