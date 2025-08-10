using MediatR;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.Reports.Plugins;
using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Common.Services;
using PolyBucket.Api.Features.SystemSettings.Plugins;
using PolyBucket.Api.Features.ACL.Services;

namespace PolyBucket.Api.Extensions;

public static class FeatureServiceCollectionExtensions
{
    public static IServiceCollection AddPolyBucketFeatures(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

        // Models
        services.AddTransient<Features.Models.Repository.IModelsRepository, Features.Models.Repository.ModelsRepository>();
        
        // CreateModel
        services.AddScoped<Features.Models.CreateModel.Domain.CreateModelService>();
        services.AddTransient<Features.Models.CreateModel.Repository.ICreateModelRepository, Features.Models.CreateModel.Repository.CreateModelRepository>();
        
        // CreateModelVersion
        services.AddTransient<Features.Models.CreateModelVersion.Domain.ICreateModelVersionService, Features.Models.CreateModelVersion.Domain.CreateModelVersionService>();
        services.AddTransient<Features.Models.CreateModelVersion.Repository.ICreateModelVersionRepository, Features.Models.CreateModelVersion.Repository.CreateModelVersionRepository>();
        
        // UpdateModel
        services.AddTransient<Features.Models.UpdateModel.Domain.IUpdateModelService, Features.Models.UpdateModel.Domain.UpdateModelService>();
        services.AddTransient<Features.Models.UpdateModel.Repository.IUpdateModelRepository, Features.Models.UpdateModel.Repository.UpdateModelRepository>();
        
        // UpdateModelVersion
        services.AddTransient<Features.Models.UpdateModelVersion.Domain.IUpdateModelVersionService, Features.Models.UpdateModelVersion.Domain.UpdateModelVersionService>();
        services.AddTransient<Features.Models.UpdateModelVersion.Repository.IUpdateModelVersionRepository, Features.Models.UpdateModelVersion.Repository.UpdateModelVersionRepository>();
        
        // DeleteModel
        services.AddTransient<Features.Models.DeleteModel.Domain.IDeleteModelService, Features.Models.DeleteModel.Domain.DeleteModelService>();
        services.AddTransient<Features.Models.DeleteModel.Repository.IDeleteModelRepository, Features.Models.DeleteModel.Repository.DeleteModelRepository>();
        
        // DeleteModelVersion
        services.AddTransient<Features.Models.DeleteModelVersion.Domain.IDeleteModelVersionService, Features.Models.DeleteModelVersion.Domain.DeleteModelVersionService>();
        services.AddTransient<Features.Models.DeleteModelVersion.Repository.IDeleteModelVersionRepository, Features.Models.DeleteModelVersion.Repository.DeleteModelVersionRepository>();
        
        // GetModelByUserId
        services.AddTransient<Features.Models.GetModelByUserId.Domain.IGetModelByUserIdService, Features.Models.GetModelByUserId.Domain.GetModelByUserIdService>();
        services.AddTransient<Features.Models.GetModelByUserId.Repository.IGetModelByUserIdRepository, Features.Models.GetModelByUserId.Repository.GetModelByUserIdRepository>();
        
        // Model Previews
        services.AddTransient<Features.Models.GetModelPreview.Repository.IModelPreviewRepository, Features.Models.GetModelPreview.Repository.ModelPreviewRepository>();
        services.AddTransient<Features.Models.GenerateModelPreview.Repository.IGenerateModelPreviewRepository, Features.Models.GenerateModelPreview.Repository.GenerateModelPreviewRepository>();
        services.AddTransient<Features.Models.GenerateModelPreview.Services.IModelPreviewGenerationService, Features.Models.GenerateModelPreview.Services.ModelPreviewGenerationService>();

        // Collections
        services.AddTransient<Features.Collections.CreateCollection.Repository.ICollectionRepository, Features.Collections.CreateCollection.Repository.CollectionRepository>();
        services.AddTransient<Features.Collections.GetCollectionById.Repository.ICollectionRepository, Features.Collections.GetCollectionById.Repository.CollectionRepository>();
        services.AddTransient<Features.Collections.UpdateCollection.Repository.ICollectionRepository, Features.Collections.UpdateCollection.Repository.CollectionRepository>();
        services.AddTransient<Features.Collections.DeleteCollection.Repository.ICollectionRepository, Features.Collections.DeleteCollection.Repository.CollectionRepository>();
        services.AddTransient<Features.Collections.GetUserCollections.Repository.ICollectionRepository, Features.Collections.GetUserCollections.Repository.CollectionRepository>();
        services.AddTransient<Features.Collections.AddModelToCollection.Repository.ICollectionRepository, Features.Collections.AddModelToCollection.Repository.CollectionRepository>();
        services.AddTransient<Features.Collections.RemoveModelFromCollection.Repository.ICollectionRepository, Features.Collections.RemoveModelFromCollection.Repository.CollectionRepository>();
        services.AddTransient<Features.Collections.AccessCollection.Repository.ICollectionRepository, Features.Collections.AccessCollection.Repository.CollectionRepository>();
        
        // Collection Command Handlers
        services.AddTransient<Features.Collections.CreateCollection.Domain.CreateCollectionCommandHandler>();
        services.AddTransient<Features.Collections.UpdateCollection.Domain.UpdateCollectionCommandHandler>();
        services.AddTransient<Features.Collections.DeleteCollection.Domain.DeleteCollectionCommandHandler>();
        services.AddTransient<Features.Collections.GetCollectionById.Domain.GetCollectionByIdQueryHandler>();
        services.AddTransient<Features.Collections.GetUserCollections.Domain.GetUserCollectionsQueryHandler>();
        services.AddTransient<Features.Collections.AddModelToCollection.Domain.AddModelToCollectionCommandHandler>();
        services.AddTransient<Features.Collections.RemoveModelFromCollection.Domain.RemoveModelFromCollectionCommandHandler>();
        services.AddTransient<Features.Collections.AccessCollection.Domain.AccessCollectionCommandHandler>();

        // Users
        services.AddTransient<Features.Users.Repository.IUserRepository, Features.Users.Repository.UserRepository>();
        
        // Authentication
        services.AddTransient<Features.Authentication.Repository.IAuthenticationRepository, Features.Authentication.Repository.AuthenticationRepository>();
        services.AddTransient<Features.Authentication.Services.ITokenService, Features.Authentication.Services.TokenService>();
        services.AddTransient<Features.Authentication.Services.IEmailService, Features.Authentication.Services.EmailService>();
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
        services.AddTransient<Features.Authentication.Register.Domain.RegisterCommandHandler>();
        services.AddTransient<Features.Authentication.Login.Domain.LoginCommandHandler>();
        services.AddTransient<Features.Authentication.ChangePassword.Domain.ChangePasswordCommandHandler>();
        services.AddTransient<Features.Authentication.TwoFactorAuth.InitializeTwoFactorAuth.Domain.InitializeTwoFactorAuthCommandHandler>();
        services.AddTransient<Features.Authentication.TwoFactorAuth.EnableTwoFactorAuth.Domain.EnableTwoFactorAuthCommandHandler>();
        services.AddTransient<Features.Authentication.TwoFactorAuth.DisableTwoFactorAuth.Domain.DisableTwoFactorAuthCommandHandler>();
        services.AddTransient<Features.Authentication.TwoFactorAuth.GetTwoFactorAuthStatus.Domain.GetTwoFactorAuthStatusQueryHandler>();
        services.AddTransient<Features.Authentication.RefreshToken.Domain.RefreshTokenCommandHandler>();
        
        // System Settings
        services.AddTransient<Features.SystemSettings.Services.IAuthenticationSettingsService, Features.SystemSettings.Services.AuthenticationSettingsService>();
        services.AddTransient<Features.SystemSettings.Services.ITokenSettingsService, Features.SystemSettings.Services.TokenSettingsService>();
        
        // ACL Services
        services.AddTransient<IPermissionService, Features.ACL.Services.PermissionService>();
        
        // Avatar Service
        services.AddTransient<Common.Services.IAvatarService, Common.Services.AvatarService>();
        
        // Regenerate Avatar Service
        services.AddTransient<Features.Users.RegenerateAvatar.Repository.IRegenerateAvatarRepository, Features.Users.RegenerateAvatar.Repository.RegenerateAvatarRepository>();
        services.AddTransient<Features.Users.RegenerateAvatar.Domain.IRegenerateAvatarService, Features.Users.RegenerateAvatar.Domain.RegenerateAvatarService>();
        
        // User Settings Services
        services.AddTransient<Features.Users.UpdateUserSettings.Handlers.UpdateUserSettingsCommandHandler>();
        services.AddTransient<Features.Users.Queries.GetUserSettingsQueryHandler>();

        // Authorization Filters
        services.AddScoped<Features.Files.Http.PublicModelAuthorizationFilter>();

        // Plugin System
        services.AddSingleton<PluginManager>(provider =>
        {
            var pluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "plugins");
            return new PluginManager(pluginsPath);
        });

        // Register default plugins
        services.AddScoped<Features.Comments.Domain.ICommentsPlugin, Features.Comments.Plugins.DefaultCommentsPlugin>();
        services.AddScoped<IReportingPlugin, DefaultReportingPlugin>();
        services.AddTransient<Features.Reports.Repository.IReportsRepository, Features.Reports.Repository.ReportsRepository>();
        
        // Theme System
        services.AddScoped<Common.Plugins.IThemePlugin, LiquidGlassThemePlugin>();
        services.AddScoped<Common.Services.IThemeManager, Common.Services.ThemeManager>();

        // Password Services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IPasswordGenerator, PasswordGenerator>();
        services.AddTransient<Features.Users.CreateUser.Domain.CreateUserCommandHandler>();

        // Email Settings
        services.AddTransient<Features.SystemSettings.Domain.UpdateEmailSettingsCommandHandler>();
        services.AddTransient<Features.SystemSettings.Domain.TestEmailConfigurationCommandHandler>();
        
        // System Setup
        services.AddTransient<Features.SystemSettings.CheckFirstTimeSetup.Domain.CheckFirstTimeSetupQueryHandler>();
        services.AddTransient<Features.SystemSettings.UpdateSiteSettings.Domain.UpdateSiteSettingsCommandHandler>();
        services.AddTransient<Features.SystemSettings.CompleteFirstTimeSetup.Domain.CompleteFirstTimeSetupCommandHandler>();

        // Seeders
        services.AddTransient<PolyBucket.Api.Data.Seeders.AdminSeeder>();

        return services;
    }
} 