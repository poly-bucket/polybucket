using Microsoft.Extensions.DependencyInjection;

namespace PolyBucket.Api.Features.Users;

public static class UsersDependencyInjectionMapping
{
    public static IServiceCollection AddUsersFeature(this IServiceCollection services)
    {
        services.AddScoped<CreateUser.Domain.ICreateUserService, CreateUser.Domain.CreateUserService>();
        services.AddTransient<CreateUser.Repository.ICreateUserRepository, CreateUser.Repository.CreateUserRepository>();

        services.AddScoped<GetUsers.Domain.IGetUsersService, GetUsers.Domain.GetUsersService>();
        services.AddTransient<GetUsers.Repository.IGetUsersRepository, GetUsers.Repository.GetUsersRepository>();

        services.AddScoped<GetUserById.Domain.IGetUserByIdService, GetUserById.Domain.GetUserByIdService>();
        services.AddTransient<GetUserById.Repository.IGetUserByIdRepository, GetUserById.Repository.GetUserByIdRepository>();

        services.AddScoped<GetUserSettings.Domain.IGetUserSettingsService, GetUserSettings.Domain.GetUserSettingsService>();
        services.AddTransient<GetUserSettings.Repository.IGetUserSettingsRepository, GetUserSettings.Repository.GetUserSettingsRepository>();

        services.AddScoped<GetUserProfile.Domain.IGetUserProfileService, GetUserProfile.Domain.GetUserProfileService>();
        services.AddTransient<GetUserProfile.Repository.IGetUserProfileRepository, GetUserProfile.Repository.GetUserProfileRepository>();

        services.AddScoped<GetUserModels.Domain.IGetUserModelsService, GetUserModels.Domain.GetUserModelsService>();
        services.AddTransient<GetUserModels.Repository.IGetUserModelsRepository, GetUserModels.Repository.GetUserModelsRepository>();

        services.AddScoped<GetUserPrinters.Domain.IGetUserPrintersService, GetUserPrinters.Domain.GetUserPrintersService>();
        services.AddTransient<GetUserPrinters.Repository.IGetUserPrintersRepository, GetUserPrinters.Repository.GetUserPrintersRepository>();

        services.AddScoped<GetUserLikedModels.Domain.IGetUserLikedModelsService, GetUserLikedModels.Domain.GetUserLikedModelsService>();
        services.AddTransient<GetUserLikedModels.Repository.IGetUserLikedModelsRepository, GetUserLikedModels.Repository.GetUserLikedModelsRepository>();

        services.AddScoped<GetUserComments.Domain.IGetUserCommentsService, GetUserComments.Domain.GetUserCommentsService>();
        services.AddTransient<GetUserComments.Repository.IGetUserCommentsRepository, GetUserComments.Repository.GetUserCommentsRepository>();

        services.AddScoped<GetPublicUserCollections.Domain.IGetPublicUserCollectionsService, GetPublicUserCollections.Domain.GetPublicUserCollectionsService>();
        services.AddTransient<GetPublicUserCollections.Repository.IGetPublicUserCollectionsRepository, GetPublicUserCollections.Repository.GetPublicUserCollectionsRepository>();

        services.AddScoped<UpdateUserProfile.Domain.IUpdateUserProfileService, UpdateUserProfile.Domain.UpdateUserProfileService>();
        services.AddTransient<UpdateUserProfile.Repository.IUpdateUserProfileRepository, UpdateUserProfile.Repository.UpdateUserProfileRepository>();

        services.AddScoped<UpdateUserSettings.Domain.IUpdateUserSettingsService, UpdateUserSettings.Domain.UpdateUserSettingsService>();
        services.AddTransient<UpdateUserSettings.Repository.IUpdateUserSettingsRepository, UpdateUserSettings.Repository.UpdateUserSettingsRepository>();

        services.AddScoped<RegenerateAvatar.Domain.IRegenerateAvatarService, RegenerateAvatar.Domain.RegenerateAvatarService>();
        services.AddTransient<RegenerateAvatar.Repository.IRegenerateAvatarRepository, RegenerateAvatar.Repository.RegenerateAvatarRepository>();

        services.AddScoped<BanUser.Domain.IBanUserService, BanUser.Domain.BanUserService>();
        services.AddTransient<BanUser.Repository.IBanUserRepository, BanUser.Repository.BanUserRepository>();

        services.AddScoped<UnbanUser.Domain.IUnbanUserService, UnbanUser.Domain.UnbanUserService>();
        services.AddTransient<UnbanUser.Repository.IUnbanUserRepository, UnbanUser.Repository.UnbanUserRepository>();

        services.AddScoped<GetBannedUsers.Domain.IGetBannedUsersService, GetBannedUsers.Domain.GetBannedUsersService>();
        services.AddTransient<GetBannedUsers.Repository.IGetBannedUsersRepository, GetBannedUsers.Repository.GetBannedUsersRepository>();

        return services;
    }
}
