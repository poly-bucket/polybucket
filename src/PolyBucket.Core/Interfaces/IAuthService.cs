using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Core.Entities;
using PolyBucket.Core.Models.Auth;

namespace PolyBucket.Core.Interfaces
{
    public interface IAuthService
    {
        // Legacy methods with ServiceResponse
        Task<ServiceResponse<bool>> IsFirstRunAsync(CancellationToken cancellationToken = default);
        Task<ServiceResponse<UserResponse>> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
        Task<ServiceResponse<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
        Task<ServiceResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
        Task<ServiceResponse<bool>> VerifyEmailAsync(string token, CancellationToken cancellationToken = default);
        Task<ServiceResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
        Task<ServiceResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
        Task<ServiceResponse<bool>> ChangePasswordAsync(ChangePasswordRequest request, Guid userId, CancellationToken cancellationToken = default);
        
        // New exception-based methods
        Task<bool> IsFirstRunExAsync(CancellationToken cancellationToken = default);
        Task<UserResponse> RegisterExAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
        Task<AuthResponse> LoginExAsync(LoginRequest request, CancellationToken cancellationToken = default);
        Task<AuthResponse> RefreshTokenExAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
        Task<bool> VerifyEmailExAsync(string token, CancellationToken cancellationToken = default);
        Task<bool> ForgotPasswordExAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
        Task<bool> ResetPasswordExAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
        Task<bool> ChangePasswordExAsync(ChangePasswordRequest request, Guid userId, CancellationToken cancellationToken = default);
        
        // Setup Wizard Status Methods
        Task<ServiceResponse<SystemSetup>> GetSetupStatusAsync(CancellationToken cancellationToken = default);
        Task<ServiceResponse<bool>> IsAdminConfiguredAsync(CancellationToken cancellationToken = default);
        Task<ServiceResponse<bool>> IsRoleConfiguredAsync(CancellationToken cancellationToken = default);
        Task<ServiceResponse<bool>> SetAdminConfiguredAsync(bool isConfigured, CancellationToken cancellationToken = default);
        Task<ServiceResponse<bool>> SetRoleConfiguredAsync(bool isConfigured, CancellationToken cancellationToken = default);
        
        // Setup Wizard Status Methods (Exception-based)
        Task<SystemSetup> GetSetupStatusExAsync(CancellationToken cancellationToken = default);
        Task<bool> IsAdminConfiguredExAsync(CancellationToken cancellationToken = default);
        Task<bool> IsRoleConfiguredExAsync(CancellationToken cancellationToken = default);
        Task SetAdminConfiguredExAsync(bool isConfigured, CancellationToken cancellationToken = default);
        Task SetRoleConfiguredExAsync(bool isConfigured, CancellationToken cancellationToken = default);
    }
} 