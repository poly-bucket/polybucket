using PolyBucket.Api.Features.Users.Repository;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.Queries
{
    public class GetUserByIdQuery
    {
        public Guid Id { get; set; }
    }

    public class GetUserByIdResponse
    {
        public string Email { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Username { get; set; } = null!;
    }

    public class GetUserByIdQueryHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GetUserByIdQueryHandler> _logger;

        public GetUserByIdQueryHandler(IUserRepository userRepository, ILogger<GetUserByIdQueryHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<GetUserByIdResponse> Handle(GetUserByIdQuery query)
        {
            var user = await _userRepository.GetByIdAsync(query.Id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {query.Id} not found");
            }
            
            // This needs a mapper
            return new GetUserByIdResponse 
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username
            };
        }
    }
} 