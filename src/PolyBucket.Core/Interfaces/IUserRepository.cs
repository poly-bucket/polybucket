using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolyBucket.Core.Entities;

namespace PolyBucket.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(Guid id);
        Task<User> GetByUsernameAsync(string username);
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByRefreshTokenAsync(string refreshToken);
        Task<IEnumerable<User>> GetAllAsync(int skip = 0, int take = 20);
        Task<User> AddAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task<int> GetTotalCountAsync();
    }
} 