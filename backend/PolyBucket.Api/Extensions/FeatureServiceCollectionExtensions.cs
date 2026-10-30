using MediatR;
using PolyBucket.Api.Features.Models;
using PolyBucket.Api.Features.Users;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.Reports.Plugins;
using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Common.Services;
using PolyBucket.Api.Features.SystemSettings.Plugins;
using PolyBucket.Api.Features.SystemSettings;
using PolyBucket.Api.Features.ACL;
using PolyBucket.Api.Features.ThemeManagement;
using PolyBucket.Api.Features.ModelModeration;
using PolyBucket.Email.DependencyInjection;
using PolyBucket.Storage.DependencyInjection;
using PolyBucket.Api.Features.Comments;
using PolyBucket.Api.Features.Notifications;
using PolyBucket.Api.Features.Plugins;
using PolyBucket.Api.Features.Filaments;
using PolyBucket.Api.Features.Email;
using PolyBucket.Api.Features.Federation;
using PolyBucket.Api.Features.Files;
using PolyBucket.Api.Features.Printers;
using PolyBucket.Api.Features.Search;

namespace PolyBucket.Api.Extensions;

public static class FeatureServiceCollectionExtensions
{
    public static IServiceCollection AddPolyBucketFeatures(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

        services.AddModelsFeature();
        services.AddModelModerationFeature();
        services.AddEmailFeatureServices();
        services.AddEmailApiFeature();
        services.AddFilamentsFeature();
        services.AddFilesFeature();
        services.AddPrintersFeature();
        services.AddSearchFeature();
        services.AddStorageFeatureServices();
        services.AddNotificationsFeature();

        // Collections
        services.AddTransient<Features.Collections.CreateCollection.Repository.ICollectionRepository, Features.Collections.CreateCollection.Repository.CollectionRepository>();
        services.AddTransient<Features.Collections.GetCollectionById.Repository.ICollectionRepository, Features.Collections.GetCollectionById.Repository.CollectionRepository>();
        services.AddTransient<Features.Collections.UpdateCollection.Repository.ICollectionRepository, Features.Collections.UpdateCollection.Repository.CollectionRepository>();
        services.AddTransient<Features.Collections.DeleteCollection.Repository.ICollectionRepository, Features.Collections.DeleteCollection.Repository.CollectionRepository>();
        services.AddTransient<Features.Collections.GetUserCollections.Repository.ICollectionRepository, Features.Collections.GetUserCollections.Repository.CollectionRepository>();
        services.AddTransient<Features.Collections.GetUserCollections.Domain.IGetUserCollectionsService, Features.Collections.GetUserCollections.Domain.GetUserCollectionsService>();
        services.AddTransient<Features.Collections.GetFavoriteCollections.Repository.IGetFavoriteCollectionsRepository, Features.Collections.GetFavoriteCollections.Repository.GetFavoriteCollectionsRepository>();
        services.AddTransient<Features.Collections.FavoriteCollection.Repository.ICollectionRepository, Features.Collections.FavoriteCollection.Repository.CollectionRepository>();
        services.AddTransient<Features.Collections.AddModelToCollection.Repository.ICollectionRepository, Features.Collections.AddModelToCollection.Repository.CollectionRepository>();
        services.AddTransient<Features.Collections.RemoveModelFromCollection.Repository.ICollectionRepository, Features.Collections.RemoveModelFromCollection.Repository.CollectionRepository>();
        services.AddTransient<Features.Collections.AccessCollection.Repository.ICollectionRepository, Features.Collections.AccessCollection.Repository.CollectionRepository>();
        
        // Collection Command Handlers
        services.AddTransient<Features.Collections.CreateCollection.Domain.CreateCollectionCommandHandler>();
        services.AddTransient<Features.Collections.UpdateCollection.Domain.UpdateCollectionCommandHandler>();
        services.AddTransient<Features.Collections.DeleteCollection.Domain.DeleteCollectionCommandHandler>();
        services.AddTransient<Features.Collections.GetCollectionById.Domain.GetCollectionByIdQueryHandler>();

        services.AddTransient<Features.Collections.AddModelToCollection.Domain.AddModelToCollectionCommandHandler>();
        services.AddTransient<Features.Collections.RemoveModelFromCollection.Domain.RemoveModelFromCollectionCommandHandler>();
        services.AddTransient<Features.Collections.AccessCollection.Domain.AccessCollectionCommandHandler>();

        services.AddUsersFeature();
        services.AddTransient<Features.Authentication.TwoFactorAuth.InitializeTwoFactorAuth.Repository.IInitializeTwoFactorAuthUserReadRepository, Features.Authentication.TwoFactorAuth.InitializeTwoFactorAuth.Repository.InitializeTwoFactorAuthUserReadRepository>();
        services.AddTransient<Features.Authentication.TwoFactorAuth.EnableTwoFactorAuth.Repository.IEnableTwoFactorAuthUserReadRepository, Features.Authentication.TwoFactorAuth.EnableTwoFactorAuth.Repository.EnableTwoFactorAuthUserReadRepository>();
        
        // Authentication
        services.AddTransient<Features.Authentication.Repository.IAuthenticationRepository, Features.Authentication.Repository.AuthenticationRepository>();
        services.AddTransient<Features.Authentication.Services.ITokenService, Features.Authentication.Services.TokenService>();
        // Two-Factor Authentication Services (Feature-specific)
        services.AddTransient<Features.Authentication.TwoFactorAuth.InitializeTwoFactorAuth.Domain.IInitializeTwoFactorAuthService, Features.Authentication.TwoFactorAuth.InitializeTwoFactorAuth.Domain.InitializeTwoFactorAuthService>();
        services.AddTransient<Features.Authentication.TwoFactorAuth.InitializeTwoFactorAuth.Repository.IInitializeTwoFactorAuthRepository, Features.Authentication.TwoFactorAuth.InitializeTwoFactorAuth.Repository.InitializeTwoFactorAuthRepository>();
        
        services.AddTransient<Features.Authentication.TwoFactorAuth.EnableTwoFactorAuth.Domain.IEnableTwoFactorAuthService, Features.Authentication.TwoFactorAuth.EnableTwoFactorAuth.Domain.EnableTwoFactorAuthService>();
        services.AddTransient<Features.Authentication.TwoFactorAuth.EnableTwoFactorAuth.Repository.IEnableTwoFactorAuthRepository, Features.Authentication.TwoFactorAuth.EnableTwoFactorAuth.Repository.EnableTwoFactorAuthRepository>();
        
        services.AddTransient<Features.Authentication.TwoFactorAuth.DisableTwoFactorAuth.Domain.IDisableTwoFactorAuthService, Features.Authentication.TwoFactorAuth.DisableTwoFactorAuth.Domain.DisableTwoFactorAuthService>();
        services.AddTransient<Features.Authentication.TwoFactorAuth.DisableTwoFactorAuth.Repository.IDisableTwoFactorAuthRepository, Features.Authentication.TwoFactorAuth.DisableTwoFactorAuth.Repository.DisableTwoFactorAuthRepository>();
        
        services.AddTransient<Features.Authentication.TwoFactorAuth.GetTwoFactorAuthStatus.Repository.IGetTwoFactorAuthStatusRepository, Features.Authentication.TwoFactorAuth.GetTwoFactorAuthStatus.Repository.GetTwoFactorAuthStatusRepository>();
        
        // RegenerateBackupCodes 2FA services
        services.AddTransient<Features.Authentication.TwoFactorAuth.RegenerateBackupCodes.Repository.IRegenerateBackupCodesRepository, Features.Authentication.TwoFactorAuth.RegenerateBackupCodes.Repository.RegenerateBackupCodesRepository>();
        services.AddTransient<Features.Authentication.TwoFactorAuth.RegenerateBackupCodes.Domain.IRegenerateBackupCodesService, Features.Authentication.TwoFactorAuth.RegenerateBackupCodes.Domain.RegenerateBackupCodesService>();
        services.AddTransient<Features.Authentication.TwoFactorAuth.RegenerateBackupCodes.Domain.RegenerateBackupCodesCommandHandler>();
        
        // Login-specific 2FA services
        services.AddTransient<Features.Authentication.Login.Domain.ILoginTwoFactorAuthService, Features.Authentication.Login.Domain.LoginTwoFactorAuthService>();
        services.AddTransient<Features.Authentication.Login.Repository.ILoginTwoFactorAuthRepository, Features.Authentication.Login.Repository.LoginTwoFactorAuthRepository>();
        services.AddScoped<Features.Authentication.DeleteAccount.Domain.IDeleteAccountService, Features.Authentication.DeleteAccount.Domain.DeleteAccountService>();
        services.AddTransient<Features.Authentication.DeleteAccount.Repository.IDeleteAccountRepository, Features.Authentication.DeleteAccount.Repository.DeleteAccountRepository>();
        services.AddScoped<Features.Authentication.ExportAccount.Domain.IExportAccountService, Features.Authentication.ExportAccount.Domain.ExportAccountService>();
        services.AddTransient<Features.Authentication.ExportAccount.Repository.IExportAccountRepository, Features.Authentication.ExportAccount.Repository.ExportAccountRepository>();
        services.AddScoped<Features.Authentication.GetActiveSessions.Domain.IGetActiveSessionsService, Features.Authentication.GetActiveSessions.Domain.GetActiveSessionsService>();
        services.AddTransient<Features.Authentication.GetActiveSessions.Repository.IGetActiveSessionsRepository, Features.Authentication.GetActiveSessions.Repository.GetActiveSessionsRepository>();
        services.AddScoped<Features.Authentication.RevokeSession.Domain.IRevokeSessionService, Features.Authentication.RevokeSession.Domain.RevokeSessionService>();
        services.AddTransient<Features.Authentication.RevokeSession.Repository.IRevokeSessionRepository, Features.Authentication.RevokeSession.Repository.RevokeSessionRepository>();
        services.AddScoped<Features.Authentication.RevokeAllSessions.Domain.IRevokeAllSessionsService, Features.Authentication.RevokeAllSessions.Domain.RevokeAllSessionsService>();
        services.AddTransient<Features.Authentication.RevokeAllSessions.Repository.IRevokeAllSessionsRepository, Features.Authentication.RevokeAllSessions.Repository.RevokeAllSessionsRepository>();
        services.AddScoped<Features.Authentication.Me.Domain.IMeService, Features.Authentication.Me.Domain.MeService>();
        services.AddTransient<Features.Authentication.Me.Repository.IMeRepository, Features.Authentication.Me.Repository.MeRepository>();
        services.AddScoped<Features.Authentication.GetRegistrationStatus.Domain.IGetRegistrationStatusService, Features.Authentication.GetRegistrationStatus.Domain.GetRegistrationStatusService>();
        services.AddTransient<Features.Authentication.GetRegistrationStatus.Repository.IGetRegistrationStatusRepository, Features.Authentication.GetRegistrationStatus.Repository.GetRegistrationStatusRepository>();
        services.AddTransient<Features.Authentication.ForgotPassword.Repository.IForgotPasswordRepository, Features.Authentication.ForgotPassword.Repository.ForgotPasswordRepository>();
        services.AddTransient<Features.Authentication.Logout.Repository.ILogoutRepository, Features.Authentication.Logout.Repository.LogoutRepository>();
        services.AddTransient<Features.Authentication.RefreshToken.Repository.IRefreshTokenRepository, Features.Authentication.RefreshToken.Repository.RefreshTokenRepository>();
        services.AddTransient<Features.Authentication.Register.Repository.IRegisterRepository, Features.Authentication.Register.Repository.RegisterRepository>();
        services.AddTransient<Features.Authentication.ResetPassword.Repository.IResetPasswordRepository, Features.Authentication.ResetPassword.Repository.ResetPasswordRepository>();
        services.AddTransient<Features.Authentication.VerifyEmail.Repository.IVerifyEmailRepository, Features.Authentication.VerifyEmail.Repository.VerifyEmailRepository>();
        services.AddTransient<Features.Authentication.ResendVerificationEmail.Repository.IResendVerificationEmailRepository, Features.Authentication.ResendVerificationEmail.Repository.ResendVerificationEmailRepository>();
        services.AddTransient<Features.Authentication.RequestEmailChange.Repository.IRequestEmailChangeRepository, Features.Authentication.RequestEmailChange.Repository.RequestEmailChangeRepository>();
        services.AddTransient<Features.Authentication.ChangePassword.Repository.IChangePasswordRepository, Features.Authentication.ChangePassword.Repository.ChangePasswordRepository>();
        services.AddScoped<Features.Authentication.SkipPasswordChange.Domain.ISkipPasswordChangeService, Features.Authentication.SkipPasswordChange.Domain.SkipPasswordChangeService>();
        services.AddTransient<Features.Authentication.SkipPasswordChange.Repository.ISkipPasswordChangeRepository, Features.Authentication.SkipPasswordChange.Repository.SkipPasswordChangeRepository>();
        services.AddTransient<Features.Authentication.OAuthAuthorize.Repository.IOAuthAuthorizeRepository, Features.Authentication.OAuthAuthorize.Repository.OAuthAuthorizeRepository>();
        services.AddTransient<Features.Authentication.OAuthCallback.Repository.IOAuthCallbackRepository, Features.Authentication.OAuthCallback.Repository.OAuthCallbackRepository>();
        services.AddTransient<Features.Authentication.Register.Domain.RegisterCommandHandler>();
        services.AddTransient<Features.Authentication.Login.Domain.LoginCommandHandler>();
        services.AddTransient<Features.Authentication.ChangePassword.Domain.ChangePasswordCommandHandler>();
        services.AddTransient<Features.Authentication.TwoFactorAuth.InitializeTwoFactorAuth.Domain.InitializeTwoFactorAuthCommandHandler>();
        services.AddTransient<Features.Authentication.TwoFactorAuth.EnableTwoFactorAuth.Domain.EnableTwoFactorAuthCommandHandler>();
        services.AddTransient<Features.Authentication.TwoFactorAuth.DisableTwoFactorAuth.Domain.DisableTwoFactorAuthCommandHandler>();
        services.AddTransient<Features.Authentication.TwoFactorAuth.GetTwoFactorAuthStatus.Domain.GetTwoFactorAuthStatusQueryHandler>();
        services.AddTransient<Features.Authentication.RefreshToken.Domain.RefreshTokenCommandHandler>();
        services.AddTransient<Features.Authentication.Logout.Domain.LogoutCommandHandler>();
        services.AddTransient<Features.Authentication.ForgotPassword.Domain.ForgotPasswordCommandHandler>();
        services.AddTransient<Features.Authentication.ResetPassword.Domain.ResetPasswordCommandHandler>();
        services.AddTransient<Features.Authentication.VerifyEmail.Domain.VerifyEmailCommandHandler>();
        services.AddTransient<Features.Authentication.ResendVerificationEmail.Domain.ResendVerificationEmailCommandHandler>();
        services.AddTransient<Features.Authentication.RequestEmailChange.Domain.RequestEmailChangeCommandHandler>();
        services.AddScoped<Features.Authentication.Services.IEmailVerificationGate, Features.Authentication.Services.EmailVerificationGate>();
        services.AddScoped<Features.Authentication.Services.IUserRegistrationGate, Features.Authentication.Services.UserRegistrationGate>();
        
        // System Settings
        services.AddSystemSettingsFeature();
        
        services.AddAclFeature();
        services.AddThemeManagementFeature();
        
        // Admin Model Statistics
        services.AddTransient<Features.Admin.GetAdminModelStatistics.Domain.IGetAdminModelStatisticsService, Features.Admin.GetAdminModelStatistics.Domain.GetAdminModelStatisticsService>();
        services.AddTransient<Features.Admin.GetAdminModelStatistics.Repository.IGetAdminModelStatisticsRepository, Features.Admin.GetAdminModelStatistics.Repository.GetAdminModelStatisticsRepository>();
        services.AddScoped<Features.Admin.GetModerationAuditLogs.Domain.IGetModerationAuditLogsService, Features.Admin.GetModerationAuditLogs.Domain.GetModerationAuditLogsService>();
        services.AddTransient<Features.Admin.GetModerationAuditLogs.Repository.IGetModerationAuditLogsRepository, Features.Admin.GetModerationAuditLogs.Repository.GetModerationAuditLogsRepository>();
        
        // Avatar Service
        services.AddTransient<Common.Services.IAvatarService, Common.Services.AvatarService>();
        services.AddSingleton<Common.Services.IAvatarSvgValidator, Common.Services.AvatarSvgValidator>();
        
        // Regenerate Avatar Service is registered in AddUsersFeature

        // Authorization Filters registered in AddFilesFeature

        // Register default plugins
        services.AddCommentsFeature();
        services.AddScoped<Features.Reports.GetAllReports.Domain.IGetAllReportsService, Features.Reports.GetAllReports.Domain.GetAllReportsService>();
        services.AddTransient<Features.Reports.GetAllReports.Repository.IGetAllReportsRepository, Features.Reports.GetAllReports.Repository.GetAllReportsRepository>();
        services.AddScoped<Features.Reports.GetReport.Domain.IGetReportService, Features.Reports.GetReport.Domain.GetReportService>();
        services.AddTransient<Features.Reports.GetReport.Repository.IGetReportRepository, Features.Reports.GetReport.Repository.GetReportRepository>();
        services.AddScoped<Features.Reports.GetReportsAnalytics.Domain.IGetReportsAnalyticsService, Features.Reports.GetReportsAnalytics.Domain.GetReportsAnalyticsService>();
        services.AddTransient<Features.Reports.GetReportsAnalytics.Repository.IGetReportsAnalyticsRepository, Features.Reports.GetReportsAnalytics.Repository.GetReportsAnalyticsRepository>();
        services.AddScoped<Features.Reports.GetTopReportedModels.Domain.IGetTopReportedModelsService, Features.Reports.GetTopReportedModels.Domain.GetTopReportedModelsService>();
        services.AddTransient<Features.Reports.GetTopReportedModels.Repository.IGetTopReportedModelsRepository, Features.Reports.GetTopReportedModels.Repository.GetTopReportedModelsRepository>();
        services.AddScoped<Features.Reports.GetTopReportedUsers.Domain.IGetTopReportedUsersService, Features.Reports.GetTopReportedUsers.Domain.GetTopReportedUsersService>();
        services.AddTransient<Features.Reports.GetTopReportedUsers.Repository.IGetTopReportedUsersRepository, Features.Reports.GetTopReportedUsers.Repository.GetTopReportedUsersRepository>();
        services.AddScoped<Features.Reports.GetTopReportedComments.Domain.IGetTopReportedCommentsService, Features.Reports.GetTopReportedComments.Domain.GetTopReportedCommentsService>();
        services.AddTransient<Features.Reports.GetTopReportedComments.Repository.IGetTopReportedCommentsRepository, Features.Reports.GetTopReportedComments.Repository.GetTopReportedCommentsRepository>();
        services.AddScoped<Features.Reports.GetReportTrends.Domain.IGetReportTrendsService, Features.Reports.GetReportTrends.Domain.GetReportTrendsService>();
        services.AddTransient<Features.Reports.GetReportTrends.Repository.IGetReportTrendsRepository, Features.Reports.GetReportTrends.Repository.GetReportTrendsRepository>();
        services.AddScoped<Features.Reports.GetModeratorReportActivity.Domain.IGetModeratorReportActivityService, Features.Reports.GetModeratorReportActivity.Domain.GetModeratorReportActivityService>();
        services.AddTransient<Features.Reports.GetModeratorReportActivity.Repository.IGetModeratorReportActivityRepository, Features.Reports.GetModeratorReportActivity.Repository.GetModeratorReportActivityRepository>();
        services.AddScoped<Features.Reports.GetReportAnalytics.Domain.IGetReportAnalyticsService, Features.Reports.GetReportAnalytics.Domain.GetReportAnalyticsService>();
        services.AddTransient<Features.Reports.GetReportAnalytics.Repository.IGetReportAnalyticsRepository, Features.Reports.GetReportAnalytics.Repository.GetReportAnalyticsRepository>();
        services.AddScoped<Features.Reports.GetReportsForTarget.Domain.IGetReportsForTargetService, Features.Reports.GetReportsForTarget.Domain.GetReportsForTargetService>();
        services.AddTransient<Features.Reports.GetReportsForTarget.Repository.IGetReportsForTargetRepository, Features.Reports.GetReportsForTarget.Repository.GetReportsForTargetRepository>();
        services.AddScoped<Features.Reports.GetUnresolvedReports.Domain.IGetUnresolvedReportsService, Features.Reports.GetUnresolvedReports.Domain.GetUnresolvedReportsService>();
        services.AddTransient<Features.Reports.GetUnresolvedReports.Repository.IGetUnresolvedReportsRepository, Features.Reports.GetUnresolvedReports.Repository.GetUnresolvedReportsRepository>();
        services.AddScoped<Features.Reports.SubmitReport.Domain.ISubmitReportService, Features.Reports.SubmitReport.Domain.SubmitReportService>();
        services.AddTransient<Features.Reports.SubmitReport.Repository.ISubmitReportRepository, Features.Reports.SubmitReport.Repository.SubmitReportRepository>();
        services.AddScoped<Features.Reports.ResolveReport.Domain.IResolveReportService, Features.Reports.ResolveReport.Domain.ResolveReportService>();
        services.AddTransient<Features.Reports.ResolveReport.Repository.IResolveReportRepository, Features.Reports.ResolveReport.Repository.ResolveReportRepository>();
        services.AddScoped<IReportingPlugin, DefaultReportingPlugin>();
        
        // Theme System
        services.AddScoped<Common.Plugins.IThemePlugin, LiquidGlassThemePlugin>();
        services.AddScoped<Common.Services.IThemeManager, Common.Services.ThemeManager>();

        // Password Services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IPasswordGenerator, PasswordGenerator>();


        // Email Settings
        // Seeders
        services.AddTransient<PolyBucket.Api.Seeders.AdminSeeder>();
        services.AddTransient<PolyBucket.Api.Seeders.CategorySeeder>();
        services.AddTransient<PolyBucket.Api.Seeders.FileTypeSettingsSeeder>();
        services.AddTransient<PolyBucket.Api.Seeders.ModelSettingsSeeder>();
        

        // Categories
        services.AddScoped<Features.Categories.CreateCategory.Domain.ICreateCategoryService, Features.Categories.CreateCategory.Domain.CreateCategoryService>();
        services.AddTransient<Features.Categories.CreateCategory.Repository.ICreateCategoryRepository, Features.Categories.CreateCategory.Repository.CreateCategoryRepository>();
        services.AddScoped<Features.Categories.DeleteCategory.Domain.IDeleteCategoryService, Features.Categories.DeleteCategory.Domain.DeleteCategoryService>();
        services.AddTransient<Features.Categories.DeleteCategory.Repository.IDeleteCategoryRepository, Features.Categories.DeleteCategory.Repository.DeleteCategoryRepository>();
        services.AddScoped<Features.Categories.UpdateCategory.Domain.IUpdateCategoryService, Features.Categories.UpdateCategory.Domain.UpdateCategoryService>();
        services.AddTransient<Features.Categories.UpdateCategory.Repository.IUpdateCategoryRepository, Features.Categories.UpdateCategory.Repository.UpdateCategoryRepository>();
        services.AddScoped<Features.Categories.GetCategoryById.Domain.IGetCategoryByIdService, Features.Categories.GetCategoryById.Domain.GetCategoryByIdService>();
        services.AddTransient<Features.Categories.GetCategoryById.Repository.IGetCategoryByIdRepository, Features.Categories.GetCategoryById.Repository.GetCategoryByIdRepository>();
        services.AddScoped<Features.Categories.GetCategories.Domain.IGetCategoriesService, Features.Categories.GetCategories.Domain.GetCategoriesService>();
        services.AddTransient<Features.Categories.GetCategories.Repository.IGetCategoriesRepository, Features.Categories.GetCategories.Repository.GetCategoriesRepository>();

        // Federation
        services.AddFederationFeature();

        return services;
    }
} 