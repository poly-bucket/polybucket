"use client";

import { useCallback, useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { Search, Box, User, FolderOpen, Layers } from "lucide-react";
import { Button } from "@/components/primitives/button";
import { SearchResults } from "@/components/search/search-results";
import {
  search,
  SearchType,
  type SearchResults as SearchResultsData,
} from "@/lib/services/searchService";

const TYPE_FILTERS: { value: SearchType; label: string; icon: typeof Box }[] = [
  { value: SearchType.All, label: "All", icon: Layers },
  { value: SearchType.Models, label: "Models", icon: Box },
  { value: SearchType.Users, label: "Users", icon: User },
  { value: SearchType.Collections, label: "Collections", icon: FolderOpen },
];

const SORT_OPTIONS = [
  { value: "relevance", label: "Relevance" },
  { value: "createdAt", label: "Date" },
  { value: "downloads", label: "Downloads" },
  { value: "likes", label: "Likes" },
];

const PAGE_SIZE = 20;

export default function SearchPage() {
  const router = useRouter();
  const searchParams = useSearchParams();

  const query = searchParams.get("q") ?? "";
  const type =
    (searchParams.get("type") as SearchType) ?? SearchType.All;
  const sortBy = searchParams.get("sortBy") ?? "relevance";
  const page = Math.max(1, parseInt(searchParams.get("page") ?? "1", 10) || 1);

  const [data, setData] = useState<SearchResultsData | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const updateParams = useCallback(
    (updates: Record<string, string>) => {
      const next = new URLSearchParams(searchParams.toString());
      for (const [key, value] of Object.entries(updates)) {
        next.set(key, value);
      }
      router.replace(`/search?${next.toString()}`, { scroll: false });
    },
    [searchParams, router],
  );

  useEffect(() => {
    if (!query.trim()) {
      setData(null);
      return;
    }

    setLoading(true);
    setError(null);

    search({
      query,
      page,
      pageSize: PAGE_SIZE,
      type: Object.values(SearchType).includes(type) ? type : SearchType.All,
      sortBy,
      sortDescending: sortBy !== "relevance",
    })
      .then(setData)
      .catch((err) => {
        setError(err instanceof Error ? err.message : "Search failed");
        setData(null);
      })
      .finally(() => setLoading(false));
  }, [query, page, type, sortBy]);

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <div className="mb-6 flex items-center gap-3">
        <Search className="h-6 w-6 text-white/50" />
        <h1 className="text-2xl font-semibold text-white">
          {query ? (
            <>
              Results for &ldquo;{query}&rdquo;
              {data && (
                <span className="ml-2 text-base font-normal text-white/50">
                  ({data.totalCount} found)
                </span>
              )}
            </>
          ) : (
            "Search"
          )}
        </h1>
      </div>

      <div className="mb-6 flex flex-wrap items-center gap-3">
        <div className="flex flex-wrap gap-1.5">
          {TYPE_FILTERS.map(({ value, label, icon: Icon }) => (
            <Button
              key={value}
              variant={type === value ? "glass" : "outline"}
              size="sm"
              onClick={() => updateParams({ type: value, page: "1" })}
              className={
                type === value
                  ? "border-white/30 bg-white/15 text-white"
                  : "border-white/15 text-white/60"
              }
            >
              <Icon className="mr-1.5 h-3.5 w-3.5" />
              {label}
            </Button>
          ))}
        </div>

        <div className="ml-auto flex items-center gap-2">
          <span className="text-xs text-white/40">Sort:</span>
          <select
            value={sortBy}
            onChange={(e) =>
              updateParams({ sortBy: e.target.value, page: "1" })
            }
            className="rounded-md border border-white/15 bg-white/5 px-2 py-1 text-xs text-white/80 outline-none focus:border-white/30"
          >
            {SORT_OPTIONS.map(({ value, label }) => (
              <option key={value} value={value} className="bg-gray-900">
                {label}
              </option>
            ))}
          </select>
        </div>
      </div>

      {error && (
        <div className="mb-6 rounded-xl border border-red-500/30 bg-red-500/10 px-4 py-3 text-sm text-red-300">
          {error}
        </div>
      )}

      <SearchResults
        results={data?.results ?? []}
        loading={loading}
      />

      {data && data.totalPages > 1 && (
        <div className="mt-8 flex items-center justify-center gap-2">
          <Button
            variant="outline"
            size="sm"
            disabled={page <= 1}
            onClick={() => updateParams({ page: String(page - 1) })}
            className="border-white/15 text-white/60"
          >
            Previous
          </Button>
          <span className="px-3 text-sm text-white/50">
            Page {page} of {data.totalPages}
          </span>
          <Button
            variant="outline"
            size="sm"
            disabled={page >= data.totalPages}
            onClick={() => updateParams({ page: String(page + 1) })}
            className="border-white/15 text-white/60"
          >
            Next
          </Button>
        </div>
      )}
    </div>
  );
}
