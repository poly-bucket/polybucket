using PolyBucket.Api.Features.Files.GetSupportedExtensions.Repository;

        namespace PolyBucket.Api.Features.Files.GetSupportedExtensions.Domain;

        public interface IGetSupportedExtensionsService
        {
            Task<List<string>> GetSupportedExtensionsAsync(CancellationToken cancellationToken = default);
        }

        public class GetSupportedExtensionsService(IGetSupportedExtensionsRepository repository) : IGetSupportedExtensionsService
        {
            public Task<List<string>> GetSupportedExtensionsAsync(CancellationToken cancellationToken = default) =>
                repository.GetExtensionsAsync(cancellationToken);
        }
