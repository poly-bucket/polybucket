"use client";

import { useCallback, useEffect, useState } from "react";
import { ModelCard, ModelCardSkeleton } from "@/components/models/model-card";
import { ModelBrowseGrid } from "@/components/models/model-browse-grid";
import { useModelBrowseLayout } from "@/lib/layout/use-model-browse-layout";
import { SimplePagination } from "@/components/collections/simple-pagination";
import { fetchUserLikedModels } from "@/lib/services/userLikedModelsService";
import type { Model, UserLikedModelListItemDto } from "@/lib/api/client";

const PAGE_SIZE = 12;

interface ProfileLikesTabProps {
  username: string;
  page: number;
  onPageChange: (page: number) => void;
}

function mapToModel(userModel: UserLikedModelListItemDto, authorUsername: string): Model {
  return {
    ...userModel,
    author: { username: authorUsername },
    comments: [],
  } as unknown as Model;
}

export function ProfileLikesTab({ username, page, onPageChange }: ProfileLikesTabProps) {
  const layout = useModelBrowseLayout();
  const [models, setModels] = useState<UserLikedModelListItemDto[]>([]);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await fetchUserLikedModels(username, page, PAGE_SIZE);
      setModels(response.models);
      setTotalPages(response.totalPages);
      setTotalCount(response.totalCount);
    } catch {
      setError("Failed to load liked models.");
      setModels([]);
      setTotalCount(0);
    } finally {
      setLoading(false);
    }
  }, [username, page]);

  useEffect(() => {
    load();
  }, [load]);

  if (loading) {
    return (
      <ModelBrowseGrid layout={layout}>
        {Array.from({ length: PAGE_SIZE }).map((_, i) => (
          <ModelCardSkeleton key={i} size={layout.cardSize} />
        ))}
      </ModelBrowseGrid>
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
      <ModelBrowseGrid layout={layout}>
        {models.map((model) => (
          <ModelCard
            key={model.id}
            model={mapToModel(model, model.author?.username ?? "unknown")}
            size={layout.cardSize}
          />
        ))}
      </ModelBrowseGrid>
      {totalPages > 1 && (
        <SimplePagination
          page={page}
          totalPages={totalPages}
          totalCount={totalCount}
          onPageChange={onPageChange}
        />
      )}
    </div>
  );
}
