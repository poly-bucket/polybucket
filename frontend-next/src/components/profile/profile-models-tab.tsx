"use client";

import { useEffect, useState, useCallback } from "react";
import { ModelBrowseModel, ModelBrowseModelSkeleton } from "@/components/models/model-browse-model";
import { ModelBrowseGrid } from "@/components/models/model-browse-grid";
import { useModelBrowseLayout } from "@/lib/layout/use-model-browse-layout";
import { SimplePagination } from "@/components/collections/simple-pagination";
import { fetchUserModels } from "@/lib/services/userProfileService";
import type { Model } from "@/lib/api/client";
import type { UserModelListItemDto } from "@/lib/api/client";

const PAGE_SIZE = 12;

interface ProfileModelsTabProps {
  username: string;
  profileUsername: string;
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

export function ProfileModelsTab({
  username,
  profileUsername,
  page,
  onPageChange,
}: ProfileModelsTabProps) {
  const layout = useModelBrowseLayout();
  const [models, setModels] = useState<UserModelListItemDto[]>([]);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadModels = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await fetchUserModels(username, page, PAGE_SIZE);
      setModels(response.models ?? []);
      setTotalPages(response.totalPages ?? 1);
      setTotalCount(response.totalCount ?? 0);
    } catch (err) {
      const message =
        (err as { result?: { message?: string }; message?: string })?.result?.message ??
        (err as { message?: string })?.message ??
        "Failed to load models";
      setError(message);
      setModels([]);
    } finally {
      setLoading(false);
    }
  }, [username, page]);

  useEffect(() => {
    loadModels();
  }, [loadModels]);

  if (loading) {
    return (
      <ModelBrowseGrid layout={layout}>
        {Array.from({ length: PAGE_SIZE }).map((_, i) => (
          <ModelBrowseModelSkeleton key={i} layout={layout} />
        ))}
      </ModelBrowseGrid>
    );
  }

  if (models.length === 0) {
    return (
      <div className="rounded-xl border border-white/20 bg-white/5 px-8 py-12 text-center">
        <p className="text-white/60">{error ?? "No models found"}</p>
      </div>
    );
  }

  const modelsForCard = models.map((m) => mapToModel(m, profileUsername));

  return (
    <div>
      <ModelBrowseGrid layout={layout}>
        {modelsForCard.map((model) => (
          <ModelBrowseModel key={model.id} model={model} layout={layout} />
        ))}
      </ModelBrowseGrid>
      <SimplePagination
        page={page}
        totalPages={totalPages}
        totalCount={totalCount}
        onPageChange={onPageChange}
      />
    </div>
  );
}
