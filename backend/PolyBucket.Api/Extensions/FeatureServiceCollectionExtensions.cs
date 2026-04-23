using MediatR;
using PolyBucket.Api.Features.Users;
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
        services.AddTransient<Features.Models.GetModels.Repository.IGetModelsRepository, Features.Models.GetModels.Repository.GetModelsRepository>();
        services.AddTransient<Features.Models.GetModelById.Repository.IGetModelByIdRepository, Features.Models.GetModelById.Repository.GetModelByIdRepository>();
        services.AddTransient<Features.Models.GetModelById.Services.IGetModelByIdService, Features.Models.GetModelById.Services.GetModelByIdService>();
        
        // CreateModel
        services.AddScoped<Features.Models.CreateModel.Domain.CreateModelService>();
        services.AddTransient<Features.Models.CreateModel.Repository.ICreateModelRepository, Features.Models.CreateModel.Repository.CreateModelRepository>();
        
        // DeleteAllModels
        services.AddTransient<Features.Models.DeleteAllModels.Domain.IDeleteAllModelsService, Features.Models.DeleteAllModels.Domain.DeleteAllModelsService>();
        services.AddTransient<Features.Models.DeleteAllModels.Repository.IDeleteAllModelsUserRepository, Features.Models.DeleteAllModels.Repository.DeleteAllModelsUserRepository>();
        
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
        
        // FontAwesome Settings
        services.AddTransient<Features.SystemSettings.Domain.IFontAwesomeSettingsService, Features.SystemSettings.Services.FontAwesomeSettingsService>();
        services.AddTransient<Features.SystemSettings.Repository.IFontAwesomeSettingsRepository, Features.SystemSettings.Repository.FontAwesomeSettingsRepository>();
        
        // ACL Services
        services.AddTransient<IPermissionService, Features.ACL.Services.PermissionService>();
        services.AddTransient<Features.ACL.Services.IRoleManagementService, Features.ACL.Services.RoleManagementService>();
        services.AddTransient<Features.ACL.Repository.IRoleRepository, Features.ACL.Repository.RoleRepository>();
        
        // Admin Model Statistics
        services.AddTransient<Features.Admin.GetAdminModelStatistics.Domain.IGetAdminModelStatisticsService, Features.Admin.GetAdminModelStatistics.Domain.GetAdminModelStatisticsService>();
        services.AddTransient<Features.Admin.GetAdminModelStatistics.Repository.IGetAdminModelStatisticsRepository, Features.Admin.GetAdminModelStatistics.Repository.GetAdminModelStatisticsRepository>();
        
        // Avatar Service
        services.AddTransient<Common.Services.IAvatarService, Common.Services.AvatarService>();
        
        // Regenerate Avatar Service is registered in AddUsersFeature

        // Authorization Filters
        services.AddScoped<Features.Files.Http.PublicModelAuthorizationFilter>();

        // Plugin System
        services.AddSingleton<PluginManager>(provider =>
        {
            var pluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "plugins");
            return new PluginManager(pluginsPath);
        });

        // Marketplace Integration
        // services.AddHttpClient<Features.Plugins.Services.MarketplaceClient>();
        // services.AddTransient<Features.Plugins.Services.MarketplaceClient>();

        // Register default plugins
        services.AddScoped<Features.Comments.Domain.ICommentsPlugin, Features.Comments.Plugins.DefaultCommentsPlugin>();
        services.AddScoped<Features.Reports.GetAllReports.Domain.IGetAllReportsService, Features.Reports.GetAllReports.Domain.GetAllReportsService>();
        services.AddTransient<Features.Reports.GetAllReports.Repository.IGetAllReportsRepository, Features.Reports.GetAllReports.Repository.GetAllReportsRepository>();
        services.AddScoped<Features.Reports.GetReport.Domain.IGetReportService, Features.Reports.GetReport.Domain.GetReportService>();
        services.AddTransient<Features.Reports.GetReport.Repository.IGetReportRepository, Features.Reports.GetReport.Repository.GetReportRepository>();
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
        services.AddTransient<Features.SystemSettings.Domain.UpdateEmailSettingsCommandHandler>();
        services.AddTransient<Features.SystemSettings.Domain.TestEmailConfigurationCommandHandler>();
        
        // System Setup
        services.AddTransient<Features.SystemSettings.CheckFirstTimeSetup.Domain.CheckFirstTimeSetupQueryHandler>();
        services.AddTransient<Features.SystemSettings.UpdateSiteSettings.Domain.UpdateSiteSettingsCommandHandler>();
        services.AddTransient<Features.SystemSettings.CompleteFirstTimeSetup.Domain.CompleteFirstTimeSetupCommandHandler>();

        // Seeders
        services.AddTransient<PolyBucket.Api.Data.Seeders.AdminSeeder>();
        services.AddTransient<PolyBucket.Api.Seeders.CategorySeeder>();
        services.AddTransient<PolyBucket.Api.Seeders.FileTypeSettingsSeeder>();
        services.AddTransient<PolyBucket.Api.Seeders.ModelSettingsSeeder>();
        
        // Theme Management
        services.AddTransient<Features.ThemeManagement.Repository.IThemeRepository, Features.ThemeManagement.Repository.ThemeRepository>();
        
        // Categories
        services.AddScoped<Features.Categories.CreateCategory.Domain.ICreateCategoryService, Features.Categories.CreateCategory.Domain.CreateCategoryService>();
        services.AddTransient<Features.Categories.CreateCategory.Repository.ICreateCategoryRepository, Features.Categories.CreateCategory.Repository.CreateCategoryRepository>();
        services.AddScoped<Features.Categories.DeleteCategory.Domain.IDeleteCategoryService, Features.Categories.DeleteCategory.Domain.DeleteCategoryService>();
        services.AddTransient<Features.Categories.DeleteCategory.Repository.IDeleteCategoryRepository, Features.Categories.DeleteCategory.Repository.DeleteCategoryRepository>();
        services.AddScoped<Features.Categories.UpdateCategory.Domain.IUpdateCategoryService, Features.Categories.UpdateCategory.Domain.UpdateCategoryService>();
        services.AddTransient<Features.Categories.UpdateCategory.Repository.IUpdateCategoryRepository, Features.Categories.UpdateCategory.Repository.UpdateCategoryRepository>();
        services.AddScoped<Features.Categories.GetCategories.Domain.IGetCategoriesService, Features.Categories.GetCategories.Domain.GetCategoriesService>();
        services.AddTransient<Features.Categories.GetCategories.Repository.IGetCategoriesRepository, Features.Categories.GetCategories.Repository.GetCategoriesRepository>();

        // Search
        services.AddTransient<Features.Search.Repository.ISearchRepository, Features.Search.Repository.SearchRepository>();

        // Federation
        services.AddTransient<Features.Federation.Repository.IFederationRepository, Features.Federation.Repository.FederationRepository>();
        services.AddTransient<Features.Federation.Services.IFederationImportService, Features.Federation.Services.FederationImportService>();
        services.AddSingleton<Features.Federation.Services.IFederationTokenService, Features.Federation.Services.FederationTokenService>();

        return services;
    }
} 