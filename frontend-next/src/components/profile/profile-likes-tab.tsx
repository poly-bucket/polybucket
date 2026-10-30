"use client";

import { useCallback, useEffect, useState } from "react";
import { ModelCard, ModelCardSkeleton } from "@/components/models/model-card";
import { SimplePagination } from "@/components/collections/simple-pagination";
import { fetchUserLikedModels } from "@/lib/services/userLikedModelsService";
import type { Model } from "@/lib/api/client";
import type { UserModelListItemDto } from "@/lib/api/client";

const PAGE_SIZE = 12;

interface ProfileLikesTabProps {
  username: string;
  page: number;
  onPageChange: (page: number) => void;
}

function mapToModel(userModel: UserModelListItemDto, authorUsername: string): Model {
  return {
    ...userModel,
    author: { username: authorUsername },
    comments: [],
  } as unknown as Model;
}

export function ProfileLikesTab({ username, page, onPageChange }: ProfileLikesTabProps) {
  const [models, setModels] = useState<UserModelListItemDto[]>([]);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await fetchUserLikedModels(username, page, PAGE_SIZE);
      setModels(response.models);
      setTotalPages(response.totalPages);
    } catch {
      setError("Failed to load liked models.");
      setModels([]);
    } finally {
      setLoading(false);
    }
  }, [username, page]);

  useEffect(() => {
    load();
  }, [load]);

  if (loading) {
    return (
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {Array.from({ length: PAGE_SIZE }).map((_, i) => (
          <ModelCardSkeleton key={i} />
        ))}
      </div>
    );
  }

  if (models.length === 0) {
    return (
      <div className="rounded-xl border border-white/20 bg-white/5 px-8 py-12 text-center">
        <p className="text-white/60">{error ?? "No liked models yet"}</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {models.map((model) => (
          <ModelCard
            key={model.id}
            model={mapToModel(model, model.author?.username ?? "unknown")}
          />
        ))}
      </div>
      {totalPages > 1 && (
        <SimplePagination page={page} totalPages={totalPages} onPageChange={onPageChange} />
      )}
    </div>
  );
}
