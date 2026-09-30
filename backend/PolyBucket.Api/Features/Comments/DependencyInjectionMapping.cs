using Microsoft.Extensions.DependencyInjection;

namespace PolyBucket.Api.Features.Comments;

public static class CommentsDependencyInjectionMapping
{
    public static IServiceCollection AddCommentsFeature(this IServiceCollection services)
    {
        services.AddScoped<Domain.ICommentsPlugin, Plugins.DefaultCommentsPlugin>();
        services.AddScoped<Domain.IEnhancedCommentsPlugin, Plugins.EnhancedCommentsPlugin>();
        services.AddTransient<Repository.ICommentReactionRepository, Repository.CommentReactionRepository>();
        services.AddScoped<Domain.ICommentReactionService, Domain.CommentReactionService>();
        services.AddScoped<Domain.ICommentResponseMapper, Domain.CommentResponseMapper>();

        return services;
    }
}
