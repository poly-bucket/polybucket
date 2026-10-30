using PolyBucket.Api.Features.Authentication.Services;

namespace PolyBucket.Api.Features.Authentication.GetRegistrationStatus.Domain;

public class GetRegistrationStatusService(IUserRegistrationGate registrationGate) : IGetRegistrationStatusService
{
    private readonly IUserRegistrationGate _registrationGate = registrationGate;

    public async Task<GetRegistrationStatusResponse> GetRegistrationStatusAsync(CancellationToken cancellationToken)
    {
        var allowed = await _registrationGate.IsRegistrationAllowedAsync(cancellationToken);
        return new GetRegistrationStatusResponse { AllowUserRegistration = allowed };
    }
}
