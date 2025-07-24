using System;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Models.Repository;

namespace PolyBucket.Api.Features.Models.Domain
{
    public class ModelService(IModelsRepository modelsRepository) : IModelService
    {
        private readonly IModelsRepository _modelsRepository = modelsRepository;

        public async Task<Model?> GetModelByIdAsync(Guid id)
        {
            return await _modelsRepository.GetModelByIdAsync(id);
        }
    }
} 