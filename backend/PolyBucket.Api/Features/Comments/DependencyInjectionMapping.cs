using Microsoft.Extensions.DependencyInjection;

namespace PolyBucket.Api.Features.Comments;

public static class CommentsDependencyInjectionMapping
{
    public static IServiceCollection AddCommentsFeature(this IServiceCollection services)
    {
        services.AddScoped<Plugins.IEnhancedCommentsPlugin, Plugins.EnhancedCommentsPlugin>();

        services.AddTransient<LikeComment.Repository.CommentReactionPersistence>();
        services.AddTransient<LikeComment.Repository.ICommentReactionPersistence>(sp =>
            sp.GetRequiredService<LikeComment.Repository.CommentReactionPersistence>());
        services.AddScoped<LikeComment.Domain.ICommentReactionOperations, LikeComment.Domain.CommentReactionOperations>();
        services.AddScoped<GetCommentsForTarget.Domain.ICommentResponseMapper, GetCommentsForTarget.Domain.CommentResponseMapper>();

        Register<CreateComment.Domain.ICreateCommentService, CreateComment.Domain.CreateCommentService,
            CreateComment.Repository.ICreateCommentRepository, CreateComment.Repository.CreateCommentRepository>(services);
        Register<UpdateComment.Domain.IUpdateCommentService, UpdateComment.Domain.UpdateCommentService,
            UpdateComment.Repository.IUpdateCommentRepository, UpdateComment.Repository.UpdateCommentRepository>(services);
        Register<DeleteComment.Domain.IDeleteCommentService, DeleteComment.Domain.DeleteCommentService,
            DeleteComment.Repository.IDeleteCommentRepository, DeleteComment.Repository.DeleteCommentRepository>(services);
        Register<GetComment.Domain.IGetCommentService, GetComment.Domain.GetCommentService,
            GetComment.Repository.IGetCommentRepository, GetComment.Repository.GetCommentRepository>(services);
        Register<GetCommentsForTarget.Domain.IGetCommentsForTargetService, GetCommentsForTarget.Domain.GetCommentsForTargetService,
            GetCommentsForTarget.Repository.IGetCommentsForTargetRepository, GetCommentsForTarget.Repository.GetCommentsForTargetRepository>(services);
        Register<GetCommentStatistics.Domain.IGetCommentStatisticsService, GetCommentStatistics.Domain.GetCommentStatisticsService,
            GetCommentStatistics.Repository.IGetCommentStatisticsRepository, GetCommentStatistics.Repository.GetCommentStatisticsRepository>(services);
        Register<GetModeratedComments.Domain.IGetModeratedCommentsService, GetModeratedComments.Domain.GetModeratedCommentsService,
            GetModeratedComments.Repository.IGetModeratedCommentsRepository, GetModeratedComments.Repository.GetModeratedCommentsRepository>(services);
        Register<GetUserCommentStatistics.Domain.IGetUserCommentStatisticsService, GetUserCommentStatistics.Domain.GetUserCommentStatisticsService,
            GetUserCommentStatistics.Repository.IGetUserCommentStatisticsRepository, GetUserCommentStatistics.Repository.GetUserCommentStatisticsRepository>(services);
        Register<LikeComment.Domain.ILikeCommentService, LikeComment.Domain.LikeCommentService,
            LikeComment.Repository.ILikeCommentRepository, LikeComment.Repository.LikeCommentRepository>(services);
        Register<DislikeComment.Domain.IDislikeCommentService, DislikeComment.Domain.DislikeCommentService,
            DislikeComment.Repository.IDislikeCommentRepository, DislikeComment.Repository.DislikeCommentRepository>(services);
        Register<RemoveCommentLike.Domain.IRemoveCommentLikeService, RemoveCommentLike.Domain.RemoveCommentLikeService,
            RemoveCommentLike.Repository.IRemoveCommentLikeRepository, RemoveCommentLike.Repository.RemoveCommentLikeRepository>(services);
        Register<RemoveCommentDislike.Domain.IRemoveCommentDislikeService, RemoveCommentDislike.Domain.RemoveCommentDislikeService,
            RemoveCommentDislike.Repository.IRemoveCommentDislikeRepository, RemoveCommentDislike.Repository.RemoveCommentDislikeRepository>(services);
        Register<ModerateComment.Domain.IModerateCommentService, ModerateComment.Domain.ModerateCommentService,
            ModerateComment.Repository.IModerateCommentRepository, ModerateComment.Repository.ModerateCommentRepository>(services);
        Register<ModerateAllUserComments.Domain.IModerateAllUserCommentsService, ModerateAllUserComments.Domain.ModerateAllUserCommentsService,
            ModerateAllUserComments.Repository.IModerateAllUserCommentsRepository, ModerateAllUserComments.Repository.ModerateAllUserCommentsRepository>(services);
        Register<UnmoderateComment.Domain.IUnmoderateCommentService, UnmoderateComment.Domain.UnmoderateCommentService,
            UnmoderateComment.Repository.IUnmoderateCommentRepository, UnmoderateComment.Repository.UnmoderateCommentRepository>(services);
        Register<ReportComment.Domain.IReportCommentService, ReportComment.Domain.ReportCommentService,
            ReportComment.Repository.IReportCommentRepository, ReportComment.Repository.ReportCommentRepository>(services);
        Register<DeleteAllCommentsForTarget.Domain.IDeleteAllCommentsForTargetService, DeleteAllCommentsForTarget.Domain.DeleteAllCommentsForTargetService,
            DeleteAllCommentsForTarget.Repository.IDeleteAllCommentsForTargetRepository, DeleteAllCommentsForTarget.Repository.DeleteAllCommentsForTargetRepository>(services);

        return services;
    }

    private static void Register<TService, TServiceImpl, TRepository, TRepositoryImpl>(IServiceCollection services)
        where TService : class
        where TServiceImpl : class, TService
        where TRepository : class
        where TRepositoryImpl : class, TRepository
    {
        services.AddScoped<TService, TServiceImpl>();
        services.AddTransient<TRepository, TRepositoryImpl>();
    }
}
