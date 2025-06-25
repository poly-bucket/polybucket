using System;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Models.Repository;

namespace PolyBucket.Api.Features.Models.Domain
{
    public class ModelService : IModelService
    {
        private readonly IModelsRepository _modelsRepository;

        public ModelService(IModelsRepository modelsRepository)
        {
            _modelsRepository = modelsRepository;
        }

        public async Task<Model> GetModelByIdAsync(Guid id)
        {
            return await _modelsRepository.GetModelByIdAsync(id);
        }
    }
} 