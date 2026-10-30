using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PolyBucket.Database.Persistence;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using TwoFactorAuthDomain = PolyBucket.Database.Entities.Authentication;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.RegenerateBackupCodes.Repository
{
    public interface IRegenerateBackupCodesRepository
    {
        Task LockByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<TwoFactorAuthDomain.TwoFactorAuth?> GetByUserIdAsync(Guid userId);
        Task DeleteBackupCodesForTwoFactorAuthAsync(Guid twoFactorAuthId, CancellationToken cancellationToken = default);
        Task SaveChangesAsync();
    }

    public class RegenerateBackupCodesRepository : IRegenerateBackupCodesRepository
    {
        private readonly PolyBucketDbContext _context;

        public RegenerateBackupCodesRepository(PolyBucketDbContext context)
        {
            _context = context;
        }

        public async Task LockByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }

            await using var command = connection.CreateCommand();
            command.Transaction = _context.Database.CurrentTransaction?.GetDbTransaction();
            command.CommandText = "SELECT \"Id\" FROM \"TwoFactorAuths\" WHERE \"UserId\" = @userId FOR UPDATE";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "userId";
            parameter.Value = userId;
            command.Parameters.Add(parameter);
            await command.ExecuteScalarAsync(cancellationToken);
        }

        public async Task<TwoFactorAuthDomain.TwoFactorAuth?> GetByUserIdAsync(Guid userId)
        {
            return await _context.TwoFactorAuths
                .Include(tfa => tfa.User)
                .AsTracking()
                .FirstOrDefaultAsync(tfa => tfa.UserId == userId);
        }

        public async Task DeleteBackupCodesForTwoFactorAuthAsync(Guid twoFactorAuthId, CancellationToken cancellationToken = default)
        {
            await _context.BackupCodes
                .Where(bc => bc.TwoFactorAuthId == twoFactorAuthId)
                .ExecuteDeleteAsync(cancellationToken);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}