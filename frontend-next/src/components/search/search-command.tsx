"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import {
  Box,
  FolderOpen,
  Loader2,
  Search,
  User,
  ArrowRight,
  Download,
  Heart,
} from "lucide-react";
import {
  CommandDialog,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
  CommandSeparator,
} from "@/components/ui/command";
import { useDebouncedValue } from "@/lib/hooks/use-debounced-value";
import {
  quickSearch,
  type SearchResult,
  type SearchResults,
} from "@/lib/services/searchService";
import { UserAvatar } from "@/components/layout/user-avatar";
import { splitAvatarForDisplay } from "@/lib/avatar/minidenticon";
import { formatNumber } from "@/lib/utils/modelUtils";

interface SearchCommandProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function SearchCommand({ open, onOpenChange }: SearchCommandProps) {
  const router = useRouter();
  const [query, setQuery] = useState("");
  const [results, setResults] = useState<SearchResults | null>(null);
  const [loading, setLoading] = useState(false);
  const debouncedQuery = useDebouncedValue(query, 300);
  const abortRef = useRef<AbortController | null>(null);

  const resetState = useCallback(() => {
    setQuery("");
    setResults(null);
    setLoading(false);
    abortRef.current?.abort();
  }, []);

  useEffect(() => {
    if (!open) {
      resetState();
    }
  }, [open, resetState]);

  useEffect(() => {
    if (!debouncedQuery.trim()) {
      setResults(null);
      setLoading(false);
      return;
    }

    abortRef.current?.abort();
    const controller = new AbortController();
    abortRef.current = controller;

    setLoading(true);
    quickSearch(debouncedQuery)
      .then((data) => {
        if (!controller.signal.aborted) {
          setResults(data);
        }
      })
      .catch(() => {
        if (!controller.signal.aborted) {
          setResults(null);
        }
      })
      .finally(() => {
        if (!controller.signal.aborted) {
          setLoading(false);
        }
      });

    return () => controller.abort();
  }, [debouncedQuery]);

  const navigate = useCallback(
    (path: string) => {
      onOpenChange(false);
      router.push(path);
    },
    [onOpenChange, router],
  );

  const handleSelect = useCallback(
    (result: SearchResult) => {
      switch (result.kind) {
        case "model":
          navigate(`/models/${result.id}`);
          break;
        case "user":
          navigate(`/profile/${result.username ?? result.id}`);
          break;
        case "collection":
          navigate(`/collections/${result.id}`);
          break;
      }
    },
    [navigate],
  );

  const handleViewAll = useCallback(() => {
    if (query.trim()) {
      navigate(`/search?q=${encodeURIComponent(query.trim())}`);
    }
  }, [query, navigate]);

  const models = results?.results.filter((r) => r.kind === "model") ?? [];
  const users = results?.results.filter((r) => r.kind === "user") ?? [];
  const collections =
    results?.results.filter((r) => r.kind === "collection") ?? [];
  const hasResults = models.length > 0 || users.length > 0 || collections.length > 0;
  const hasQuery = debouncedQuery.trim().length > 0;

  return (
    <CommandDialog
      open={open}
      onOpenChange={onOpenChange}
      title="Search"
      description="Search for models, users, and collections"
      showCloseButton={false}
      className="glass-bg border-white/20 text-white sm:max-w-xl"
    >
      <CommandInput
        placeholder="Search models, users, collections..."
        value={query}
        onValueChange={setQuery}
        className="text-white placeholder:text-white/40"
      />
      <CommandList className="max-h-[min(60vh,400px)]">
        {loading && (
          <div className="flex items-center justify-center py-6">
            <Loader2 className="h-5 w-5 animate-spin text-white/40" />
          </div>
        )}

        {!loading && hasQuery && !hasResults && (
          <CommandEmpty className="text-white/60">
            No results found.
          </CommandEmpty>
        )}

        {!loading && !hasQuery && (
          <CommandEmpty className="text-white/40">
            Start typing to search...
          </CommandEmpty>
        )}

        {models.length > 0 && (
          <CommandGroup
            heading="Models"
            className="[&_[cmdk-group-heading]]:text-white/50"
          >
            {models.map((result) => (
              <CommandItem
                key={`model-${result.id}`}
                value={`model-${result.id}-${result.title}`}
                onSelect={() => handleSelect(result)}
                className="gap-3 text-white/80 data-[selected=true]:bg-white/10 data-[selected=true]:text-white"
              >
                {result.thumbnailUrl ? (
                  <img
                    src={result.thumbnailUrl}
                    alt=""
                    className="h-8 w-8 shrink-0 rounded object-cover"
                  />
                ) : (
                  <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded bg-white/10">
                    <Box className="h-4 w-4 text-white/40" />
                  </div>
                )}
                <div className="min-w-0 flex-1">
                  <div className="truncate text-sm font-medium">
                    {result.title}
                  </div>
                  {result.author && (
                    <div className="truncate text-xs text-white/40">
                      by {result.author}
                    </div>
                  )}
                </div>
                <div className="flex shrink-0 items-center gap-2 text-xs text-white/30">
                  {result.downloads != null && (
                    <span className="flex items-center gap-0.5">
                      <Download className="h-3 w-3" />
                      {formatNumber(result.downloads)}
                    </span>
                  )}
                  {result.likes != null && (
                    <span className="flex items-center gap-0.5">
                      <Heart className="h-3 w-3" />
                      {formatNumber(result.likes)}
                    </span>
                  )}
                </div>
              </CommandItem>
            ))}
          </CommandGroup>
        )}

        {users.length > 0 && (
          <>
            {models.length > 0 && (
              <CommandSeparator className="bg-white/10" />
            )}
            <CommandGroup
              heading="Users"
              className="[&_[cmdk-group-heading]]:text-white/50"
            >
              {users.map((result) => {
                const av = splitAvatarForDisplay(result.avatar);
                return (
                <CommandItem
                  key={`user-${result.id}`}
                  value={`user-${result.id}-${result.title}`}
                  onSelect={() => handleSelect(result)}
                  className="gap-3 text-white/80 data-[selected=true]:bg-white/10 data-[selected=true]:text-white"
                >
                  <UserAvatar
                    userId={result.id}
                    username={result.username ?? result.title}
                    profilePictureUrl={av.profilePictureUrl}
                    avatar={av.storedAvatar}
                    size="sm"
                    className="h-8 w-8"
                  />
                  <div className="min-w-0 flex-1">
                    <div className="truncate text-sm font-medium">
                      {result.username ?? result.title}
                    </div>
                  </div>
                  <User className="h-3.5 w-3.5 shrink-0 text-white/30" />
                </CommandItem>
                );
              })}
            </CommandGroup>
          </>
        )}

        {collections.length > 0 && (
          <>
            {(models.length > 0 || users.length > 0) && (
              <CommandSeparator className="bg-white/10" />
            )}
            <CommandGroup
              heading="Collections"
              className="[&_[cmdk-group-heading]]:text-white/50"
            >
              {collections.map((result) => (
                <CommandItem
                  key={`collection-${result.id}`}
                  value={`collection-${result.id}-${result.title}`}
                  onSelect={() => handleSelect(result)}
                  className="gap-3 text-white/80 data-[selected=true]:bg-white/10 data-[selected=true]:text-white"
                >
                  <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded bg-white/10">
                    <FolderOpen className="h-4 w-4 text-white/40" />
                  </div>
                  <div className="min-w-0 flex-1">
                    <div className="truncate text-sm font-medium">
                      {result.title}
                    </div>
                    {result.author && (
                      <div className="truncate text-xs text-white/40">
                        by {result.author}
                      </div>
                    )}
                  </div>
                  {result.modelCount != null && (
                    <span className="shrink-0 text-xs text-white/30">
                      {result.modelCount} model{result.modelCount !== 1 ? "s" : ""}
                    </span>
                  )}
                </CommandItem>
              ))}
            </CommandGroup>
          </>
        )}

        {hasQuery && hasResults && (
          <>
            <CommandSeparator className="bg-white/10" />
            <CommandGroup>
              <CommandItem
                value={`view-all-${query}`}
                onSelect={handleViewAll}
                className="justify-center gap-2 text-white/60 data-[selected=true]:bg-white/10 data-[selected=true]:text-white"
              >
                <Search className="h-3.5 w-3.5" />
                <span>View all results for &ldquo;{query.trim()}&rdquo;</span>
                <ArrowRight className="h-3.5 w-3.5" />
              </CommandItem>
            </CommandGroup>
          </>
        )}
      </CommandList>
    </CommandDialog>
  );
}

export function useSearchCommand() {
  const [open, setOpen] = useState(false);

  useEffect(() => {
    function onKeyDown(e: KeyboardEvent) {
      if (e.key === "k" && (e.metaKey || e.ctrlKey)) {
        e.preventDefault();
        setOpen((prev) => !prev);
      }
    }
    document.addEventListener("keydown", onKeyDown);
    return () => document.removeEventListener("keydown", onKeyDown);
  }, []);

  return { open, setOpen } as const;
}
