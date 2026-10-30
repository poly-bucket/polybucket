using PolyBucket.Api.Features.Files.GetSupportedExtensionsByType.Repository;

        namespace PolyBucket.Api.Features.Files.GetSupportedExtensionsByType.Domain;

        public interface IGetSupportedExtensionsByTypeService
        {
            Task<List<string>> GetSupportedExtensionsForTypeAsync(string fileType, CancellationToken cancellationToken = default);
        }

        public class GetSupportedExtensionsByTypeService(IGetSupportedExtensionsByTypeRepository repository) : IGetSupportedExtensionsByTypeService
        {
            public Task<List<string>> GetSupportedExtensionsForTypeAsync(string fileType, CancellationToken cancellationToken = default) =>
                repository.GetExtensionsForTypeAsync(fileType, cancellationToken);
        }
