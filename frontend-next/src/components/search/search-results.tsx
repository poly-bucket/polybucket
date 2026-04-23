"use client";

import Link from "next/link";
import { Box, Download, FolderOpen, Heart, Users } from "lucide-react";
import { Card } from "@/components/primitives/card";
import { ModelCard, ModelCardSkeleton } from "@/components/models/model-card";
import { UserAvatar } from "@/components/layout/user-avatar";
import { splitAvatarForDisplay } from "@/lib/avatar/minidenticon";
import type { SearchResult } from "@/lib/services/searchService";
import type { Model } from "@/lib/api/client";
import { formatNumber } from "@/lib/utils/modelUtils";
import { formatDate } from "@/lib/utils/format";

interface SearchResultsProps {
  results: SearchResult[];
  loading?: boolean;
}

function toModel(result: SearchResult): Model {
  return {
    id: result.id,
    name: result.title,
    description: result.description ?? "",
    thumbnailUrl: result.thumbnailUrl,
    author: result.author ? { username: result.author } : undefined,
    authorId: result.authorId ?? "",
    downloads: result.downloads ?? 0,
    likes: result.likes ?? 0,
    createdAt: result.createdAt,
  } as Model;
}

function UserResultCard({ result }: { result: SearchResult }) {
  const av = splitAvatarForDisplay(result.avatar);
  return (
    <Link href={`/profile/${result.username ?? result.id}`}>
      <Card
        variant="glass"
        className="flex cursor-pointer flex-row items-center gap-4 border-white/20 px-4 py-4 transition-all duration-200 hover:scale-[1.01] hover:bg-white/10"
      >
        <UserAvatar
          userId={result.id}
          username={result.username ?? result.title}
          profilePictureUrl={av.profilePictureUrl}
          avatar={av.storedAvatar}
          size="lg"
        />
        <div className="min-w-0 flex-1">
          <h3 className="truncate text-sm font-semibold text-white">
            {result.username ?? result.title}
          </h3>
          {result.description && (
            <p className="mt-0.5 line-clamp-1 text-xs text-white/60">
              {result.description}
            </p>
          )}
        </div>
        {result.createdAt && (
          <span className="shrink-0 text-xs text-white/40">
            Joined {formatDate(result.createdAt)}
          </span>
        )}
      </Card>
    </Link>
  );
}

function CollectionResultCard({ result }: { result: SearchResult }) {
  return (
    <Link href={`/collections/${result.id}`}>
      <Card
        variant="glass"
        className="flex cursor-pointer flex-row items-center gap-4 border-white/20 px-4 py-4 transition-all duration-200 hover:scale-[1.01] hover:bg-white/10"
      >
        {result.thumbnailUrl ? (
          <img
            src={result.thumbnailUrl}
            alt=""
            className="h-12 w-12 shrink-0 rounded-lg object-cover"
          />
        ) : (
          <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-lg bg-white/10">
            <FolderOpen className="h-5 w-5 text-white/40" />
          </div>
        )}
        <div className="min-w-0 flex-1">
          <h3 className="truncate text-sm font-semibold text-white">
            {result.title}
          </h3>
          <div className="mt-0.5 flex items-center gap-3 text-xs text-white/50">
            {result.author && <span>by {result.author}</span>}
            {result.modelCount != null && (
              <span>
                {result.modelCount} model{result.modelCount !== 1 ? "s" : ""}
              </span>
            )}
          </div>
        </div>
        {result.createdAt && (
          <span className="shrink-0 text-xs text-white/40">
            {formatDate(result.createdAt)}
          </span>
        )}
      </Card>
    </Link>
  );
}

export function SearchResults({ results, loading }: SearchResultsProps) {
  const models = results.filter((r) => r.kind === "model");
  const users = results.filter((r) => r.kind === "user");
  const collections = results.filter((r) => r.kind === "collection");

  if (loading) {
    return (
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {Array.from({ length: 8 }).map((_, i) => (
          <ModelCardSkeleton key={i} />
        ))}
      </div>
    );
  }

  if (results.length === 0) {
    return (
      <div className="rounded-xl border border-white/20 bg-white/5 px-6 py-16 text-center">
        <Box className="mx-auto mb-3 h-10 w-10 text-white/30" />
        <p className="text-white/60">No results found.</p>
        <p className="mt-1 text-sm text-white/40">
          Try adjusting your search query or filters.
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      {models.length > 0 && (
        <section>
          <h3 className="mb-4 flex items-center gap-2 text-sm font-medium text-white/70">
            <Box className="h-4 w-4" />
            Models
            <span className="text-white/40">({models.length})</span>
          </h3>
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
            {models.map((result) => (
              <ModelCard key={result.id} model={toModel(result)} />
            ))}
          </div>
        </section>
      )}

      {users.length > 0 && (
        <section>
          <h3 className="mb-4 flex items-center gap-2 text-sm font-medium text-white/70">
            <Users className="h-4 w-4" />
            Users
            <span className="text-white/40">({users.length})</span>
          </h3>
          <div className="grid gap-2 sm:grid-cols-2 lg:grid-cols-3">
            {users.map((result) => (
              <UserResultCard key={result.id} result={result} />
            ))}
          </div>
        </section>
      )}

      {collections.length > 0 && (
        <section>
          <h3 className="mb-4 flex items-center gap-2 text-sm font-medium text-white/70">
            <FolderOpen className="h-4 w-4" />
            Collections
            <span className="text-white/40">({collections.length})</span>
          </h3>
          <div className="grid gap-2 sm:grid-cols-2 lg:grid-cols-3">
            {collections.map((result) => (
              <CollectionResultCard key={result.id} result={result} />
            ))}
          </div>
        </section>
      )}
    </div>
  );
}
