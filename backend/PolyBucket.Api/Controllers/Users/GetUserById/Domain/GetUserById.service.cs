using Api.Controllers.Users.GetUser.Domain;
using Api.Controllers.Users.GetUserById.Persistance;

namespace Api.Controllers.Users.GetUserById.Domain
{
    public class GetUserByIdService
    {
        private readonly GetUserByIdDataAccess _dataAccess;
        private readonly ILogger<GetUserByIdService> _logger;

        public GetUserByIdService(GetUserByIdDataAccess dataAccess, ILogger<GetUserByIdService> logger)
        {
            _dataAccess = dataAccess;
            _logger = logger;
        }

        public async Task<GetUserByIdResponse> GetUserByIdAsync(Guid id)
        {
            try
            {
                var user = await _dataAccess.GetUserByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {Id} not found", id);
                    throw new KeyNotFoundException($"User with ID {id} not found");
                }
                return user;
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("User with ID {Id} not found", id);
                throw new KeyNotFoundException($"User with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID {Id}", id);
                throw new Exception("An error occurred while retrieving the user");
            }
        }
    }
}