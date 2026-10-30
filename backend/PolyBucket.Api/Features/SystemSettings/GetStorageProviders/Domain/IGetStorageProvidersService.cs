using PolyBucket.Storage;

namespace PolyBucket.Api.Features.SystemSettings.GetStorageProviders.Domain;

public interface IGetStorageProvidersService
{
    StorageProvidersResponseDto GetStorageProviders();
}
