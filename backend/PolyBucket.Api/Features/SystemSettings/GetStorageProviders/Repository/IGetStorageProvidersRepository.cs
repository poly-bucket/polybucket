using PolyBucket.Storage;

namespace PolyBucket.Api.Features.SystemSettings.GetStorageProviders.Repository;

public interface IGetStorageProvidersRepository
{
    StorageProvidersResponseDto GetProviders();
}
