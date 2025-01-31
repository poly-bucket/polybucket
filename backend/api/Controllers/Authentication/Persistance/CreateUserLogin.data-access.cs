using AutoMapper;
using Core.Models.Users;
using Database;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers.Authentication.Persistance;

public class CreateUserLoginDataAccess
{
    private readonly Context _context;
    private readonly ILogger<CreateUserLoginDataAccess> _logger;
    private readonly IMapper _mapper;

    public CreateUserLoginDataAccess(Context context, ILogger<CreateUserLoginDataAccess> logger, IMapper mapper)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<GetUserLoginResponse?> PersistUserLoginAttempt(string email)
    {
        _logger.LogDebug($"Getting user with email {email}");

        User? user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            _logger.LogTrace($"User with email {email} not found");
            throw new KeyNotFoundException($"User with email {email} not found");
        }

        return _mapper.Map<User, GetUserLoginResponse>(user);
    }

    public async Task<User?> FindUserByEmailAsync(string email)
    {
        return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task CreateLoginRecordAsync(UserLogin userLogin)
    {
        await _context.UserLogins.AddAsync(userLogin);
        await _context.SaveChangesAsync();
    }
}
