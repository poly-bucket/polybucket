using Core.Models.Users;
using Database;
using AutoMapper;
using Api.Controllers.Users.GetUser.Domain;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers.Users.GetUserById.Persistance
{
    public class GetUserByIdDataAccess(Context context, IMapper mapper, ILogger<GetUserByIdDataAccess> logger)
    {
        private readonly Context _context = context;
        private readonly ILogger<GetUserByIdDataAccess> _logger = logger;
        private readonly IMapper _mapper = mapper;

        public async Task<GetUserByIdResponse?> GetUserByIdAsync(Guid id)
        {
            _logger.LogDebug($"Getting user with ID {id}");

            User? user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                _logger.LogTrace($"User with ID {id} not found");
                throw new KeyNotFoundException($"User with ID {id} not found");
            }

            return _mapper.Map<User, GetUserByIdResponse>(user);
        }
    }
}