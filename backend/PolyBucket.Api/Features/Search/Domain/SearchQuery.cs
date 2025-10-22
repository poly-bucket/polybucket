using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Search.Domain
{
    public class SearchQuery
    {
        public string Query { get; set; } = string.Empty;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public SearchType Type { get; set; } = SearchType.All;
        public string? Category { get; set; }
        public string? SortBy { get; set; } = "relevance";
        public bool SortDescending { get; set; } = false;
    }

    public enum SearchType
    {
        All,
        Models,
        Users,
        Collections
    }
}
