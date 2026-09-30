using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Search.Domain;

namespace PolyBucket.Api.Features.Search.Repository;

public sealed class ScoredEntity<T>
{
    public T Entity { get; init; } = default!;
    public double Score { get; init; }
}

public static class SearchQueryBuilder
{
    private const string EscapeCharacter = "\\";

    public static string NormalizeTerm(string query) => query.Trim().ToLowerInvariant();

    public static string ContainsPattern(string term) => $"%{EscapeLikePattern(term)}%";

    public static string PrefixPattern(string term) => $"{EscapeLikePattern(term)}%";

    public static string EscapeLikePattern(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

    public static IQueryable<ScoredEntity<Model>> ScoreModels(IQueryable<Model> models, string term, SearchTextMode mode)
    {
        if (mode == SearchTextMode.Trigram)
        {
            var contains = ContainsPattern(term);
            var prefix = PrefixPattern(term);
            return models
                .Where(m =>
                    EF.Functions.ILike(m.Name, contains, EscapeCharacter)
                    || EF.Functions.TrigramsAreWordSimilar(term, m.Name)
                    || (m.Description != null && EF.Functions.ILike(m.Description, contains, EscapeCharacter))
                    || m.Tags.Any(t => EF.Functions.ILike(t.Name, contains, EscapeCharacter))
                    || m.Categories.Any(c => EF.Functions.ILike(c.Name, contains, EscapeCharacter))
                    || EF.Functions.ILike(m.Author.Username, contains, EscapeCharacter))
                .Select(m => new ScoredEntity<Model>
                {
                    Entity = m,
                    Score = EF.Functions.TrigramsWordSimilarity(term, m.Name) * 100.0
                        + (m.Name.ToLower() == term ? 50.0 : EF.Functions.ILike(m.Name, prefix, EscapeCharacter) ? 25.0 : 0.0)
                        + (m.Description != null && EF.Functions.ILike(m.Description, contains, EscapeCharacter) ? 15.0 : 0.0)
                });
        }

        return models
            .Where(m =>
                m.Name.ToLower().Contains(term)
                || (m.Description != null && m.Description.ToLower().Contains(term))
                || m.Tags.Any(t => t.Name.ToLower().Contains(term))
                || m.Categories.Any(c => c.Name.ToLower().Contains(term))
                || m.Author.Username.ToLower().Contains(term))
            .Select(m => new ScoredEntity<Model>
            {
                Entity = m,
                Score = (m.Name.ToLower() == term ? 100.0 : m.Name.ToLower().StartsWith(term) ? 80.0 : m.Name.ToLower().Contains(term) ? 60.0 : 0.0)
                    + (m.Description != null && m.Description.ToLower().Contains(term) ? 30.0 : 0.0)
            });
    }

    public static IQueryable<ScoredEntity<User>> ScoreUsers(IQueryable<User> users, string term, SearchTextMode mode)
    {
        if (mode == SearchTextMode.Trigram)
        {
            var contains = ContainsPattern(term);
            var prefix = PrefixPattern(term);
            return users
                .Where(u =>
                    EF.Functions.ILike(u.Username, contains, EscapeCharacter)
                    || EF.Functions.TrigramsAreWordSimilar(term, u.Username)
                    || (u.FirstName != null && EF.Functions.ILike(u.FirstName, contains, EscapeCharacter))
                    || (u.LastName != null && EF.Functions.ILike(u.LastName, contains, EscapeCharacter))
                    || (u.Bio != null && EF.Functions.ILike(u.Bio, contains, EscapeCharacter)))
                .Select(u => new ScoredEntity<User>
                {
                    Entity = u,
                    Score = EF.Functions.TrigramsWordSimilarity(term, u.Username) * 100.0
                        + (u.Username.ToLower() == term ? 50.0 : EF.Functions.ILike(u.Username, prefix, EscapeCharacter) ? 25.0 : 0.0)
                        + (u.Bio != null && EF.Functions.ILike(u.Bio, contains, EscapeCharacter) ? 15.0 : 0.0)
                });
        }

        return users
            .Where(u =>
                u.Username.ToLower().Contains(term)
                || (u.FirstName != null && u.FirstName.ToLower().Contains(term))
                || (u.LastName != null && u.LastName.ToLower().Contains(term))
                || (u.Bio != null && u.Bio.ToLower().Contains(term)))
            .Select(u => new ScoredEntity<User>
            {
                Entity = u,
                Score = (u.Username.ToLower() == term ? 100.0 : u.Username.ToLower().StartsWith(term) ? 80.0 : u.Username.ToLower().Contains(term) ? 60.0 : 0.0)
                    + (u.Bio != null && u.Bio.ToLower().Contains(term) ? 30.0 : 0.0)
            });
    }

    public static IQueryable<ScoredEntity<Collection>> ScoreCollections(IQueryable<Collection> collections, string term, SearchTextMode mode)
    {
        if (mode == SearchTextMode.Trigram)
        {
            var contains = ContainsPattern(term);
            var prefix = PrefixPattern(term);
            return collections
                .Where(c =>
                    EF.Functions.ILike(c.Name, contains, EscapeCharacter)
                    || EF.Functions.TrigramsAreWordSimilar(term, c.Name)
                    || (c.Description != null && EF.Functions.ILike(c.Description, contains, EscapeCharacter))
                    || EF.Functions.ILike(c.Owner.Username, contains, EscapeCharacter))
                .Select(c => new ScoredEntity<Collection>
                {
                    Entity = c,
                    Score = EF.Functions.TrigramsWordSimilarity(term, c.Name) * 100.0
                        + (c.Name.ToLower() == term ? 50.0 : EF.Functions.ILike(c.Name, prefix, EscapeCharacter) ? 25.0 : 0.0)
                        + (c.Description != null && EF.Functions.ILike(c.Description, contains, EscapeCharacter) ? 15.0 : 0.0)
                });
        }

        return collections
            .Where(c =>
                c.Name.ToLower().Contains(term)
                || (c.Description != null && c.Description.ToLower().Contains(term))
                || c.Owner.Username.ToLower().Contains(term))
            .Select(c => new ScoredEntity<Collection>
            {
                Entity = c,
                Score = (c.Name.ToLower() == term ? 100.0 : c.Name.ToLower().StartsWith(term) ? 80.0 : c.Name.ToLower().Contains(term) ? 60.0 : 0.0)
                    + (c.Description != null && c.Description.ToLower().Contains(term) ? 30.0 : 0.0)
            });
    }

    public static IOrderedQueryable<ScoredEntity<Model>> OrderModels(IQueryable<ScoredEntity<Model>> query, string? sortBy, bool descending)
    {
        IOrderedQueryable<ScoredEntity<Model>> ordered = NormalizeSort(sortBy) switch
        {
            "createdat" => descending ? query.OrderByDescending(s => s.Entity.CreatedAt) : query.OrderBy(s => s.Entity.CreatedAt),
            "downloads" => descending ? query.OrderByDescending(s => s.Entity.Downloads) : query.OrderBy(s => s.Entity.Downloads),
            "likes" => descending ? query.OrderByDescending(s => s.Entity.Likes) : query.OrderBy(s => s.Entity.Likes),
            _ => query.OrderByDescending(s => s.Score)
        };
        return ordered.ThenByDescending(s => s.Entity.CreatedAt).ThenBy(s => s.Entity.Id);
    }

    public static IOrderedQueryable<ScoredEntity<User>> OrderUsers(IQueryable<ScoredEntity<User>> query, string? sortBy, bool descending)
    {
        IOrderedQueryable<ScoredEntity<User>> ordered = NormalizeSort(sortBy) switch
        {
            "createdat" => descending ? query.OrderByDescending(s => s.Entity.CreatedAt) : query.OrderBy(s => s.Entity.CreatedAt),
            _ => query.OrderByDescending(s => s.Score)
        };
        return ordered.ThenByDescending(s => s.Entity.CreatedAt).ThenBy(s => s.Entity.Id);
    }

    public static IOrderedQueryable<ScoredEntity<Collection>> OrderCollections(IQueryable<ScoredEntity<Collection>> query, string? sortBy, bool descending)
    {
        IOrderedQueryable<ScoredEntity<Collection>> ordered = NormalizeSort(sortBy) switch
        {
            "createdat" => descending ? query.OrderByDescending(s => s.Entity.CreatedAt) : query.OrderBy(s => s.Entity.CreatedAt),
            _ => query.OrderByDescending(s => s.Score)
        };
        return ordered.ThenByDescending(s => s.Entity.CreatedAt).ThenBy(s => s.Entity.Id);
    }

    private static string NormalizeSort(string? sortBy) =>
        string.IsNullOrWhiteSpace(sortBy) ? "relevance" : sortBy.Trim().ToLowerInvariant();
}
