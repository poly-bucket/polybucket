using Core.Models.Users;
using Microsoft.EntityFrameworkCore;
using Core.Interfaces;

namespace Api.Controllers.Authentication.Login.Persistance;

public class CreateUserLoginDataAccess(IUserRepository userRepository, ILogger<CreateUserLoginDataAccess> logger)
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ILogger<CreateUserLoginDataAccess> _logger = logger;

    public async Task<User?> FindUserByEmailAsync(string email)
    {
        if (email.Contains("@"))
        {
            return await _userRepository.GetByEmailAsync(email);
        }
        else
        {
            return await _userRepository.GetByUsernameAsync(email);
        }
    }

    public async Task CreateLoginRecordAsync(UserLogin userLogin)
    {
        //await _context.UserLogins.AddAsync(userLogin);
        //await _context.SaveChangesAsync();
    }
}